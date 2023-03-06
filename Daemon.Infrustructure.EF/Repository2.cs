using Daemon.Infrustructure.Contract;
using Daemon.Model;
using Daemon.Model.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using Daemon.Common.Query.Framework;
using Daemon.Infrustructure.EF.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Daemon.Common.ExpressionHelper;
using System.Data.SqlClient;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Daemon.Common.Const;
using System.Text;
using Daemon.Data.Substructure.Enums;
namespace Daemon.Infrustructure.EF
{
    public abstract partial class Repository<TEntity, TK> : RepositorySubstructure<TEntity>, IRepository<TEntity, TK>
    where TEntity : class, new()
    {
        public virtual MassUpdateResult MassUpdate(MassUpdateSettings massUpdateSettings)
        {
            var ids = massUpdateSettings.Ids.Select(r => (TK)Convert.ChangeType(r, typeof(TK))).ToList();
            var list = GetDbQuery().Contains(ids, PrimaryKeyExpression).ToList();
            var keyFunc = PrimaryKeyExpression.Compile();
            Func<TEntity, int> intKeyFunc = i => Convert.ToInt32(keyFunc(i));
            var context = new MassUpdateContext<TEntity>(massUpdateSettings, list, intKeyFunc, this.Context.GetMapping<TEntity>());
            MassUpdate(context);
            return context.Result;
        }

        #region Mass update protected methods

        protected virtual void SaveMassUpdateValidEntities(MassUpdateContext<TEntity> massUpdateContext, DbUpdateException ex)
        {
            List<TK> tempList = ex.Entries.Select(r => (TEntity)r.Entity).Select(PrimaryKeyExpression.Compile()).ToList();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            Expression binaryExpression = ExpressionHelper.GetQueryExpression<TK>(PrimaryKeyExpression.Body, parameterExpression, tempList, LinqOperatorEnum.Contains);
            Expression<Func<TEntity, bool>> queryExpresson = Expression.Lambda<Func<TEntity, bool>>(binaryExpression, parameterExpression);

            List<EntityEntry<TEntity>> allChangeDatas = this.Context.ChangeTracker.Entries<TEntity>().ToList();
            List<TEntity> allEntities = allChangeDatas.Select(r => r.Entity).Where(queryExpresson.Compile()).ToList();
            List<EntityEntry<TEntity>> allInvalidDatas = allChangeDatas.Where(r => allEntities.Contains(r.Entity)).ToList();
            allInvalidDatas.ForEach(r => r.State = EntityState.Unchanged);

            this.Context.SaveChanges();

            List<TEntity> invalidEntities = allInvalidDatas.Select(r => r.Entity).ToList();
            massUpdateContext.MarkFailed(invalidEntities);
            List<TEntity> validEntities = allChangeDatas.Where(r => !allEntities.Contains(r.Entity)).Select(r => r.Entity).ToList();
            massUpdateContext.MarkUpdated(validEntities);
        }

        protected virtual bool IsSimpleMassUpdate(MassUpdateContext<TEntity> massUpdateContext)
        {
            return false;
        }

        protected virtual void MassUpdate(MassUpdateContext<TEntity> massUpdateContext)
        {
            var dbcontext = this.Context;
            var settings = massUpdateContext.Settings;
            var originalIds = settings.Ids.ToList();
            ProcessIgnoreData(massUpdateContext, dbcontext);
            if (!settings.Ids.Any())
            {
                massUpdateContext.Result.IgnoredIds.AddRange(originalIds);
                return;
            }

            var list = massUpdateContext.Entities;
            var originalList = Copy(list);
            if (settings.Ids.Count != originalIds.Count)
            {
                var ignoredIds = originalIds.Where(o => !settings.Ids.Contains(o));
                massUpdateContext.Result.IgnoredIds.AddRange(ignoredIds);
            }

            var isUserInputUDFVal = settings.TargetField != null && settings.TargetField.Equals("UserDefinedFields", StringComparison.OrdinalIgnoreCase);

            var properties = massUpdateContext.PropertyColumnMap;
            var patchData = CreateMassUpdatePatchData(settings);
            var isSimple = IsSimpleMassUpdate(massUpdateContext);
            if (!isSimple)
            {
                var specialPatchData = new ConcurrentBag<JsonPatchData>();
                var dict = patchData.GroupBy(r => r.Id).ToDictionary(r => r.Key, r => r.ToList());
                Parallel.ForEach(list, item =>
                {
                    if (!dict.TryGetValue(massUpdateContext.GetId(item).ToString(), out var patchDataForItem))
                    {
                        return;
                    }

                    var lstJsonPatchData = GetSpecialFieldsAndReplaceByField(patchDataForItem, item, properties);
                    lstJsonPatchData.ForEach(r => specialPatchData.Add(r));
                    GetJsonPatchDocument(patchDataForItem).ApplyTo(item);
                });

                patchData.AddRange(specialPatchData);
                UpdateEntitiesBeforePatch(list, originalList, patchData, dbcontext);
            }

            var lastUpdatedInfoSql = GetLastUpdatedInfoSql(EntityORMType.UPDATE_TYPE_NUMBER, properties);
            var (updateSqlBuffer, parameters) = CreateMassUpdateSql(settings, properties, patchData);

            var sql = string.Empty;
            updateSqlBuffer.Append(updateSqlBuffer.Length == 0 ? lastUpdatedInfoSql.TrimStart(',') : lastUpdatedInfoSql);
            if (updateSqlBuffer.Length > 0)
            {
                var mapping = massUpdateContext.TableMapping;
                sql = $"update [{mapping.TableName}] set {updateSqlBuffer} where [{GetPrimaryKeyColumnName(mapping)}] in ({{0}})";
            }

            if (RelationshipNames.Count > 0)
            {
                list.ForEach(entity => SaveRelationshipsForPatch(entity, patchData, dbcontext));
                dbcontext.SaveChanges();
            }

            MassUpdateSqlBulkPatch(dbcontext, massUpdateContext, list, sql, parameters);
            UpdateEntitiesAfterPatch(list, patchData, dbcontext);
        }

        protected virtual void ProcessIgnoreData(MassUpdateContext<TEntity> massUpdateContext, ApiDBContent context)
        {
        }

        protected void MassUpdateSqlBulkPatch(ApiDBContent context, MassUpdateContext<TEntity> massUpdateContext, List<int> ids, string sql)
        {
            MassUpdateSqlBulkPatch(context, massUpdateContext, ids, sql, new List<SqlParameter>());
        }

        protected void MassUpdateSqlBulkPatch(ApiDBContent context, MassUpdateContext<TEntity> massUpdateContext, List<int> ids, string sql, List<SqlParameter> paramenters)
        {
            if (ids.Count <= 0 || string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            try
            {
                string idsSql;
                if (ids.Count > 1)
                {
                    var hashKey = Guid.NewGuid().ToString();
                    var type = $"MassUpdate.{typeof(TEntity).Name}";
                    var now = DateTime.UtcNow;
                    var bulkRecords = ids.Select(r => new RecordIDFilter()
                    {
                        RecordID = r,
                        Hash = hashKey,
                        Type = type,
                        CreateDateTime = now,
                    }).ToList();
                    // ServiceLocator.GetInstance<RecordIDFilterRepository>().AddRange(bulkRecords);
                    idsSql = $"select recordId from _IdFilter where Hash = '{hashKey}'";
                }
                else
                {
                    idsSql = string.Join(",", ids);
                }

                sql = string.Format(sql, idsSql);
                context.Database.ExecuteSqlRaw(sql, paramenters.ToArray());

                massUpdateContext.MarkUpdated(ids);
            }
            catch (Exception ex)
            {
                massUpdateContext.MarkFailed(ids);
                massUpdateContext.Terminate(ex.Message);
            }
        }

        #endregion

        #region Mass update private methods

        protected string GetMeasurementUnitConversionParameter(string value, DataTypeEnum dataType, string sourceField, string targetField)
        {
            return value;
        }

        private (StringBuilder UpdateSqlBuffer, List<SqlParameter> Parameters) CreateMassUpdateSql(MassUpdateSettings settings, IDictionary<string, string> properties, List<JsonPatchData> patchData)
        {
            var parameters = new List<SqlParameter>();
            var updateSqlBuffer = new StringBuilder();
            if (!string.IsNullOrEmpty(settings.SourceField))
            {
                if (!properties.TryGetValue(settings.SourceField, out var sourceColumn))
                {
                    sourceColumn = settings.SourceField;
                }

                if (properties.TryGetValue(settings.TargetField, out var columnName))
                {
                    updateSqlBuffer.Append($"[{columnName}]=[{sourceColumn}]");

                    var value = GetMeasurementUnitConversionParameter(string.Empty, settings.DataType, sourceColumn, columnName);
                    updateSqlBuffer.Append(value);
                }
            }
            else
            {
                var groups = patchData.GroupBy(i => i.Path).Select(i => i.First());
                foreach (var group in groups)
                {
                    if (!properties.TryGetValue(@group.Path, out var columnName))
                    {
                        continue;
                    }

                    if (updateSqlBuffer.Length != 0)
                    {
                        updateSqlBuffer.Append(',');
                    }

                    parameters.Add(new SqlParameter("@" + columnName, @group.Value ?? (object)DBNull.Value));
                    updateSqlBuffer.Append($"[{columnName}]=@{columnName}");
                }
            }

            return (updateSqlBuffer, parameters);
        }

        private void MassUpdateSqlBulkPatch(ApiDBContent context, MassUpdateContext<TEntity> massUpdateContext, List<TEntity> bulkEntities, string sql, List<SqlParameter> paramenters)
        {
            var ids = bulkEntities.Select(PrimaryKeyExpression.Compile()).Select(r => Convert.ToInt32(r)).ToList();
            MassUpdateSqlBulkPatch(context, massUpdateContext, ids, sql, paramenters);
        }

        private List<JsonPatchData> CreateMassUpdatePatchData(MassUpdateSettings massUpdateSettings)
        {
            var result = new List<JsonPatchData>();
            if (massUpdateSettings.TargetUdfId.HasValue || massUpdateSettings.SourceUdfId.HasValue)
            {
                return result;
            }

            var hasRelationship = RelationshipNames.Any();
            var operation = hasRelationship ? BaseOperationType.REPLACTIONSHIP : BaseOperationType.REPLACE;
            foreach (var id in massUpdateSettings.Ids)
            {
                var data = new JsonPatchData
                {
                    Id = id.ToString(),
                    Path = massUpdateSettings.TargetField,
                    Value = massUpdateSettings.Value?.ToString(),
                    Op = operation,
                    From = massUpdateSettings.SourceField,
                };

                result.Add(data);
            }

            return result;
        }

        private IEnumerable<JsonPatchData> GetSpecialFieldsAndReplaceByField(List<JsonPatchData> list, TEntity entity, IDictionary<string, string> tableFieldDic)
        {
            var lstJsonPatchData = new List<JsonPatchData>();
            foreach (var jsonPatchData in list)
            {
                if (string.IsNullOrWhiteSpace(jsonPatchData.From))
                {
                    lstJsonPatchData = AddSpecialField(jsonPatchData);
                    continue;
                }

                if (tableFieldDic.ContainsKey(jsonPatchData.From) && string.IsNullOrWhiteSpace(jsonPatchData.Value))
                {
                    jsonPatchData.Value = entity.GetType().GetProperty(jsonPatchData.From).GetValue(entity, null)?.ToString();
                }
            }

            return lstJsonPatchData;
        }
        #endregion
    }
}

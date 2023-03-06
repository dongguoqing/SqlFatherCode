using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Model;
using Daemon.Infrustructure.Contract;
using Microsoft.EntityFrameworkCore;
using Daemon.Common.ExpressionHelper;
using Daemon.Common;
using Daemon.Common.Helpers;
using Daemon.Model.Entities;
using Daemon.Common.Middleware;
using Daemon.Common.Query.Framework;
using Newtonsoft.Json;
using Daemon.Common.Filter;
using Daemon.Common.Const;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Daemon.Data.Substructure.Helpers;
using Microsoft.AspNetCore.JsonPatch;
using Daemon.Common.Extend;
using Daemon.Common.Executer;
using Daemon.Infrustructure.EF.Framework;
using Daemon.Data.Infrastructure.Auth;
using System.Text;
using Daemon.Infrustructure.EF.SqlScripts;
using Daemon.Common.Exceptions;
using Daemon.Model.Entities.DBContextExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Daemon.Infrustructure.EF
{
    public abstract class RepositorySubstructure<TEntity> : IRepository<TEntity>, IDisposable
    where TEntity : class, new()
    {
        protected ApiDBContent Context;
        protected DbSet<TEntity> DbSet;
        private IAuthInfo _authInfo;

        private Dictionary<string, PropertyInfo> _entityProperties;

        protected Dictionary<string, PropertyInfo> EntityProperties
        {
            get
            {
                if (_entityProperties == null)
                {
                    _entityProperties = typeof(TEntity).GetProperties().ToDictionary(r => r.Name, r => r);
                }

                return _entityProperties;
            }
        }

        private List<string> _relationshipNames = null;

        public RepositorySubstructure(ApiDBContent context)
        {
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }

        protected virtual List<string> RelationshipNames
        {
            get
            {
                if (_relationshipNames == null)
                {
                    if (Relationships != null)
                    {
                        _relationshipNames = Relationships.Select(r => r.Name).ToList();
                    }
                    else
                    {
                        _relationshipNames = new List<string>();
                    }
                }

                return _relationshipNames;
            }
        }

        private List<Relationship> _relationships = null;

        protected virtual List<Relationship> Relationships
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = ServiceLocator.Resolve<RelationshipExecutor>().GetRelationships();

                    if (_relationships == null)
                    {
                        _relationships = new List<Relationship>();
                    }
                }

                return _relationships;
            }
        }

        public IAuthInfo AuthInfo
        {
            get
            {
                // if (_authInfo == null)
                // {
                //     var sign = this.Context.BlogSysInfo.FirstOrDefault(user => user.InfoId == "sign")?.InfoValue;
                //     var httpContext = ServiceLocator._applicationServices.GetRequiredService<HttpContextAccessor>().HttpContext;
                //     var token = httpContext?.Request.Query["token"].FirstOrDefault();
                //     if (string.IsNullOrEmpty(token))
                //         token = httpContext?.Request.Headers["token"].FirstOrDefault();
                //     try
                //     {
                //         var userJson = JwtHelper.ValidateJwtToken(token, sign);
                //         _authInfo = JsonConvert.DeserializeObject<IAuthInfo>(userJson);
                //     }
                //     catch
                //     {

                //     }
                // }

                return _authInfo;
            }

            set
            {
                _authInfo = value;
            }
        }

        protected virtual Expression<Func<TEntity, object>> DuplicateFieldExpression => null;

        protected virtual Dictionary<string, string> FieldAlias => new Dictionary<string, string>();

        public virtual bool HasRelationships()
        {
            return RelationshipNames.Any();
        }

        public IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, short? associatedDatabaseId = null)
        {
            return IncludeRelationships(entities, this.Context, associatedDatabaseId);
        }

        public int TotalCount()
        {
            return DbSet.AsNoTracking().Count();
        }

        public virtual string GetPrimaryKeys()
        {
            return string.Empty;
        }

        public Func<string, string> GetFieldAliasFunc()
        {
            return name => FieldAlias.TryGetValue(name, out string alias) ? alias : name;
        }

        public int GetAliasCount()
        {
            return FieldAlias.Count;
        }

        public void Execute(string sql, params object[] parameters)
        {
            Execute(sql, this.Context, parameters);
        }

        public IQueryable<TEntity> IncludeRelationships(IQueryable<TEntity> entities, ApiDBContent context, short? associatedDatabaseId = null)
        {
            if (RelationshipNames.Count == 0 || entities == null || !entities.Any())
            {
                return entities;
            }

            if (associatedDatabaseId.HasValue)
            {
                return IncludeRelationships(entities.ToList(), context, associatedDatabaseId.Value).AsQueryable();
            }
            else
            {
                return IncludeRelationships(entities.ToList(), context).AsQueryable();
            }
        }

        public virtual void SetRelationships(List<Relationship> relationships)
        {
            if (relationships == null)
            {
                relationships = new List<Relationship>();
            }

            _relationships = relationships;

            _relationshipNames = relationships.Select(r => r.Name).ToList();
        }

        protected virtual IQueryable<TEntity> GetDbQuery(ApiDBContent context, bool isNoTracking = true)
        {

            IQueryable<TEntity> dbquery = isNoTracking ? DbSet.AsNoTracking() : DbSet;

            if (AuthInfo != null)
            {
                return FilterByAuthInfo(dbquery, context);
            }

            return dbquery;
        }

        protected virtual IQueryable<TEntity> GetDbQuery(bool isNoTracking = true)
        {
            return GetDbQuery(this.Context, isNoTracking);
        }

        protected IEnumerable<dynamic> SelectQueryWithRelationships(IQueryable<TEntity> queryable, Relationship relationship, ApiDBContent db, List<string> needSelectFields = null, List<string> emptyFields = null)
        {
            IEnumerable<dynamic> result;

            if (relationship != null)
            {
                SetRelationships(relationship?.Relationships);

                queryable = IncludeRelationships(queryable, db);

                if (!string.IsNullOrEmpty(relationship.SelectFields))
                {
                    string key = GetGroupKeys();

                    var fields = relationship.SelectFields.Split(',');

                    if (needSelectFields != null)
                    {
                        fields = fields.Union(needSelectFields).ToArray();
                    }

                    fields = fields.Union(new string[1] { key }).ToArray();

                    result = queryable.Select(fields, GetFieldAliasFunc(), emptyFields).ToList();
                }
                else
                {
                    var list = queryable.ToList();
                    result = list.Select(r => (dynamic)r);
                }
            }
            else
            {
                var list = queryable.ToList();
                result = list.Select(r => (dynamic)r);
            }

            return result;
        }

        protected IEnumerable<TEntity> QueryWithRelationship<TRelationship>(IQueryable<TEntity> queryable, TRelationship relationship, ApiDBContent db)
            where TRelationship : Enum
        {
            return QueryWithRelationships(queryable, new Relationship { Relationships = new List<Relationship> { new Relationship { Name = relationship.ToString() } } }, db);
        }

        protected IEnumerable<TEntity> QueryWithRelationships(IQueryable<TEntity> queryable, Relationship relationship, ApiDBContent db)
        {
            if (relationship != null)
            {
                SetRelationships(relationship?.Relationships);

                queryable = IncludeRelationships(queryable, db);
            }

            return queryable;
        }

        protected virtual void SetUnmodifiedFields(TEntity entity, ApiDBContent db)
        {
            if (EntityProperties.ContainsKey("CreatedBy"))
            {
                db.Entry(entity).Property("CreatedBy").IsModified = false;
            }

            if (EntityProperties.ContainsKey("CreatedOn"))
            {
                db.Entry(entity).Property("CreatedOn").IsModified = false;
            }

            if (EntityProperties.ContainsKey("CreateDateTime"))
            {
                db.Entry(entity).Property("CreateDateTime").IsModified = false;
            }
        }

        protected virtual void SetLastUpdatedInfo(TEntity entity, short type)
        {
            var utc = DateTime.UtcNow;

            if (EntityProperties.ContainsKey("LastUpdated"))
            {
                EntityProperties["LastUpdated"].SetValue(entity, utc);
            }

            if (EntityProperties.ContainsKey("LastUpdatedType"))
            {
                EntityProperties["LastUpdatedType"].SetValue(entity, type);
            }

            if (EntityProperties.ContainsKey("LastUpdatedOn"))
            {
                EntityProperties["LastUpdatedOn"].SetValue(entity, utc);
            }

            if (type == EntityORMType.ADD_TYPE_NUMBER)
            {
                if (EntityProperties.ContainsKey("CreatedOn"))
                {
                    EntityProperties["CreatedOn"].SetValue(entity, utc);
                }

                if (EntityProperties.ContainsKey("CreateDateTime"))
                {
                    EntityProperties["CreateDateTime"].SetValue(entity, utc);
                }
            }

            if (AuthInfo == null)
            {
                return;
            }

            if (EntityProperties.ContainsKey("LastUpdatedId"))
            {
                EntityProperties["LastUpdatedId"].SetValue(entity, AuthInfo.UserId);
            }

            if (EntityProperties.ContainsKey("LastUpdatedID"))
            {
                EntityProperties["LastUpdatedID"].SetValue(entity, AuthInfo.UserId);
            }

            if (EntityProperties.ContainsKey("LastUpdatedBy"))
            {
                EntityProperties["LastUpdatedBy"].SetValue(entity, AuthInfo.UserId);
            }

            if (type == EntityORMType.ADD_TYPE_NUMBER)
            {
                if (EntityProperties.ContainsKey("CreatedBy"))
                {
                    EntityProperties["CreatedBy"].SetValue(entity, AuthInfo.UserId);
                }
            }
        }

        protected string GetLastUpdatedInfoSql(short type, IDictionary<string, string> columnMappingDict)
        {
            if (AuthInfo == null)
            {
                return string.Empty;
            }

            var sqlBuffer = new StringBuilder();
            var utc = DateTime.UtcNow;
            Action<string[], object> setValue = (propertyNames, value) =>
            {
                foreach (var propertyName in propertyNames)
                {
                    if (columnMappingDict.TryGetValue(propertyName, out string column))
                    {
                        if (value is DateTime)
                        {
                            sqlBuffer.Append($",{column}='{value}'");
                        }
                        else
                        {
                            sqlBuffer.Append($",{column}={value}");
                        }

                        return;
                    }
                }
            };

            setValue(new[] { "LastUpdated", "LastUpdatedOn" }, utc);
            setValue(new[] { "LastUpdatedId", "LastUpdatedBy" }, AuthInfo.UserId);
            setValue(new[] { "LastUpdatedType" }, type);

            if (type == EntityORMType.ADD_TYPE_NUMBER)
            {
                setValue(new[] { "CreatedOn", "CreateDateTime" }, utc);
                setValue(new[] { "CreatedBy" }, AuthInfo.UserId);
            }

            return sqlBuffer.ToString();
        }

        protected virtual void SetLastUpdatedInfo<TEntity>(TEntity entity, short type)
        {
            if (AuthInfo == null)
            {
                return;
            }

            var utc = DateTime.UtcNow;

            Dictionary<string, PropertyInfo> properties = typeof(TEntity).GetProperties().ToDictionary(r => r.Name, r => r);

            if (properties.ContainsKey("LastUpdated"))
            {
                properties["LastUpdated"].SetValue(entity, utc);
            }

            if (properties.ContainsKey("LastUpdatedId"))
            {
                properties["LastUpdatedId"].SetValue(entity, AuthInfo.UserId);
            }

            if (properties.ContainsKey("LastUpdatedID"))
            {
                properties["LastUpdatedID"].SetValue(entity, AuthInfo.UserId);
            }

            if (properties.ContainsKey("LastUpdatedType"))
            {
                properties["LastUpdatedType"].SetValue(entity, type);
            }

            if (properties.ContainsKey("LastUpdatedBy"))
            {
                properties["LastUpdatedBy"].SetValue(entity, AuthInfo.UserId);
            }

            if (properties.ContainsKey("LastUpdatedOn"))
            {
                properties["LastUpdatedOn"].SetValue(entity, utc);
            }

            if (type == EntityORMType.ADD_TYPE_NUMBER)
            {
                if (properties.ContainsKey("CreatedBy"))
                {
                    properties["CreatedBy"].SetValue(entity, AuthInfo.UserId);
                }

                if (properties.ContainsKey("CreatedOn"))
                {
                    properties["CreatedOn"].SetValue(entity, utc);
                }

                if (properties.ContainsKey("CreateDateTime"))
                {
                    properties["CreateDateTime"].SetValue(entity, utc);
                }
            }
        }

        protected virtual string GetGroupKeys()
        {
            return GetPrimaryKeys();
        }

        protected virtual IEnumerable<TEntity> IncludeRelationships(IEnumerable<TEntity> entities, ApiDBContent db)
        {
            return entities;
        }

        protected virtual IEnumerable<TEntity> IncludeRelationships(IEnumerable<TEntity> entities, ApiDBContent db, short associatedDatabaseId)
        {
            return entities;
        }

        /// <summary>
        /// Get entity collection.
        /// </summary>
        /// <param name="sql">The sql script of query.</param>
        /// <param name="db">The db context.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>Returns the entity collection.</returns>
        protected virtual IEnumerable<TEntity> GetEntityCollection(string sql, ApiDBContent db, params object[] parameters)
        {
            return DbSet.FromSqlRaw<TEntity>(sql, parameters);
        }

        /// <summary>
        /// Get entity collection.
        /// </summary>
        /// <param name="sql">The sql script of query.</param>
        /// <param name="db">The db context.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>Returns the entity collection.</returns>
        protected virtual IEnumerable<TR> GetFieldCollection<TR>(string sql, ApiDBContent db, params object[] parameters)
        {
            return this.Context.RawSqlQuery<TR>(sql, parameters, r => default(TR));
        }

        /// <summary>
        /// Get entity collection.
        /// </summary>
        /// <param name="sql">The sql script of query.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>Returns the entity collection.</returns>
        protected virtual IEnumerable<TR> GetFieldCollection<TR>(string sql, params object[] parameters)
        {
            return GetFieldCollection<TR>(sql, this.Context, parameters);
        }

        /// <summary>
        /// Execute sql.
        /// </summary>
        /// <param name="sql">The sql script of query.</param>
        /// <param name="db">The db context.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>Returns the count of execute.</returns>
        protected virtual int Execute(string sql, ApiDBContent db, params object[] parameters)
        {
            return db.Database.ExecuteSqlRaw(sql, parameters);
        }

        protected virtual Expression<Func<TEntity, bool>> GetExpressionByAuthInfo(ApiDBContent context)
        {
            return r => true;
        }

        protected virtual IQueryable<TEntity> FilterByAuthInfo(IQueryable<TEntity> queryable, ApiDBContent context)
        {
            return queryable.Where(GetExpressionByAuthInfo(context));
        }

        protected JsonPatchDocument GetJsonPatchDocument(List<JsonPatchData> patchData)
        {
            JsonPatchDocument patch = new JsonPatchDocument();
            foreach (var data in patchData)
            {
                switch (data.Op)
                {
                    case BaseOperationType.ADD:
                        patch.Add(data.Path, GetOrignalPatchData(data));
                        break;
                    case BaseOperationType.REMOVE:
                        patch.Remove(data.Path);
                        break;
                    case BaseOperationType.REPLACE:
                        patch.Replace(data.Path, GetOrignalPatchData(data));
                        break;
                    case BaseOperationType.MOVE:
                        patch.Move(data.From, data.Path);
                        break;
                }
            }

            return patch;
        }

        protected Expression<Func<TEntity, bool>> GetQueryExpression<TV>(Expression<Func<TEntity, TV>> expression, TV field, LinqOperatorEnum linqOperatorEnum = LinqOperatorEnum.Equal)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));

            Expression binaryExpression = ExpressionHelper.GetQueryExpression(expression.Body, parameterExpression, field, linqOperatorEnum);

            return Expression.Lambda<Func<TEntity, bool>>(binaryExpression, parameterExpression);
        }

        private object GetOrignalPatchData(JsonPatchData data)
        {
            Type type = typeof(TEntity).GetProperty(data.Path.TrimStart('/')).PropertyType;
            if (type.IsArray)
            {
                var typeCode = Type.GetTypeCode(type.GetElementType());
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        return ByteArrayEncodingHelper.Decode(data.Value);
                }
            }

            return data.Value;
        }

        public void Dispose()
        {
            this.Context.Dispose();
            this._relationshipNames = null;
            this._relationships = null;
        }
    }

    public abstract partial class Repository<TEntity, TK> : RepositorySubstructure<TEntity>, IRepository<TEntity, TK>, IDisposable
        where TEntity : class, new()
    {
        protected ApiDBContent Context;

        protected IBaseSqlScriptCollection<TEntity> BaseSqlScriptCollection { get; set; }

        protected abstract Expression<Func<TEntity, TK>> PrimaryKeyExpression { get; }

        public Repository(ApiDBContent context) : base(context)
        {
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> FindAll()
        {
            return GetDbQuery();
        }

        public virtual TEntity FindById(TK id)
        {
            return GetDbQuery().FirstOrDefault(GetQueryExpression(PrimaryKeyExpression, id));
        }

        public virtual IQueryable<TEntity> FindByIds(List<TK> ids)
        {
            return GetDbQuery().Contains(ids, PrimaryKeyExpression);
        }

        public virtual TEntity Add(TEntity entity)
        {
            return Add(entity, this.Context);
        }

        public TEntity AddWithRelationships(TEntity entity)
        {
            TEntity result;

            ApiDBContent dbcontext = this.Context;

            CheckDuplicateField(entity, dbcontext);

            if (RelationshipNames.Count > 0)
            {
                TEntity relationshipEntity = new TEntity();

                ModifyRelationshipEntity(entity, relationshipEntity);

                TEntity resultEntity = AddEntity(entity, dbcontext);

                SaveRelationships(resultEntity, relationshipEntity, dbcontext, EntityORMType.ADD_TYPE_NUMBER);

                dbcontext.SaveChanges();

                result = resultEntity;
            }
            else
            {
                InitRelationshipColumnProperties();

                ModifyRelationshipEntity(entity);

                result = Add(entity, dbcontext);
            }

            return result;
        }

        public virtual IEnumerable<TEntity> AddRangeWithRelationships(IEnumerable<TEntity> entities)
        {
            ApiDBContent dbcontext = this.Context;

            foreach (var entity in entities)
            {
                CheckDuplicateField(entity, dbcontext);
            }

            UpdateEntitiesBeforeCreate(entities, dbcontext);

            List<TEntity> result = new List<TEntity>();

            if (RelationshipNames.Count > 0)
            {
                foreach (var entity in entities)
                {
                    TEntity relationshipEntity = new TEntity();

                    ModifyRelationshipEntity(entity, relationshipEntity);

                    TEntity resultEntity = AddEntity(entity, dbcontext);

                    SaveRelationships(resultEntity, relationshipEntity, dbcontext, EntityORMType.ADD_TYPE_NUMBER);

                    dbcontext.SaveChanges();

                    result.Add(resultEntity);
                }
            }
            else
            {
                InitRelationshipColumnProperties();

                foreach (var entity in entities)
                {
                    ModifyRelationshipEntity(entity);
                }

                result.AddRange(BulkAdd(entities, dbcontext));
            }

            UpdateEntitiesAfterCreate(result, dbcontext);

            return result;
        }

        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
            return AddRange(entities, this.Context);
        }

        public TEntity Update(TEntity entity)
        {
            return Update(entity, this.Context);
        }

        public virtual TEntity UpdateWithRelationships(TEntity entity)
        {
            TEntity result;

            ApiDBContent dbcontext = this.Context;

            CheckExistedAndThrow(entity, dbcontext);

            CheckDuplicateField(entity, dbcontext, false);

            UpdateEntitiesBeforeUpdate(new List<TEntity> { entity }, dbcontext);

            ClearTracker(dbcontext, new List<TEntity> { entity }.Select(PrimaryKeyExpression.Compile()).ToList());

            if (RelationshipNames.Count > 0)
            {
                TEntity relationshipEntity = new TEntity();

                ModifyRelationshipEntity(entity, relationshipEntity);

                SaveRelationships(entity, relationshipEntity, dbcontext, EntityORMType.UPDATE_TYPE_NUMBER);

                result = UpdateEntity(entity, dbcontext, false);
            }
            else
            {
                InitRelationshipColumnProperties();

                ModifyRelationshipEntity(entity);

                result = UpdateEntity(entity, dbcontext, false);
            }

            UpdateEntitiesAfterUpdate(new List<TEntity> { result }, dbcontext);

            return result;
        }

        public virtual IEnumerable<TEntity> UpdateRangeWithRelationships(IEnumerable<TEntity> entities)
        {
            ApiDBContent dbcontext = this.Context;

            foreach (var entity in entities)
            {
                CheckExistedAndThrow(entity, dbcontext);

                CheckDuplicateField(entity, dbcontext, false);
            }

            UpdateEntitiesBeforeUpdate(entities, dbcontext);

            List<TEntity> result = new List<TEntity>();

            ClearTracker(dbcontext, entities.Select(PrimaryKeyExpression.Compile()).ToList());

            if (RelationshipNames.Count > 0)
            {
                foreach (var entity in entities)
                {
                    TEntity relationshipEntity = new TEntity();

                    ModifyRelationshipEntity(entity, relationshipEntity);

                    SaveRelationships(entity, relationshipEntity, dbcontext, EntityORMType.UPDATE_TYPE_NUMBER);

                    TEntity resultEntity = UpdateEntity(entity, dbcontext, false);

                    result.Add(resultEntity);
                }
            }
            else
            {
                InitRelationshipColumnProperties();

                foreach (var entity in entities)
                {
                    ModifyRelationshipEntity(entity);
                }

                result.AddRange(BulkUpdate(entities, dbcontext));
            }

            UpdateEntitiesAfterUpdate(result, dbcontext);

            return result;
        }

        public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
        {
            return UpdateRange(entities, this.Context);
        }

        public virtual int Delete(TEntity entity)
        {
            return Delete(entity, this.Context);
        }

        public virtual int DeleteRange(IQueryable<TEntity> entities)
        {
            return DeleteRange(entities, this.Context);
        }

        public virtual int DeleteRange(IEnumerable<TEntity> entities)
        {
            return DeleteRange(entities.AsQueryable(), this.Context);
        }

        public virtual int DeleteRangeByIds(IEnumerable<TK> ids)
        {
            return BulkDeleteRange(GetDbQuery(this.Context, false).Where(GetEntitiesByIds(ids)), this.Context);
        }

        public virtual int DeleteRangeByIds(IEnumerable<TK> ids, ApiDBContent dataContext)
        {
            return BulkDeleteRange(GetDbQuery(dataContext, false).Where(GetEntitiesByIds(ids.ToList())), dataContext);
        }

        public IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData)
        {
            List<TEntity> result = new List<TEntity>();
            if (patchData.All(r => !string.IsNullOrEmpty(r.Id)))
            {
                List<TK> ids = patchData.Select(r => (TK)Convert.ChangeType(r.Id, typeof(TK))).ToList();
                List<TEntity> list = GetDbQuery().Contains(ids, PrimaryKeyExpression).ToList();
                return PatchRange(patchData, list, Copy(list), (Expression<Func<TEntity, string>>)new IdToStringVisitor<TEntity>().Visit(PrimaryKeyExpression));
            }

            IQueryable<TEntity> queryable = GetDbQuery(), queryableForFilter = queryable.Filter().ColumnQuery();

            if (queryable == queryableForFilter)
            {
                return result;
            }

            return PatchRange(patchData, queryableForFilter.ToList());
        }

        public TEntity Patch(List<JsonPatchData> patchData, TK id)
        {
            TEntity entity = GetDbQuery().FirstOrDefault(GetQueryExpression(PrimaryKeyExpression, id));
            if (entity == null)
            {
                return entity;
            }

            if (RelationshipNames.Count > 0)
            {
                SaveRelationshipsForPatch(entity, patchData, this.Context);
            }

            JsonPatchDocument patch = GetJsonPatchDocument(patchData);
            patch.ApplyTo(entity);

            UpdateEntitiesBeforePatch(entity, Copy(entity), patchData, this.Context);
            this.Context.Entry(entity).State = EntityState.Modified;
            BulkPatch(entity, this.Context, new List<TEntity>() { entity }, 1);
            UpdateEntitiesAfterPatch(new List<TEntity> { entity }, patchData, this.Context);

            return entity;
        }

        public IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData, List<TK> ids)
        {
            List<TEntity> list = GetDbQuery().Contains(ids, PrimaryKeyExpression).Filter().ColumnQuery().ToList();
            return PatchRange(patchData, list);
        }

        public override string GetPrimaryKeys()
        {
            MemberExpression memberExpression;

            if (PrimaryKeyExpression.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = PrimaryKeyExpression.Body as MemberExpression;
            }

            return memberExpression?.Member?.Name;
        }

        public IEnumerable<TEntity> UpdateRangeByClientKey(IEnumerable<TEntity> entities, string clientKey)
        {
            return UpdateRange(entities, this.Context);
        }

        public TEntity AddByDBContext(TEntity entity, ApiDBContent context)
        {
            return Add(entity, context);
        }

        public int DeleteByDBContext(TEntity entity, ApiDBContent context)
        {
            return Delete(entity, context);
        }

        public IQueryable<TEntity> GetDbQueryAsNoTracking(ApiDBContent context = null)
        {
            if (context != null)
            {
                return GetDbQuery(context, true);
            }

            return GetDbQuery(true);
        }

        protected IEnumerable<TEntity> AddRangeByDBContext(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            return AddRange(entities, context);
        }

        protected TEntity UpdateByDBContext(TEntity entity, ApiDBContent context)
        {
            return Update(entity, context);
        }

        protected IEnumerable<TEntity> UpdateRangeByDBContext(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            return UpdateRange(entities, context);
        }

        protected int DeleteRangeByDBContext(IQueryable<TEntity> entities, ApiDBContent context)
        {
            return DeleteRange(entities, context);
        }

        protected int DeleteRangeByDBContext(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            return DeleteRange(entities.AsQueryable(), context);
        }

        protected override IQueryable<TEntity> GetDbQuery(ApiDBContent context, bool isNoTracking = true)
        {
            return base.GetDbQuery(context, isNoTracking);
        }

        protected override IQueryable<TEntity> GetDbQuery(bool isNoTracking = true)
        {
            var query = base.GetDbQuery(isNoTracking);
            return SelectEntitiesBeforeSelect(query, this.Context);
        }

        protected virtual IQueryable<TEntity> SelectEntitiesBeforeSelect(IQueryable<TEntity> entities, ApiDBContent context)
        {
            return entities;
        }

        protected IEnumerable<TEntity> AddRangeWithRelationships(IEnumerable<TEntity> entities, List<Relationship> relationships)
        {
            SetRelationships(relationships);
            return AddRangeWithRelationships(entities);
        }

        protected IEnumerable<TEntity> UpdateRangeWithRelationships(IEnumerable<TEntity> entities, List<Relationship> relationships)
        {
            SetRelationships(relationships);
            return UpdateRangeWithRelationships(entities);
        }

        protected virtual List<TEntity> PreprocessAddResultDataInBulkUpdate(List<TEntity> entities)
        {
            return entities;
        }

        protected virtual TEntity Add(TEntity entity, ApiDBContent context)
        {
            CheckDuplicateField(entity, context);

            UpdateEntitiesBeforeCreate(new List<TEntity> { entity }, context);

            TEntity result = AddEntity(entity, context);

            UpdateEntitiesAfterCreate(new List<TEntity> { result }, context);

            return result;
        }

        protected virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            foreach (var entity in entities)
            {
                CheckDuplicateField(entity, context);
            }

            UpdateEntitiesBeforeCreate(entities, context);

            ClearTracker(context, entities.Select(PrimaryKeyExpression.Compile()).ToList());

            IEnumerable<TEntity> result = BulkAdd(entities, context);

            UpdateEntitiesAfterCreate(result, context);

            return result;
        }

        protected virtual TEntity Update(TEntity entity, ApiDBContent context)
        {
            CheckExistedAndThrow(entity, context);

            CheckDuplicateField(entity, context, false);

            UpdateEntitiesBeforeUpdate(new List<TEntity> { entity }, context);

            ClearTracker(context, new List<TEntity> { entity }.Select(PrimaryKeyExpression.Compile()).ToList());

            TEntity result = UpdateEntity(entity, context);

            UpdateEntitiesAfterUpdate(new List<TEntity> { result }, context);

            return result;
        }

        protected virtual IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            foreach (var entity in entities)
            {
                CheckExistedAndThrow(entity, context);

                CheckDuplicateField(entity, context, false);
            }

            UpdateEntitiesBeforeUpdate(entities, context);

            ClearTracker(context, entities.Select(PrimaryKeyExpression.Compile()).ToList());

            IEnumerable<TEntity> result = BulkUpdate(entities, context);

            UpdateEntitiesAfterUpdate(result, context);

            return result;
        }

        protected virtual int Delete(TEntity entity, ApiDBContent context)
        {
            IQueryable<TEntity> entities = RemoveLockEntities(new List<TEntity>() { entity }.AsQueryable());

            if (entities.Count() > 0)
            {
                GetRelatedRecordsBeforeDelete(entities, context);

                DeleteRelationships(entities, context);

                var tarckers = context.ChangeTracker.Entries().ToList();

                if (tarckers.Any(r => r.State != EntityState.Detached))
                {
                    context.SaveChanges();

                    ClearTrackingEntities(tarckers);
                }

                InitRelationshipColumnProperties();

                ModifyRelationshipEntity(entity);

                context.Entry(entity).State = EntityState.Deleted;

                int num = context.SaveChanges();

                UpdateEntitiesAfterDelete(entities, context);

                return num;
            }

            return 0;
        }

        protected virtual int DeleteRange(IQueryable<TEntity> entities, ApiDBContent context)
        {
            return BulkDeleteRange(entities, context);
        }

        protected virtual int ExqueteSqlEvery5000Ids(string sqlstring, List<int> ids, ApiDBContent context)
        {
            int num = 0;
            if (ids.Any())
            {
                foreach (var chunk in Utility.SplitList(ids, 5000))
                {
                    num += context.Database.ExecuteSqlRaw(sqlstring + $" in ({string.Join(",", chunk)})");
                }
            }

            return num;
        }

        protected virtual void CheckDuplicateField(TEntity entity, ApiDBContent context, bool isNew = true)
        {
            if (DuplicateFieldExpression == null)
            {
                return;
            }

            string fieldName = ExpressionHelper.GetMemberName(DuplicateFieldExpression.Body);

            PropertyInfo propertyInfo = entity.GetType().GetProperty(fieldName);

            object fieldValue = propertyInfo.GetValue(entity);

            Expression<Func<TEntity, bool>> expression = GetQueryExpression(DuplicateFieldExpression, fieldValue, LinqOperatorEnum.Equal);

            TEntity existedEntity = context.Set<TEntity>().AsNoTracking().FirstOrDefault(expression);

            if (existedEntity != null)
            {
                if (!isNew)
                {
                    string primaryKeyName = ExpressionHelper.GetMemberName(PrimaryKeyExpression.Body);

                    PropertyInfo keyPropertyInfo = entity.GetType().GetProperty(primaryKeyName);

                    string keyValue = keyPropertyInfo.GetValue(entity)?.ToString();

                    string key = keyPropertyInfo.GetValue(existedEntity)?.ToString();

                    if (keyValue == key)
                    {
                        return;
                    }
                }

                throw new ValueDuplicateException("The " + fieldName.ToLower() + " '" + fieldValue + "' already exists.");
            }
        }

        protected virtual void CheckExistedAndThrow(TEntity entity, ApiDBContent context)
        {
            if (!CheckExisted(entity, context))
            {
                throw new NonexistentEntityException();
            }
        }

        protected virtual bool CheckExisted(TEntity entity, ApiDBContent context)
        {
            var keyField = ExpressionHelper.GetMemberName(PrimaryKeyExpression.Body);

            var keyValue = GetFieldValue<TK>(entity, keyField);

            Expression<Func<TEntity, bool>> expression = GetQueryExpression(PrimaryKeyExpression, keyValue, LinqOperatorEnum.Equal);

            return context.Set<TEntity>().AsNoTracking().Any(expression);
        }

        protected virtual void UpdateEntitiesBeforeCreate(IEnumerable<TEntity> entities, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesAfterCreate(IEnumerable<TEntity> entities, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesBeforeUpdate(IEnumerable<TEntity> entities, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesAfterUpdate(IEnumerable<TEntity> entities, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesBeforePatch(TEntity entity, TEntity originalEntity, List<JsonPatchData> patchData, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesBeforePatch(List<TEntity> entities, List<TEntity> originalEntities, List<JsonPatchData> patchData, ApiDBContent context)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                UpdateEntitiesBeforePatch(entities[i], originalEntities[i], patchData, context);
            }
        }

        protected virtual void UpdateEntitiesAfterPatch(IEnumerable<TEntity> entities, List<JsonPatchData> patchData, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntitiesAfterDelete(IEnumerable<TEntity> entities, ApiDBContent context)
        {
        }

        protected virtual void SaveRelationships(TEntity entity, TEntity entityWithRelationships, ApiDBContent db, short entityORMType)
        {
        }

        protected virtual void GetRelatedRecordsBeforeDelete(IQueryable<TEntity> entities, ApiDBContent db)
        {
        }

        protected virtual IQueryable<TEntity> DeleteRelationships(IQueryable<TEntity> entities, ApiDBContent db)
        {
            var result = entities.ToList();
            DeleteRelationships(result, db);
            return result.AsQueryable();
        }

        // Change all override to use IQueryable method not IEnumerable in the further, if not any override method, delete it.
        protected virtual void DeleteRelationships(IEnumerable<TEntity> entities, ApiDBContent db)
        {
        }

        protected virtual IQueryable<TEntity> RemoveLockEntities(IQueryable<TEntity> entities)
        {
            return entities;
        }

        protected virtual void SaveRelationshipsForPatch(TEntity entity, List<JsonPatchData> patchData, ApiDBContent db)
        {
        }

        protected virtual List<JsonPatchData> AddSpecialField(JsonPatchData patchData)
        {
            return new List<JsonPatchData>();
        }

        protected Dictionary<TKeyType, TValueType> GetRelationshipDict<TDictEntity, TKeyType, TValueType, TE>(
            List<TEntity> entities,
            Func<TEntity, bool> whereEntityFunc,
            Func<TEntity, TKeyType> selectEntityFunc,
            Expression<Func<TDictEntity, bool>> whereExpression,
            Expression<Func<TDictEntity, TKeyType>> selectExpression,
            Expression<Func<TDictEntity, TValueType>> resultExpression,
            TE relationshipEnum,
            ApiDBContent context)
            where TDictEntity : class
        {
            Dictionary<TKeyType, TValueType> dict = new Dictionary<TKeyType, TValueType>();

            if (RouteParameterHelper.CheckRouteParameter(RelationshipNames, relationshipEnum))
            {
                List<TKeyType> ids = entities.Where(whereEntityFunc).Select(selectEntityFunc).ToList();

                dict = context.Set<TDictEntity>().Where(whereExpression).Contains(ids, selectExpression).SafeToDictionary(selectExpression.Compile(), resultExpression.Compile()) as Dictionary<TKeyType, TValueType>;
            }

            return dict;
        }

        protected Expression<Func<TEntity, bool>> GetQueryExpression<TV>(Expression<Func<TEntity, TV>> expression, IList<TV> fields, LinqOperatorEnum linqOperatorEnum)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));

            Expression binaryExpression = ExpressionHelper.GetQueryExpression(expression.Body, parameterExpression, fields, linqOperatorEnum);

            return Expression.Lambda<Func<TEntity, bool>>(binaryExpression, parameterExpression);
        }

        protected virtual IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData, List<TEntity> list, List<TEntity> orignalList, Expression<Func<TEntity, string>> keyExpression)
        {
            var dbcontext = this.Context;

            List<TEntity> result = new List<TEntity>();
            List<TEntity> bulkEntities = new List<TEntity>();
            Dictionary<string, List<JsonPatchData>> groups = patchData.GroupBy(r => r.Id).ToDictionary(r => r.Key, r => r.ToList());

            Dictionary<string, TEntity> entityDictionary = list.AsQueryable().GroupBy(keyExpression).ToDictionary(r => r.Key, r => r.FirstOrDefault());
            foreach (var group in groups)
            {
                GetJsonPatchDocument(group.Value).ApplyTo(entityDictionary[group.Key]);
            }

            UpdateEntitiesBeforePatch(list, orignalList, patchData, dbcontext);

            int batchSize = RelationshipNames.Count > 0 ? 1 : SystemConst.BULK_BATCH_SIZE;
            foreach (var group in groups)
            {
                TEntity entity = entityDictionary[group.Key];
                if (entity != null)
                {
                    if (RelationshipNames.Count > 0)
                    {
                        SaveRelationshipsForPatch(entity, group.Value, dbcontext);
                        dbcontext.SaveChanges();
                        ClearTrackingEntities(dbcontext.ChangeTracker.Entries());
                    }

                    bulkEntities.Add(entity);
                    dbcontext.Entry(entity).State = EntityState.Modified;
                    BulkPatch(entity, dbcontext, bulkEntities, batchSize);
                    result.Add(entity);
                }
            }

            if (bulkEntities.Count > 0)
            {
                dbcontext.SaveChanges();
            }

            UpdateEntitiesAfterPatch(result, patchData, dbcontext);
            return result;
        }

        protected TV GetFieldValue<TV>(TEntity entity, string field)
        {
            var keyPropertyInfo = entity.GetType().GetProperty(field);

            return (TV)Convert.ChangeType(keyPropertyInfo.GetValue(entity), typeof(TV));
        }

        protected IEnumerable<TEntity> PatchRange(List<JsonPatchData> patchData, List<TEntity> list)
        {
            List<TEntity> result = new List<TEntity>();
            List<TEntity> bulkEntities = new List<TEntity>();
            JsonPatchDocument patch = GetJsonPatchDocument(patchData);
            List<TK> ids = list.Select(PrimaryKeyExpression.Compile()).ToList();
            int batchSize = RelationshipNames.Count > 0 ? 1 : SystemConst.BULK_BATCH_SIZE;

            ids.ForEach(id =>
            {
                TEntity entity = list.FirstOrDefault(GetQueryExpression(PrimaryKeyExpression, id).Compile());
                if (RelationshipNames.Count > 0)
                {
                    SaveRelationshipsForPatch(entity, patchData, this.Context);
                }

                bulkEntities.Add(entity);
                patch.ApplyTo(entity);
                UpdateEntitiesBeforePatch(entity, Copy(entity), patchData, this.Context);

                this.Context.Entry(entity).State = EntityState.Modified;
                BulkPatch(entity, this.Context, bulkEntities, batchSize);
                result.Add(entity);
            });

            if (bulkEntities.Count > 0)
            {
                this.Context.SaveChanges();
            }

            UpdateEntitiesAfterPatch(result, patchData, this.Context);

            return result;
        }

        protected void BulkPatch(TEntity entity, ApiDBContent context, List<TEntity> bulkEntities, int batchSize)
        {
            try
            {
                SetLastUpdatedInfo(entity, EntityORMType.UPDATE_TYPE_NUMBER);
                SetUnmodifiedFields(entity, context);
                int count = bulkEntities.Count;
                if (count % batchSize == 0 && count > 0)
                {
                    context.SaveChanges();
                    bulkEntities.Clear();
                }
            }
            catch
            {
                bulkEntities.Remove(entity);
                throw;
            }
        }

        protected List<EntityEntry<TEntity>> GetDbEntityEntries<TV>(Expression<Func<TEntity, TV>> expression, List<TV> tempList, ApiDBContent context)
        {
            Expression<Func<TEntity, bool>> queryExpresson = GetQueryExpression(expression, tempList, LinqOperatorEnum.Contains);

            List<EntityEntry<TEntity>> allChangeDatas = context.ChangeTracker.Entries<TEntity>().ToList();

            List<TEntity> allEntities = allChangeDatas.Select(r => r.Entity).Where(queryExpresson.Compile()).ToList();

            return allChangeDatas.Where(r => allEntities.Contains(r.Entity)).ToList();
        }

        protected virtual void ModifyRelationshipEntity(TEntity entity)
        {
            foreach (var property in _relationshipProperties)
            {
                property.SetValue(entity, null);
            }
        }

        protected virtual void ModifyRelationshipEntity(TEntity entity, TEntity relationshipEntity)
        {
            InitRelationshipColumnProperties();

            foreach (var property in _columnProperties)
            {
                property.SetValue(relationshipEntity, property.GetValue(entity));

                if (_relationshipColumns.Contains(property.Name))
                {
                    property.SetValue(entity, null);
                }
            }
        }

        protected virtual int BulkDeleteRange(IQueryable<TEntity> entities, ApiDBContent context)
        {
            var entityList = RemoveLockEntities(entities);

            GetRelatedRecordsBeforeDelete(entityList, context);

            entityList = DeleteRelationships(entityList, context);

            context.SaveChanges();

            var ids = entities.Select(PrimaryKeyExpression).ToList();

            int num = 0;

            if (ids.Any())
            {
                var mapping = context.GetMapping<TEntity>();

                var primaryKey = GetPrimaryKeyColumnName(mapping);

                foreach (var chunk in Utility.SplitList(ids, 5000))
                {
                    num += context.Database.ExecuteSqlRaw($"delete from {mapping.TableName} where {primaryKey} in ({string.Join(",", chunk)})");
                }
            }

            UpdateEntitiesAfterDelete(entityList, context);

            return num;
        }

        protected List<TEntity> Copy(List<TEntity> source)
        {
            return source.AsParallel().Select(x =>
            {
                string cloneString = JsonConvert.SerializeObject(x);

                return JsonConvert.DeserializeObject<TEntity>(cloneString);
            }).ToList();
        }

        protected TEntity Copy(TEntity source)
        {
            return JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(source));
        }

        private TEntity AddEntity(TEntity entity, ApiDBContent context)
        {
            TEntity result = null;

            SetLastUpdatedInfo(entity, EntityORMType.ADD_TYPE_NUMBER);

            if (BaseSqlScriptCollection != null)
            {
                string sql = BaseSqlScriptCollection.GetInsertByEntity(entity);

                if (!string.IsNullOrEmpty(sql))
                {
                    result = GetEntityCollection(sql, context, BaseSqlScriptCollection.GetSqlParameters().ToArray()).FirstOrDefault();
                }
            }

            if (result == null)
            {
                context.Set<TEntity>().Add(entity);

                context.SaveChanges();

                result = entity;
            }

            return result;
        }

        private IEnumerable<TEntity> BulkAdd(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            List<TEntity> result = new List<TEntity>();

            List<TEntity> bulkEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                SetLastUpdatedInfo(entity, EntityORMType.ADD_TYPE_NUMBER);

                bulkEntities.Add(entity);

                int count = bulkEntities.Count;

                if (count % SystemConst.BULK_BATCH_SIZE == 0 && count > 0)
                {
                    result.AddRange(AddEntities(bulkEntities, context));

                    bulkEntities.Clear();
                }
            }

            if (bulkEntities.Count > 0)
            {
                result.AddRange(AddEntities(bulkEntities, context));
            }

            return result;
        }

        private IEnumerable<TEntity> AddEntities(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            if (BaseSqlScriptCollection != null)
            {
                string sql = BaseSqlScriptCollection.GetInsertByEntity(entities);

                if (!string.IsNullOrEmpty(sql))
                {
                    var result = GetEntityCollection(sql, context, BaseSqlScriptCollection.GetSqlParameters().ToArray()).ToList();

                    var currentContext = context as ApiDBContent;
                    return result;
                }
            }

            context.Set<TEntity>().AddRange(entities);

            context.SaveChanges();

            return entities;
        }

        private IEnumerable<TEntity> BulkUpdate(IEnumerable<TEntity> entities, ApiDBContent context)
        {
            List<TEntity> result = new List<TEntity>();

            List<TEntity> bulkEntities = new List<TEntity>();

            foreach (var entity in entities)
            {
                SetLastUpdatedInfo(entity, EntityORMType.UPDATE_TYPE_NUMBER);

                context.Entry(entity).State = EntityState.Modified;

                SetUnmodifiedFields(entity, context);

                bulkEntities.Add(entity);

                int count = bulkEntities.Count;

                if (count % SystemConst.BULK_BATCH_SIZE == 0 && count > 0)
                {
                    context.SaveChanges();

                    result.AddRange(bulkEntities);

                    bulkEntities.Clear();
                }
            }

            if (bulkEntities.Count > 0)
            {
                context.SaveChanges();

                bulkEntities = PreprocessAddResultDataInBulkUpdate(bulkEntities);
                result.AddRange(bulkEntities);
            }

            return FindByIds(bulkEntities.Select(PrimaryKeyExpression.Compile()).ToList());
        }

        private TEntity UpdateEntity(TEntity entity, ApiDBContent context, bool includeRelationships = false)
        {
            SetLastUpdatedInfo(entity, EntityORMType.UPDATE_TYPE_NUMBER);

            context.Entry(entity).State = EntityState.Modified;

            SetUnmodifiedFields(entity, context);

            context.SaveChanges();

            return includeRelationships ? FindById(PrimaryKeyExpression.Compile().Invoke(entity)) : entity;
        }

        private void ClearTracker(ApiDBContent context, List<TK> tempList)
        {
            List<EntityEntry<TEntity>> allInvalidDatas = GetDbEntityEntries(PrimaryKeyExpression, tempList, context);

            allInvalidDatas.ForEach(r => r.State = EntityState.Detached);
        }

        private List<string> _relationshipColumns = null;

        private IEnumerable<PropertyInfo> _relationshipProperties = null;

        private IEnumerable<PropertyInfo> _columnProperties = null;

        private void InitRelationshipColumnProperties()
        {
            if (_relationshipColumns == null)
            {
                _relationshipColumns = this.Context.ExtColumn<TEntity>().ToList();
            }

            if (_columnProperties == null)
            {
                List<string> edmColumns = this.Context.Column<TEntity>().ToList();

                _columnProperties = typeof(TEntity).GetProperties().Where(r => !edmColumns.Contains(r.Name) && r.SetMethod != null);
            }

            if (_relationshipProperties == null)
            {
                _relationshipProperties = _columnProperties.Where(c => _relationshipColumns.Contains(c.Name));
            }
        }

        private void ClearTrackingEntities(IEnumerable<EntityEntry> trackingEntities)
        {
            foreach (var trackingEntity in trackingEntities)
            {
                trackingEntity.State = EntityState.Detached;
            }
        }

        private string GetPrimaryKeyColumnName(TableMapping mapping)
        {
            var primaryKeyName = GetPrimaryKeys();
            var primaryKey = mapping.ColumnMappings.FirstOrDefault(x => x.PropertyName == primaryKeyName);
            return primaryKey.ColumnName;
        }

        private Expression<Func<TEntity, bool>> GetEntitiesByIds(IEnumerable<TK> ids)
        {
            var containsMethod = ids.GetType().GetMethod("Contains", new[] { typeof(TK) });

            var paramExpr = Expression.Parameter(typeof(TEntity));

            MemberExpression memberExp = Expression.Property(paramExpr, (PropertyInfo)((MemberExpression)PrimaryKeyExpression.Body).Member);

            var converted = Expression.Convert(memberExp, typeof(TK));

            return Expression.Lambda<Func<TEntity, bool>>(Expression.Call(Expression.Constant(ids), containsMethod, converted), paramExpr);
        }
    }

    public abstract class BaseSingleTableRepository<TEntity, TK> : RepositorySubstructure<TEntity>, IBaseSingleTableRepository<TEntity>
    where TEntity : class, new()
    {
        protected ApiDBContent Context;
        public BaseSingleTableRepository(ApiDBContent context) : base(context)
        {
            this.Context = context;
        }

        protected abstract Expression<Func<TEntity, TK>> PrimaryKeyExpression { get; }

        public virtual TEntity Find()
        {
            return FindCurrent();
        }

        public TEntity Update(TEntity entity)
        {
            TEntity existedEntity = FindCurrent();

            UpdateEntityBeforeUpdate(entity, this.Context);
            if (existedEntity != null)
            {
                PropertyInfo propertyInfo = typeof(TEntity).GetProperty((PrimaryKeyExpression.Body as MemberExpression).Member.Name);
                propertyInfo.SetValue(entity, propertyInfo.GetValue(existedEntity));
                SaveRelationships(entity, this.Context);
                this.Context.Entry(entity).State = EntityState.Modified;
                SetLastUpdatedInfo(entity, EntityORMType.UPDATE_TYPE_NUMBER);
                SetUnmodifiedFields(entity, this.Context);
            }
            else
            {
                SetLastUpdatedInfo(entity, EntityORMType.ADD_TYPE_NUMBER);
                DbSet.Add(entity);
            }

            this.Context.SaveChanges();
            UpdateEntityAfterUpdate(entity, Context);
            return entity;
        }

        public TEntity Patch(List<JsonPatchData> patchData)
        {
            JsonPatchDocument patch = GetJsonPatchDocument(patchData);

            TEntity existedEntity = FindCurrent();

            patch.ApplyTo(existedEntity);

            this.Context.Entry(existedEntity).State = EntityState.Modified;

            this.Context.SaveChanges();

            UpdateEntityAfterUpdate(existedEntity, this.Context);

            return existedEntity;
        }

        public override string GetPrimaryKeys()
        {
            return (PrimaryKeyExpression.Body as MemberExpression).Member.Name;
        }

        protected abstract TEntity GetDefaultEntity();

        protected virtual void SaveRelationships(TEntity entity, ApiDBContent db)
        {
        }

        /// <summary>
        /// Get the current user profile, if the user profile don't exists, create a default user profile.
        /// </summary>
        /// <returns>Returns the current user profile.</returns>
        protected TEntity FindCurrent()
        {
            TEntity entity = GetDbQuery().FirstOrDefault();

            if (entity == null)
            {
                entity = GetDefaultEntity();

                if (entity == null)
                {
                    return default(TEntity);
                }

                entity = DbSet.Add(entity).Entity;

                this.Context.SaveChanges();

                this.Context.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }

        protected virtual void UpdateEntityAfterUpdate(TEntity entity, ApiDBContent context)
        {
        }

        protected virtual void UpdateEntityBeforeUpdate(TEntity entity, ApiDBContent context)
        {
        }
    }

    /// <summary>
    /// Override the base repository, the primary key type is int.
    /// </summary>
    /// <typeparam name="TD">DB context generics.</typeparam>
    /// <typeparam name="T">Entity generics.</typeparam>
    public abstract class Repository<TEntity> : Repository<TEntity, int>, IRepository<TEntity, int>
        where TEntity : class, new()
    {
        public Repository(ApiDBContent context)
            : base(context)
        {
        }
    }

    public class IdToStringVisitor<TSource> : ExpressionVisitor
    {
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TSource), typeof(string));

            return Expression.Lambda(delegateType, Visit(node.Body), node.Parameters);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var to_string_method = typeof(object).GetMethod("ToString");

            return Expression.Call(Expression.Property(Visit(node.Expression), node.Member.Name), to_string_method);
        }
    }

}

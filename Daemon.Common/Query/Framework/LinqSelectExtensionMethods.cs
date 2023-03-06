using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Daemon.Common.Middleware;
using Daemon.Common.Query.Framework.Query;

namespace Daemon.Common.Query.Framework
{
    public static class LinqSelectExtensionMethods
    {
        private const string SELECT = "@fields";
        private const string EXCLUDED = "@excluded";
        private const string DISTINCT = "Distinct";

        public static IQueryable<dynamic> Select<TDomainEntity>(this IQueryable<TDomainEntity> queryable, IEnumerable<string> fields)
            where TDomainEntity : class
        {
            string AliasFunc(string name) => name;
            return queryable.Select(fields, AliasFunc);
        }

        public static IQueryable<dynamic> Select<TDomainEntity>(this IQueryable<TDomainEntity> queryable, IEnumerable<string> fields, Func<string, string> aliasFunc, List<string> emptyFields = null)
            where TDomainEntity : class
        {
            if (fields == null || !fields.Any())
            {
                return queryable.Select(r => new { });
            }

            fields = MatchFieldsWithEntityProperties<TDomainEntity>(fields);
            return queryable.Select(CreateNewStatement<TDomainEntity>(fields, aliasFunc, emptyFields));
        }

        /// <summary>
        /// Adds the where conditions to queryable.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <returns>The queryable with filter applied.</returns>
        public static IQueryable<object> Select<TEntity>(this IQueryable<TEntity> queryable, Func<string, string> aliasFunc, int aliasCount, bool hasRelationshipFields = false)
            where TEntity : class
        {
            string selecValue = HttpContextHelper.Current.Request.Query[SELECT], excludedValue = HttpContextHelper.Current.Request.Query[EXCLUDED];
            try
            {
                List<string> fields = new List<string>();
                bool isDistinct = false;
                if (!string.IsNullOrEmpty(selecValue))
                {
                    var index = selecValue.IndexOf(" ");
                    if (index != -1)
                    {
                        isDistinct = DISTINCT.Equals(selecValue.Substring(0, index), StringComparison.OrdinalIgnoreCase);
                        if (isDistinct)
                        {
                            selecValue = selecValue.Substring(index + 1);
                        }
                    }

                    fields = selecValue.Split(',').ToList();
                }

                List<string> excludedFields = new List<string>();
                if (!string.IsNullOrEmpty(excludedValue))
                {
                    excludedFields = excludedValue.Split(',').ToList();
                }

                List<string> columnNames = queryable.Column().ToList();
                if (fields.Count != 0 && excludedFields.Count != 0)
                {
                    fields = fields.Where(r => !excludedFields.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
                }
                else if (excludedFields.Count != 0)
                {
                    List<string> columns = columnNames;
                    if (hasRelationshipFields)
                    {
                        columns = typeof(TEntity).GetProperties().Select(r => r.Name).ToList();
                    }

                    fields = columns.Where(r => !excludedFields.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();
                }

                if (fields.Count == 0)
                {
                    if (aliasCount != 0 && hasRelationshipFields)
                    {
                        fields = typeof(TEntity).GetProperties().Select(r => r.Name).ToList();
                    }
                    else
                    {
                        return queryable;
                    }
                }

                IQueryable<dynamic> result;
                if (fields.Any(r => !columnNames.Contains(r, StringComparer.OrdinalIgnoreCase)))
                {
                    result = queryable.ToList().AsQueryable().Select(fields, aliasFunc);
                }
                else
                {
                    result = queryable.Select(fields, aliasFunc);
                }

                if (isDistinct)
                {
                    return result.Distinct();
                }

                return result;
            }
            catch
            {
                return queryable;
            }
        }

        private static Expression<Func<TDomainEntity, dynamic>> CreateNewStatement<TDomainEntity>(IEnumerable<string> fields, Func<string, string> aliasFunc, List<string> emptyFields = null)
        {
            var instanceType = typeof(TDomainEntity);

            // input parameter "o"
            var parameter = Expression.Parameter(instanceType, "o");

            fields = emptyFields != null ? fields.Union(emptyFields) : fields;
            Dictionary<string, PropertyInfo> sourceProperties = fields.Select(name => new { name, alias = aliasFunc(name) }).Where(field => instanceType.GetProperty(field.alias) != null).ToDictionary(field => field.name, field => instanceType.GetProperty(field.alias));
            Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType(sourceProperties);
            IEnumerable<MemberBinding> bindings = dynamicType.GetFields().Select(p => Expression.Bind(p, emptyFields != null && emptyFields.Contains(p.Name) ? (Expression)Expression.Constant(string.Empty) : Expression.Property(parameter, aliasFunc(p.Name)))).OfType<MemberBinding>();

            // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var memberInit = Expression.MemberInit(Expression.New(dynamicType.GetConstructor(Type.EmptyTypes)), bindings);

            // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
            var lambda = Expression.Lambda<Func<TDomainEntity, dynamic>>(memberInit, parameter);

            return lambda;
        }

        private static IEnumerable<string> MatchFieldsWithEntityProperties<TDomainEntity>(IEnumerable<string> fields)
        {
            if (fields == null || !fields.Any())
            {
                return fields;
            }

            IEnumerable<string> columnNames = GetEntityProperties<TDomainEntity>();
            List<string> result = new List<string>();
            foreach (string field in fields)
            {
                if (columnNames.Contains(field))
                {
                    result.Add(field);
                    continue;
                }

                var matchedField = columnNames.FirstOrDefault(i => string.Equals(i, field, StringComparison.OrdinalIgnoreCase));
                if (matchedField != null)
                {
                    result.Add(matchedField);
                    continue;
                }

                result.Add(field);
            }

            return result;
        }

        private static IEnumerable<string> GetEntityProperties<TDomainEntity>()
        {
            Type tmp = typeof(TDomainEntity);
            IEnumerable<string> ret;
            string key = tmp.Name;
            if (!_dictEntityProperties.ContainsKey(key))
            {
                _dictEntityProperties.TryAdd(key, tmp.GetProperties().Select(r => r.Name).ToList());
            }

            ret = _dictEntityProperties[key];
            return ret;
        }

        private static ConcurrentDictionary<string, IEnumerable<string>> _dictEntityProperties = new ConcurrentDictionary<string, IEnumerable<string>>();
    }
}

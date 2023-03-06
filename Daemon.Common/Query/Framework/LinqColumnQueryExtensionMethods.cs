using System.Linq;
using Daemon.Common.Middleware;
using Daemon.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Daemon.Common.Const;
using Daemon.Common.Extend;
namespace Daemon.Common.Query.Framework
{
    public static class LinqColumnQueryExtensionMethods
    {
        public static IQueryable<TEntity> ColumnQuery<TEntity>(this IQueryable<TEntity> queryable)
            where TEntity : class
        {
            var queryString = HttpContextHelper.Current.Request.Query;
            Dictionary<string, string> queryDict = queryString.Keys.Where(r => !r.StartsWith("@"))
                .ToDictionary(r => r, r => HttpContextHelper.Current.Request.Query[r].FirstOrDefault());
            if (queryDict.Count > 0)
            {
                try
                {
                    var ctx = ServiceLocator.Resolve<ApiDBContent>();
                    var entityType = ctx.Model.FindEntityType(Daemon.Common.Const.SystemConst.Model_Name_Space + typeof(TEntity).Name);
                    var properties = entityType.GetProperties();
                    List<string> allColumns = properties
                         .Select(x => x.Name)
                         .ToList();
                    List<FilterItem> filterItems = (from field in queryDict
                                                    join column in allColumns on field.Key.ToLower() equals column.ToLower()
                                                    select new FilterItem()
                                                    {
                                                        Operator = FilterConst.FILTER_OPERATION_EQUAL_TO,
                                                        FieldName = column,
                                                        Value = field.Value,
                                                    }).ToList();

                    FilterSet filterSet = new FilterSet()
                    {
                        FilterItems = filterItems,
                        LogicalOperator = LogicalOperator.And,
                    };

                    return queryable.Filter(filterSet, null);
                }
                catch
                {
                    return queryable;
                }
            }
            return queryable;
        }

        public static IQueryable<string> Column<TEntity>(this IQueryable<TEntity> queryable)
        {
            var ctx = ServiceLocator.Resolve<ApiDBContent>();
            var entityType = ctx.Model.GetEntityTypes().Where(a => a.Name.IndexOf("." + typeof(TEntity).Name) > -1).FirstOrDefault();
            var properties = entityType.GetProperties();
            List<string> allColumns = properties
                 .Select(x => x.Name)
                 .ToList();
            return allColumns.AsQueryable();
        }

        public static IQueryable<string> ExtColumn<TEntity>(this IQueryable<TEntity> queryable)
        {
            var ctx = ServiceLocator.Resolve<ApiDBContent>();
            var entityType = ctx.Model.GetEntityTypes().Where(a => a.Name.IndexOf("." + typeof(TEntity).Name) > -1).FirstOrDefault();
            var properties = entityType.GetProperties().Select(r => r.Name);
            var allProperties = typeof(TEntity).GetProperties();
            var extProperties = allProperties.Where(r => !properties.Contains(r.Name)).Select(r => r.Name);
            return extProperties.AsQueryable();
        }
    }
}

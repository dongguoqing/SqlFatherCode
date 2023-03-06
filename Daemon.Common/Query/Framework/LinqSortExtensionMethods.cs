using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Daemon.Common.Linq;
using Daemon.Common.Reflection;
using Daemon.Common.Query.Framework.Query;
using Daemon.Common.Const;
using Daemon.Common.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Daemon.Common.Query.Framework
{
    public static class LinqSortExtensionMethods
    {
        private const string SORT = "@sort";
        private const string DESCENDING = "desc";

        public static IQueryable<TDomainEntity> Sort<TDomainEntity>(this IQueryable<TDomainEntity> queryable, IEnumerable<ISortItem> sortItems)
        {
            if (sortItems == null)
            {
                return queryable;
            }

            string entityTypeName = typeof(TDomainEntity).Name;
            bool isExist = sortItems.Any(r => LinqExtensionConst._excludedItems.ContainsKey(entityTypeName + "-" + r.Name));
            if (isExist)
            {
                queryable = queryable.ToList().AsQueryable();
            }

            IOrderedQueryable<TDomainEntity> orderedQueryable = null;
            foreach (ISortItem sortItem in sortItems)
            {
                // Parse the first sort item separately because the subsequent sorting needs to call a different set of methods.
                if (isExist && LinqExtensionConst._excludedItems.ContainsKey(entityTypeName + "-" + sortItem.Name))
                {
                    orderedQueryable = orderedQueryable == null ? ApplyOrderBy(queryable, sortItem) : ApplyOrderBy(orderedQueryable, sortItem);
                }
                else
                {
                    orderedQueryable = orderedQueryable == null ? ApplySortExpression(queryable, sortItem) : ApplySortExpression(orderedQueryable, sortItem);
                }
            }

            return orderedQueryable ?? queryable;
        }

        public static IEnumerable<dynamic> Sort<TDomainEntity>(this IEnumerable<dynamic> enumerable, IEnumerable<ISortItem> sortItems)
        {
            if (sortItems == null)
            {
                return enumerable;
            }

            string entityTypeName = typeof(TDomainEntity).Name;
            IOrderedEnumerable<dynamic> orderedEnumerable = null;
            foreach (ISortItem sortItem in sortItems)
            {
                string excludedSortItem = entityTypeName + "-" + sortItem.Name;
                if (LinqExtensionConst._excludedItems.ContainsKey(excludedSortItem))
                {
                    orderedEnumerable = orderedEnumerable == null ? ApplyOrderBy(enumerable, sortItem, excludedSortItem) : ApplyOrderBy(orderedEnumerable, sortItem, excludedSortItem);
                }
                else
                {
                    orderedEnumerable = orderedEnumerable == null ? ApplySortExpression(enumerable, sortItem) : ApplySortExpression(orderedEnumerable, sortItem);
                }
            }

            return orderedEnumerable ?? enumerable;
        }

        public static IQueryable<TDomainEntity> Sort<TDomainEntity>(this IQueryable<TDomainEntity> queryable)
            where TDomainEntity : class
        {
            string value = HttpContextHelper.Current.Request.Query[SORT];
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return ApplyOrderBy(queryable, value);
                }
                catch(Exception ex)
                {
                    return queryable;
                }
            }

            return queryable;
        }

        private static IQueryable<TDomainEntity> ApplyOrderBy<TDomainEntity>(IQueryable<TDomainEntity> queryable, string sort)
            where TDomainEntity : class
        {
            List<string> columnNames = queryable.Column<TDomainEntity>().ToList();
            IEnumerable<SortItem> sortItems = sort.Split(',').Select(item =>
            {
                var subItems = item.Split('|');
                string name = subItems[0];
                bool isDescending = subItems.Length > 1 && subItems[1] == DESCENDING;
                if (!columnNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    throw new Exception("The sort param is invalid.");
                }
                else
                {
                    name = columnNames.FirstOrDefault(r => string.Equals(r, name, StringComparison.OrdinalIgnoreCase));
                }

                return new SortItem()
                {
                    Name = subItems[0],
                    Direction = isDescending ? SortDirection.Descending : SortDirection.Ascending,
                };
            });
            return queryable.Sort(sortItems);
        }

        private static IOrderedQueryable<TDomainEntity> ApplyOrderBy<TDomainEntity>(IQueryable<TDomainEntity> queryable, ISortItem sortItem)
        {
            Comparer comparer = GetComparer<TDomainEntity>(sortItem);
            return sortItem.Direction == SortDirection.Descending ? queryable.OrderByDescending(p => p, comparer) : queryable.OrderBy(p => p, comparer);
        }

        private static IOrderedQueryable<TDomainEntity> ApplyOrderBy<TDomainEntity>(IOrderedQueryable<TDomainEntity> queryable, ISortItem sortItem)
        {
            Comparer comparer = GetComparer<TDomainEntity>(sortItem);
            return sortItem.Direction == SortDirection.Descending ? queryable.ThenByDescending(p => p, comparer) : queryable.ThenBy(p => p, comparer);
        }

        private static Comparer GetComparer<TDomainEntity>(ISortItem sortItem)
        {
            return new Comparer
            {
                CurrentSortItemName = sortItem.Name,
                CurrentSortInstanceType = typeof(TDomainEntity),
                SortItems = LinqExtensionConst._excludedItems,
            };
        }

        /// <summary>
        /// Applies the initial sort expression.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private static IOrderedQueryable<TDomainEntity> ApplySortExpression<TDomainEntity>(IQueryable<TDomainEntity> queryable, ISortItem sortItem)
        {
            string sortMethod = sortItem.Direction == SortDirection.Descending ? "OrderByDescending" : "OrderBy";
            return ApplySortExpression(queryable, sortItem, sortMethod);
        }

        /// <summary>
        /// Applies a subsequent sort expression.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private static IOrderedQueryable<TDomainEntity> ApplySortExpression<TDomainEntity>(IOrderedQueryable<TDomainEntity> queryable, ISortItem sortItem)
        {
            string sortMethod = sortItem.Direction == SortDirection.Descending ? "ThenByDescending" : "ThenBy";
            return ApplySortExpression(queryable, sortItem, sortMethod);
        }

        /// <summary>
        /// Applies the sort expression.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <param name="orderByMethod">The order by method.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private static IOrderedQueryable<TDomainEntity> ApplySortExpression<TDomainEntity>(IQueryable<TDomainEntity> queryable, ISortItem sortItem, string orderByMethod)
        {
            PropertyMap propertyMap = new PropertyMap(typeof(TDomainEntity));
            MethodCallExpression expressionResult = null;
            ReadOnlyCollection<PropertyInfo> nestedPropertyList = propertyMap.FindNestedProperty(PropertyParser.FindProperty<TDomainEntity>(sortItem.Name));

            if (nestedPropertyList == null && nestedPropertyList.Count == 0)
            {
                throw new InvalidOperationException("The class '" + typeof(TDomainEntity).Name + "' does not contain the property '" + sortItem.Name + "'.");
            }

            Type entityType = typeof(TDomainEntity);

            // Get the overall expression property. (e.g. the "o" in the expression o => o.Name == 'test');
            ParameterExpression expressionParameter = Expression.Parameter(entityType);
            MemberExpression propertyAccess = ExpressionParser.CreateNestedPropertyExpression(propertyMap, sortItem.Name, expressionParameter);
            LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, expressionParameter);

            Type[] expressionTypes = new Type[] { entityType, nestedPropertyList.Last().PropertyType };
            expressionResult = Expression.Call(typeof(Queryable), orderByMethod, expressionTypes, queryable.Expression, Expression.Quote(orderByExpression));

            return queryable.Provider.CreateQuery<TDomainEntity>(expressionResult) as IOrderedQueryable<TDomainEntity>;
        }

        private static IOrderedEnumerable<dynamic> ApplyOrderBy(IEnumerable<dynamic> queryable, ISortItem sortItem, string excludedSortItem)
        {
            Type type = LinqExtensionConst._excludedItems[excludedSortItem.ToLower()];
            dynamic Func(dynamic r) => Convert.ChangeType(((IDictionary<string, object>)r)[sortItem.Name], type);
            return sortItem.Direction == SortDirection.Ascending ? queryable.OrderBy(Func) : queryable.OrderByDescending(Func);
        }

        private static IOrderedEnumerable<dynamic> ApplyOrderBy(IOrderedEnumerable<dynamic> queryable, ISortItem sortItem, string excludedSortItem)
        {
            Type type = LinqExtensionConst._excludedItems[excludedSortItem.ToLower()];
            dynamic Func(dynamic r) => Convert.ChangeType(((IDictionary<string, object>)r)[sortItem.Name], type);
            return sortItem.Direction == SortDirection.Ascending ? queryable.OrderBy(Func) : queryable.OrderByDescending(Func);
        }

        private static Comparer GetComparer(ISortItem sortItem, Type entityType)
        {
            return new Comparer
            {
                CurrentSortItemName = sortItem.Name,
                CurrentSortInstanceType = entityType,
                SortItems = LinqExtensionConst._excludedItems,
            };
        }

        /// <summary>
        /// Applies the initial sort expression.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private static IOrderedEnumerable<dynamic> ApplySortExpression(IEnumerable<dynamic> enumerable, ISortItem sortItem)
        {
            dynamic Func(dynamic r) => ((IDictionary<string, object>)r)[sortItem.Name];
            return sortItem.Direction == SortDirection.Ascending ? enumerable.OrderBy(Func) : enumerable.OrderByDescending(Func);
        }

        /// <summary>
        /// Applies the initial sort expression.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private static IOrderedEnumerable<dynamic> ApplySortExpression(IOrderedEnumerable<dynamic> enumerable, ISortItem sortItem)
        {
            dynamic Func(dynamic r) => ((IDictionary<string, object>)r)[sortItem.Name];
            return sortItem.Direction == SortDirection.Ascending ? enumerable.ThenBy(Func) : enumerable.ThenByDescending(Func);
        }
    }
}

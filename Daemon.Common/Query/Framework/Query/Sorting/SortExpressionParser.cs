using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Reflection;
using Daemon.Common.Linq;

namespace Daemon.Common.Query.Framework.Sorting
{
    /// <summary>
    /// Responsible for managing sort expressions for application to a Queryable.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class SortExpressionParser<TEntity>
        where TEntity : class
    {
        private readonly PropertyMap _propertyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortExpressionParser{TEntity}"/> class.
        /// </summary>
        /// <param name="propertyMap">The property map.</param>
        internal SortExpressionParser(PropertyMap propertyMap)
        {
            _propertyMap = propertyMap;
        }

        /// <summary>
        /// Applies the initial sort expression.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="sortItem">The sort item.</param>
        /// <returns>The queryable with sorting applied.</returns>
        internal IOrderedQueryable<TEntity> ApplySortExpression(IQueryable<TEntity> queryable, ISortItem sortItem)
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
        internal IOrderedQueryable<TEntity> ApplySortExpression(IOrderedQueryable<TEntity> queryable, ISortItem sortItem)
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
        private IOrderedQueryable<TEntity> ApplySortExpression(IQueryable<TEntity> queryable, ISortItem sortItem, string orderByMethod)
        {
            MethodCallExpression expressionResult = null;
            ReadOnlyCollection<PropertyInfo> nestedPropertyList = _propertyMap.FindNestedProperty(PropertyParser.FindProperty<TEntity>(sortItem.Name));

            if (nestedPropertyList == null && nestedPropertyList.Count == 0)
            {
                throw new InvalidOperationException("The class '" + typeof(TEntity).Name + "' does not contain the property '" + sortItem.Name + "'.");
            }

            Type entityType = typeof(TEntity);

            // Get the overall expression property. (e.g. the "o" in the expression o => o.Name == 'test');
            ParameterExpression expressionParameter = Expression.Parameter(entityType);
            MemberExpression propertyAccess = ExpressionParser.CreateNestedPropertyExpression(_propertyMap, sortItem.Name, expressionParameter);
            LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, expressionParameter);

            Type[] expressionTypes = new Type[] { entityType, nestedPropertyList.Last().PropertyType };
            expressionResult = Expression.Call(typeof(Queryable), orderByMethod, expressionTypes, queryable.Expression, Expression.Quote(orderByExpression));

            return queryable.Provider.CreateQuery<TEntity>(expressionResult) as IOrderedQueryable<TEntity>;
        }
    }
}

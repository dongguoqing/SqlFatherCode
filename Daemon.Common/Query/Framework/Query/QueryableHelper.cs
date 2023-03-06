using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Daemon.Common.Reflection;
using Daemon.Common.Query.Framework.Query.Aggregate;
using Daemon.Common.ExpressionHelper;
using Daemon.Common.Query.Framework.Sorting;
using Daemon.Common.Json;

namespace Daemon.Common.Query.Framework
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class QueryableHelper<TEntity>
            where TEntity : class
    {
        private readonly AggregateExpressionParser<TEntity> _aggregateExpressionParser;
        private readonly Dictionary<string, Type> _excludedSortItems = null;
        private readonly FilterExpressionParser<TEntity> _filterExpressionParser;
        private readonly SortExpressionParser<TEntity> _sortExpressionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableHelper{T}"/> class.
        /// </summary>
        public QueryableHelper(IDelimitedListParser delimitedListParser)
        {
            PropertyMap propertyMap = new PropertyMap(typeof(TEntity));
            _aggregateExpressionParser = new AggregateExpressionParser<TEntity>(propertyMap);
            _sortExpressionParser = new SortExpressionParser<TEntity>(propertyMap);
            _filterExpressionParser = new FilterExpressionParser<TEntity>(propertyMap, delimitedListParser);

            _excludedSortItems = new Dictionary<string, Type>()
            {
                { "schoolgridentity-arrivaltime", typeof(DateTime) },
                { "schoolgridentity-departtime", typeof(DateTime) },
                { "schoolgridentity-begintime", typeof(DateTime) },
                { "schoolgridentity-endtime", typeof(DateTime) },
                { "fieldtripgridentity-departuretime", typeof(DateTime) },
                { "fieldtripgridentity-returntime", typeof(DateTime) },
            };
        }

        public IQueryable<TEntity> AddSkipTake(IQueryable<TEntity> queryable, int? skip, int? take)
        {
            if (skip.HasValue)
            {
                queryable = queryable.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                if (take.Value != 0)
                {
                    queryable = queryable.Take(take.Value);
                }
                else
                {
                    // to fix a design of LLBLGen https://www.llblgen.com/tinyforum/Messages.aspx?ThreadID=21029
                    queryable = queryable.Where(c => 1 == 0);
                }
            }

            return queryable;
        }

        /// <summary>
        /// Adds the where conditions to queryable.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>The queryable with sorting applied.</returns>
        public IQueryable<TEntity> AddWhereConditions(
            IQueryable<TEntity> queryable,
            FilterSet filters)
        {
            IQueryable<TEntity> returnValue = queryable;
            string entityTypeName = typeof(TEntity).Name;
            if (filters != null)
            {
                var item = filters.FilterItems.FirstOrDefault(p => _excludedSortItems.ContainsKey(entityTypeName.ToLower() + "-" + p.FieldName.ToLower()));
                if (item != null)
                {
                    returnValue = queryable.ToList().AsQueryable().Where(_filterExpressionParser.CreateWhereClause(filters));
                }
                else
                {
                    var whereExp = _filterExpressionParser.CreateWhereClause(filters);
                    if (whereExp != null)
                    {
                        returnValue = queryable.Where(whereExp);
                    }
                    else
                    {
                        returnValue = queryable;
                    }
                }
            }

            return returnValue;
        }

        public IQueryable<TEntity> ApplySorting(IQueryable<TEntity> queryable, IEnumerable<ISortItem> sortItems)
        {
            if (sortItems == null)
            {
                return queryable;
            }

            bool isExist = false;
            string entityTypeName = typeof(TEntity).Name;
            foreach (var item in sortItems)
            {
                if (_excludedSortItems.ContainsKey(entityTypeName.ToLower() + "-" + item.Name.ToLower()))
                {
                    isExist = true;
                    break;
                }
            }

            if (!isExist)
            {
                return AddSorting(queryable, sortItems);
            }
            else
            {
                var li = queryable.ToList();
                queryable = li.AsQueryable();
                if (sortItems.Count() > 0)
                {
                    if (_excludedSortItems.ContainsKey(entityTypeName.ToLower() + "-" + sortItems.First().Name.ToLower()))
                    {
                        queryable = ApplyOrderBy(queryable, sortItems.First(), true);
                    }
                    else
                    {
                        queryable = ApplySortExpression(queryable, sortItems.First(), true);
                    }

                    if (sortItems.Count() > 1)
                    {
                        foreach (ISortItem sortItem in sortItems.Skip(1))
                        {
                            if (_excludedSortItems.ContainsKey(entityTypeName.ToLower() + "-" + sortItem.Name.ToLower()))
                            {
                                queryable = ApplyOrderBy(queryable, sortItem, false);
                            }
                            else
                            {
                                queryable = ApplySortExpression(queryable, sortItem, false);
                            }
                        }
                    }
                }

                return queryable;
            }
        }

        public string ConvertWhereConditions(FilterSet filters, bool convertToRawWhereClause = false)
        {
            return _filterExpressionParser.CreateSQLWhereClause(filters, convertToRawWhereClause);
        }

        public object GetAggregateResult(IQueryable<TEntity> queryable, AggregateItem aggregateItem)
        {
            return _aggregateExpressionParser.GetAggregationResult(queryable, aggregateItem);
        }

        public object GetAggregateResultEntityFramework(IQueryable<TEntity> queryable, AggregateItem aggregateItem)
        {
            return _aggregateExpressionParser.GetAggregationResultEntityFramework(queryable, aggregateItem);
        }

        /// <summary>
        /// Adds the sorting to queryable.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="sortItems">The sort items.</param>
        /// <returns>The queryable with sorting applied.</returns>
        private IQueryable<TEntity> AddSorting(
            IQueryable<TEntity> queryable,
            IEnumerable<ISortItem> sortItems)
        {
            IQueryable<TEntity> returnValue = queryable;

            if (sortItems != null && sortItems.Count() > 0)
            {
                IOrderedQueryable<TEntity> orderedQueryable = null;

                // Parse the first sort item separately because the subsequent sorting needs to call a different set of methods.
                ISortItem firstSortItem = sortItems.FirstOrDefault();
                orderedQueryable = _sortExpressionParser.ApplySortExpression(queryable, firstSortItem);

                foreach (ISortItem sortItem in sortItems.Skip(1))
                {
                    orderedQueryable = _sortExpressionParser.ApplySortExpression(orderedQueryable, sortItem);
                }

                if (orderedQueryable != null)
                {
                    returnValue = orderedQueryable;
                }
            }

            return returnValue;
        }

        private IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> queryable, ISortItem sortItem, bool isOrderBy)
        {
            Comparer comparer = new Comparer();
            comparer.CurrentSortItemName = sortItem.Name;
            comparer.CurrentSortInstanceType = typeof(TEntity);
            comparer.SortItems = _excludedSortItems;
            if (sortItem.Direction == SortDirection.Descending)
            {
                if (isOrderBy)
                {
                    queryable = queryable.OrderByDescending(p => p, comparer);
                }
                else
                {
                    queryable = (queryable as IOrderedQueryable<TEntity>).ThenByDescending(p => p, comparer);
                }
            }
            else
            {
                if (isOrderBy)
                {
                    queryable = queryable.OrderBy(p => p, comparer);
                }
                else
                {
                    queryable = (queryable as IOrderedQueryable<TEntity>).ThenBy(p => p, comparer);
                }
            }

            return queryable;
        }

        private IOrderedQueryable<TEntity> ApplySortExpression(IQueryable<TEntity> query, ISortItem sortItem, bool isOrderBy)
        {
            if (isOrderBy)
            {
                return _sortExpressionParser.ApplySortExpression(query, sortItem);
            }
            else
            {
                return _sortExpressionParser.ApplySortExpression(query as IOrderedQueryable<TEntity>, sortItem);
            }
        }
    }

    public class Comparer : IComparer<object>
    {
        public Type CurrentSortInstanceType { get; set; }

        public string CurrentSortItemName { get; set; }

        public Dictionary<string, Type> SortItems { get; set; }

        public int Compare(object x, object y)
        {
            PropertyInfo info = CurrentSortInstanceType.GetProperty(CurrentSortItemName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            object valueX = info.GetValue(x, null);
            object valueY = info.GetValue(y, null);

            if (!SortItems.ContainsKey(CurrentSortInstanceType.Name.ToLower() + "-" + CurrentSortItemName.Trim().ToLower()))
            {
                return 0;
            }

            Type targetType = SortItems[CurrentSortInstanceType.Name.ToLower() + "-" + CurrentSortItemName.Trim().ToLower()];
            string targetTypeName = targetType.Name;
            if (targetType.IsGenericType)
            {
                targetTypeName = targetType.GetGenericArguments()[0].Name;
            }

            switch (targetTypeName)
            {
                case "DateTime":
                    DateTime vx = DateTime.MinValue;
                    DateTime vy = DateTime.MinValue;
                    if (valueX != null && !DateTime.TryParse(valueX.ToString(), out vx))
                    {
                        vx = DateTime.MaxValue;
                    }

                    if (valueY != null && !DateTime.TryParse(valueY.ToString(), out vy))
                    {
                        vy = DateTime.MaxValue;
                    }

                    if (vx > vy)
                    {
                        return 1;
                    }

                    if (vx == vy)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }

                default:
                    return 0;
            }
        }
    }
}

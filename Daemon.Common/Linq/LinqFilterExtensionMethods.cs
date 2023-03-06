using System.Collections.Generic;
using System.Linq;
using System;
using Daemon.Common.Query;
using Daemon.Common.Const;
using Daemon.Common.Middleware;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Daemon.Common.Json;
using System.Text;
using Daemon.Common.Query.Framework;
using Daemon.Common.ExpressionHelper;
using Daemon.Common.Reflection;
namespace Daemon.Common.Extend
{
    public static class LinqFilterExtensionMethods
    {
        private const string FILTER = "@filter";
        private const string IN_OPERATOR = "In";
        private const char AND = '&';
        private const char OR = '|';
        private const char LEFT_PARENTHESIS = '(';
        private const char RIGHT_PARENTHESIS = ')';
        private static List<string> _noValueOperators = new List<string>() { "isnull", "isnotnull" };

        /// <summary>
        /// Adds the where conditions to queryable.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>The queryable with filter applied.</returns>
        public static IQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> queryable, FilterSet filters, IDelimitedListParser parser)
            where TEntity : class
        {
            IQueryable<TEntity> returnValue = queryable;
            if (filters != null)
            {
                string entityTypeName = typeof(TEntity).Name;
                var item = filters.FilterItems.FirstOrDefault(r => LinqExtensionConst._excludedItems.ContainsKey(entityTypeName + "-" + r.FieldName));
                if (item != null)
                {
                    queryable = queryable.AsEnumerable().AsQueryable();
                }

                FilterExpressionParser<TEntity> filterExpressionParser = new FilterExpressionParser<TEntity>(new PropertyMap(typeof(TEntity)), parser);
                var whereExp = CreateWhereClause(filters, filterExpressionParser);
                if (whereExp != null)
                {
                    returnValue = queryable.Where(whereExp);
                }
                else
                {
                    returnValue = queryable;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Adds the where conditions to queryable.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="type"></param>
        /// <param name="filters">The filters.</param>
        /// <returns>The queryable with filter applied.</returns>
        public static IEnumerable<dynamic> Filter<TEntity>(this IEnumerable<dynamic> queryable, Type type, FilterSet filters, IDelimitedListParser parser)
            where TEntity : class
        {
            IEnumerable<dynamic> returnValue = queryable;
            if (filters != null)
            {
                FilterExpressionParser<TEntity> filterExpressionParser = new FilterExpressionParser<TEntity>(new PropertyMap(type), parser);
                var whereExp = CreateWhereClause(filters, type, filterExpressionParser);
                if (whereExp != null)
                {
                    returnValue = queryable.Where(r => whereExp.DynamicInvoke(r));
                }
                else
                {
                    returnValue = queryable;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Adds the where conditions to queryable.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <returns>The queryable with filter applied.</returns>
        public static IQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> queryable)
            where TEntity : class
        {
            string value = HttpContextHelper.Current.Request.Query[FILTER];
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return ApplyFilter(queryable, value);
                }
                catch (Exception ex)
                {
                    return queryable;
                }
            }

            return queryable;
        }

        public static short GetDBID()
        {
            string value = HttpContextHelper.Current.Request.Query[FILTER];
            if (!string.IsNullOrEmpty(value))
            {
                Regex rx = new Regex(@".*eq\(dbid,\s*(\d*)\s*\).*", RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(value);
                if (matches.Count > 0 && matches[0].Groups.Count > 1)
                {
                    return Convert.ToInt16(matches[0].Groups[1].Value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Creates the where clause.
        /// </summary>
        /// <param name="filterSet">The filter set.</param>
        /// <returns></returns>
        private static Expression<Func<TEntity, bool>> CreateWhereClause<TEntity>(FilterSet filterSet, FilterExpressionParser<TEntity> filterExpressionParser)
            where TEntity : class
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            Expression expressionBody = filterExpressionParser.CreateLambdaExpression(filterSet, parameterExpression);
            if (expressionBody != null)
            {
                return Expression.Lambda<Func<TEntity, bool>>(expressionBody, parameterExpression);
            }

            return null;
        }

        /// <summary>
        /// Creates the where clause.
        /// </summary>
        /// <param name="filterSet">The filter set.</param>
        /// <returns></returns>
        private static Delegate CreateWhereClause<TEntity>(FilterSet filterSet, Type type, FilterExpressionParser<TEntity> filterExpressionParser)
            where TEntity : class
        {
            ParameterExpression parameterExpression = Expression.Parameter(type);
            Expression expressionBody = filterExpressionParser.CreateLambdaExpression(filterSet, parameterExpression, false, type);
            if (expressionBody != null)
            {
                return Expression.Lambda(expressionBody, parameterExpression).Compile();
            }

            return null;
        }

        private static IQueryable<TEntity> ApplyFilter<TEntity>(IQueryable<TEntity> queryable, string filter)
            where TEntity : class
        {
            FilterSet filterSet = new FilterSet()
            {
                FilterItems = new List<FilterItem>(),
                FilterSets = new List<FilterSet>(),
            };

            List<string> columnNames = queryable.Column().ToList();
            //数据库中没有的实体类
            List<string> extColumns = queryable.ExtColumn().ToList();
            ParseFilter(filter, filterSet, columnNames, extColumns);
            IDelimitedListParser delimitedListParser = ServiceLocator.Resolve<IDelimitedListParser>();
            return queryable.Filter(filterSet, delimitedListParser);
        }

        private static string ParseFilter(string filter, FilterSet filterSet, List<string> columnNames, List<string> extColumns)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return filter;
            }

            filter = filter.TrimStart();
            char firstWord = filter.First();
            switch (firstWord)
            {
                case LEFT_PARENTHESIS:
                    filter = filter.Substring(1);
                    FilterSet childFilterSet = new FilterSet()
                    {
                        FilterItems = new List<FilterItem>(),
                        FilterSets = new List<FilterSet>(),
                    };
                    filter = ParseFilter(filter, childFilterSet, columnNames, extColumns);
                    filterSet.FilterSets.Add(childFilterSet);
                    break;
                case RIGHT_PARENTHESIS:
                    filter = filter.Substring(1);
                    return filter;
                case AND:
                    filter = filter.Substring(1);
                    if (filterSet.LogicalOperator == LogicalOperator.Or)
                    {
                        FilterSet grandchildFilterSet = new FilterSet()
                        {
                            FilterItems = filterSet.FilterItems.ToList(),
                            FilterSets = filterSet.FilterSets.ToList(),
                            LogicalOperator = LogicalOperator.Or,
                        };
                        filterSet.FilterItems.Clear();
                        filterSet.FilterSets.Clear();
                        filterSet.FilterSets.Add(grandchildFilterSet);
                    }

                    filterSet.LogicalOperator = LogicalOperator.And;
                    break;
                case OR:
                    filter = filter.Substring(1);
                    if (filterSet.LogicalOperator == LogicalOperator.And)
                    {
                        FilterSet grandchildFilterSet = new FilterSet()
                        {
                            FilterItems = filterSet.FilterItems.ToList(),
                            FilterSets = filterSet.FilterSets.ToList(),
                            LogicalOperator = LogicalOperator.And,
                        };
                        filterSet.FilterItems.Clear();
                        filterSet.FilterSets.Clear();
                        filterSet.FilterSets.Add(grandchildFilterSet);
                    }

                    filterSet.LogicalOperator = LogicalOperator.Or;
                    break;
                default:
                    FilterItem filterItem = CreateFilterItem(filter, columnNames, extColumns, out int length);
                    if (filterItem == null)
                    {
                        return filter;
                    }

                    filterSet.FilterItems.Add(filterItem);
                    filter = filter.Substring(length);
                    break;
            }

            return ParseFilter(filter, filterSet, columnNames, extColumns);
        }

        private static FilterItem CreateFilterItem(string filter, List<string> columnNames, List<string> extColumns, out int length)
        {
            length = 0;
            FilterItem filterItem = null;
            string value = GetMatchValue(filter);
            if (!string.IsNullOrEmpty(value))
            {
                int leftPosition = value.IndexOf(LEFT_PARENTHESIS), splitPosition = value.IndexOf(',');
                string itemOperator = value.Substring(0, leftPosition).Trim();
                if (_noValueOperators.Contains(itemOperator))
                {
                    splitPosition = splitPosition != -1 ? splitPosition : value.IndexOf(RIGHT_PARENTHESIS);
                    filterItem = new FilterItem
                    {
                        Operator = ConvertOperator(itemOperator),
                        FieldName = value.Substring(leftPosition + 1, splitPosition - leftPosition - 1).Trim(),
                        Value = string.Empty,
                    };
                }
                else
                {
                    filterItem = new FilterItem
                    {
                        Operator = ConvertOperator(itemOperator),
                        FieldName = value.Substring(leftPosition + 1, splitPosition - leftPosition - 1).Trim(),
                        Value = value.Substring(splitPosition + 1, value.Length - splitPosition - 2).Trim(),
                    };
                }

                if (!columnNames.Contains(filterItem.FieldName, StringComparer.OrdinalIgnoreCase))
                {

                    filterItem.FieldName = extColumns.FirstOrDefault(r => string.Equals(r, filterItem.FieldName, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    filterItem.FieldName = columnNames.FirstOrDefault(r => string.Equals(r, filterItem.FieldName, StringComparison.OrdinalIgnoreCase));
                }

                if (filterItem.Operator == IN_OPERATOR)
                {
                    var valueStrList = filterItem.Value.Split(',').Where(s => !string.IsNullOrEmpty(s));
                    filterItem.ValueList = "[" + string.Join(",", valueStrList.Select(r => "\"" + r + "\"")) + "]";
                }

                length = value.Length;
            }

            return filterItem;
        }

        private static string GetMatchValue(string filter)
        {
            int num = 0;
            StringBuilder value = new StringBuilder();
            foreach (var word in filter)
            {
                value.Append(word);
                if (word == LEFT_PARENTHESIS)
                {
                    num++;
                }
                else if (word == RIGHT_PARENTHESIS)
                {
                    num--;
                    if (num == 0)
                    {
                        break;
                    }
                }
            }

            return value.ToString();
        }

        private static string ConvertOperator(string operatorType)
        {
            return FilterConst.ConvertOperations.ContainsKey(operatorType) ? FilterConst.ConvertOperations[operatorType] : string.Empty;
        }
    }
}

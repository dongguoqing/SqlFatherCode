using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Reflection;
using Daemon.Common.Json;
using Daemon.Common.Query.ExpressionProviders;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.Framework.Query
{
    /// <summary>
    ///
    /// </summary>
    public class FilterExpressionParser<TEntity>
            where TEntity : class
    {
        private readonly IDelimitedListParser _delimitedListParser;
        private readonly PropertyMap _propertyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterExpressionParser{TEntity}"/> class.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public FilterExpressionParser(PropertyMap propertyMap, IDelimitedListParser parser)
        {
            _delimitedListParser = parser;
            _propertyMap = propertyMap;
        }

        /// <summary>
        /// Parses the filter set.
        /// </summary>
        /// <param name="filterSet">The filter set.</param>
        /// <returns></returns>
        public Expression CreateLambdaExpression(FilterSet filterSet, ParameterExpression parameterExpression, bool convertToRawWhereClause = false, Type type = null)
        {
            Expression expression = null;

            List<Expression> childExpressions = new List<Expression>();

            if (filterSet.FilterSets != null)
            {
                foreach (FilterSet childFilterSet in filterSet.FilterSets)
                {
                    Expression childFilterSetExpression = CreateLambdaExpression(childFilterSet, parameterExpression, convertToRawWhereClause, type);
                    if (childFilterSetExpression != null)
                    {
                        childExpressions.Add(childFilterSetExpression);
                    }
                }
            }

            if (filterSet.FilterItems != null)
            {
                foreach (FilterItem childFilterItem in filterSet.FilterItems)
                {
                    Expression childFilterExpression = ParseFilterItem(childFilterItem, parameterExpression, convertToRawWhereClause, type);
                    if (childFilterExpression != null)
                    {
                        childExpressions.Add(childFilterExpression);
                    }
                }
            }

            // Get the logical operation delegate (e.g. chooses between AND and OR).
            var logicalOperationDelegate = CreateLogicalOperationDelegate(filterSet.LogicalOperator);

            expression = childExpressions.FirstOrDefault();

            if (childExpressions.Count > 1)
            {
                for (int i = 1; i < childExpressions.Count; i++)
                {
                    expression = logicalOperationDelegate(expression, childExpressions[i]);
                }
            }

            return expression;
        }

        public string CreateSQLWhereClause(FilterSet filterSet, bool convertToRawWhereClause = false, Type type = null)
        {
            ParameterExpression parameterExpression = Expression.Parameter(type ?? typeof(TEntity));
            Expression expressionBody = CreateLambdaExpression(filterSet, parameterExpression, convertToRawWhereClause, type);
            QueryTranslator translator = new QueryTranslator();
            string sql = translator.Translate(expressionBody);
            return sql;
        }

        /// <summary>
        /// Creates the where clause.
        /// </summary>
        /// <param name="filterSet">The filter set.</param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> CreateWhereClause(FilterSet filterSet)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            Expression expressionBody = CreateLambdaExpression(filterSet, parameterExpression);
            if (expressionBody != null)
            {
                return Expression.Lambda<Func<TEntity, bool>>(expressionBody, parameterExpression);
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="expressionLeft"></param>
        /// <param name="expressionRight"></param>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> MergeLambdaExpression(Expression<Func<TEntity, bool>> expressionLeft, Expression<Func<TEntity, bool>> expressionRight, LogicalOperator logicalOperator)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            var logicalOperationDelegate = CreateLogicalOperationDelegate(logicalOperator);
            Expression expressionBody = logicalOperationDelegate(expressionLeft, expressionRight);
            if (expressionBody != null)
            {
                return Expression.Lambda<Func<TEntity, bool>>(expressionBody, parameterExpression);
            }

            return null;
        }

        /// <summary>
        /// Creates the logical operation delegate.
        /// </summary>
        /// <param name="logicalOperator">The logical operation.</param>
        /// <returns></returns>
        private Func<Expression, Expression, Expression> CreateLogicalOperationDelegate(LogicalOperator logicalOperator)
        {
            Func<Expression, Expression, Expression> logicalOperationDelegate = null;
            switch (logicalOperator)
            {
                case LogicalOperator.And:
                    logicalOperationDelegate = (primaryExpression, secondaryExpression) => Expression.AndAlso(primaryExpression, secondaryExpression);
                    break;
                case LogicalOperator.Or:
                    logicalOperationDelegate = (primaryExpression, secondaryExpression) => Expression.OrElse(primaryExpression, secondaryExpression);
                    break;
                default:
                    throw new InvalidOperationException("The logical operation '" + logicalOperator + "' is not supported.");
            }

            return logicalOperationDelegate;
        }

        private Type GetActualType(Type type, bool isListFilter, string typeHint)
        {
            Type actualType = null;
            if (isListFilter)
            {
                actualType = type;
            }
            else if (typeHint == "integer")
            {
                actualType = TypeCode.Int32.GetType(); // fix issue VIEW-1298
            }
            else
            {
                actualType = type.FindUnderlyingTypeIfNullable();
            }

            return actualType;
        }

        /// <summary>
        /// Parses the filter item.
        /// </summary>
        /// <param name="filterItem">The filter item.</param>
        /// <returns></returns>
        private Expression ParseFilterItem(FilterItem filterItem, ParameterExpression parameterExpression, bool convertToRawWhereClause, Type entityType)
        {
            if (filterItem.FieldName.Equals("1"))
            {
                return GetSimpleFilterItem(filterItem);
            }

            IEnumerable<PropertyInfo> propertyList = _propertyMap.FindNestedProperty(filterItem.FieldName);

            // Based on type create an expression provider.
            FilterExpressionProvider<TEntity> expressionProvider = null;

            // Get type of the property.
            Type type = propertyList.Last().PropertyType;
            ListFilterOperator listFilterOperator;
            bool isListFilter = Enum.TryParse<ListFilterOperator>(filterItem.Operator, out listFilterOperator);

            Type actualType = GetActualType(type, isListFilter, filterItem.TypeHint);

            if (isListFilter)
            {
                expressionProvider = ParseFilterList(filterItem.ValueList, parameterExpression, listFilterOperator, actualType, entityType);
            }
            else
            {
                switch (Type.GetTypeCode(actualType))
                {
                    case TypeCode.String:
                        expressionProvider = new StringFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                        break;
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Double:
                        int numberPrecision = filterItem.TypeHint == null ? 0 : filterItem.Precision ?? 10;
                        expressionProvider = new DecimalFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, numberPrecision, entityType);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Byte:
                        expressionProvider = new IntegerFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                        break;
                    case TypeCode.DateTime:
                        expressionProvider = DateTimeExpressionProvider(filterItem, parameterExpression, entityType);
                        break;
                    case TypeCode.Boolean:
                        expressionProvider = new BooleanFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                        break;
                    default:
                        if (actualType.Name == "TimeSpan")
                        {
                            expressionProvider = new TimeFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                        }
                        else
                        {
                            throw new InvalidOperationException("The type '" + actualType.Name + "' is not supported for a filter list.");
                        }

                        break;
                }
            }

            // Create member access hierarchy for property.
            return expressionProvider.GenerateExpressionBody(filterItem.FieldName, convertToRawWhereClause);
        }

        private Expression GetSimpleFilterItem(FilterItem filterItem)
        {
            Expression<Func<bool>> exp;
            if (filterItem.Operator.Equals("NotEqualTo", StringComparison.OrdinalIgnoreCase))
            {
                exp = () => false;
            }
            else
            {
                exp = () => true;
            }

            return exp.Body;
        }

        /// <summary>
        /// Creates a filter expression provider that accepts a typed list.  Only supported types are parsed, otherwise an exception is thrown.
        /// </summary>
        /// <param name="delimitedList">The delimited list.</param>
        /// <param name="parameterExpression"></param>
        /// <param name="listFilterOperator">The list filter operator.</param>
        /// <param name="listValueType">Type of the list value.</param>
        /// <returns></returns>
        private FilterExpressionProvider<TEntity> ParseFilterList(string delimitedList, ParameterExpression parameterExpression, ListFilterOperator listFilterOperator, Type listValueType, Type entityType)
        {
            // TODO: Add unsupported type exception for list types.
            return new ListFilterExpressionProvider<TEntity>(delimitedList, parameterExpression, listValueType, listFilterOperator, _delimitedListParser, entityType);
        }

        /// <summary>
        /// Parses the name of the property type by.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private Type ParsePropertyTypeByName(string propertyName)
        {
            string[] nestedNames = propertyName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            Type type = typeof(TEntity);
            foreach (string name in nestedNames)
            {
                PropertyInfo property = type.GetProperty(name);
                if (property != null)
                {
                    type = property.PropertyType.FindUnderlyingTypeIfNullable();
                }
                else
                {
                    type = null;
                    break;
                }
            }

            return type;
        }

        /// <summary>
        /// Datetime expression provider.
        /// </summary>
        /// <param name="filterItem">The filter item.</param>
        /// <returns></returns>
        private FilterExpressionProvider<TEntity> DateTimeExpressionProvider(FilterItem filterItem, ParameterExpression parameterExpression, Type entityType)
        {
            var typeHint = filterItem.TypeHint;
            if (string.IsNullOrEmpty(typeHint))
            {
                DateTime time = DateTime.Parse(filterItem.Value);
                if (time.Date != time)
                {
                    typeHint = time.Second > 0 ? "DateTimeSecond" : "DateTime";
                }
                else
                {
                    typeHint = "Date";
                }
            }

            switch (typeHint)
            {
                case "Time":
                    return new TimeFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                case "DateTime":
                    return new DateTimeFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
                case "DateTimeSecond":
                    return new DateTimeFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType, true);
                case "Date":
                default:
                    return new DateFilterExpressionProvider<TEntity>(filterItem.Value, parameterExpression, filterItem.Operator, entityType);
            }
        }
    }
}

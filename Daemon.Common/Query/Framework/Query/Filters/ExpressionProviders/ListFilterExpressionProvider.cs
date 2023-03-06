using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Reflection;
using Daemon.Common.Const;
using Daemon.Common.Json;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.Filters.ExpressionProviders
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TStorageEntity">The type of the storage entity.</typeparam>
    internal class ListFilterExpressionProvider<TStorageEntity> : FilterExpressionProvider<TStorageEntity>
        where TStorageEntity : class
    {
        private readonly ListFilterOperator _filterOperator;
        private readonly Type _listItemType;
        private readonly IDelimitedListParser _listParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFilterExpressionProvider{TStorageEntity}"/> class.
        /// </summary>
        /// <param name="filterOperator">The filter operation.</param>
        /// <param name="value">The value.</param>
        internal ListFilterExpressionProvider(string value, ParameterExpression parameterExpression, Type listItemType, ListFilterOperator filterOperator, IDelimitedListParser listParser, Type type)
            : base(value, parameterExpression, type)
        {
            _listItemType = listItemType;
            _filterOperator = filterOperator;
            _listParser = listParser;
        }

        /// <summary>
        /// Initializes the expression body.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected override Expression InitializeExpressionBody(MemberExpression property, string propertyName, string rawValue, bool convertToRawWhereClause = false)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("Parameter", "IN");
            if (propertyName.Equals("Sex", StringComparison.OrdinalIgnoreCase))
            {
                // RW-16798 Gender column list mover quick filter in student grid cannot return results when filtering with the options.
                rawValue = rawValue.Replace("Female", "F").Replace("Male", "M");
            }

            object parsedValues = _listParser.ParseList(_listItemType, rawValue, parameters);
            Type listType = typeof(ListWithParameter<>).MakeGenericType(new Type[] { _listItemType });

            Dictionary<string, object> filterFieldsDict = null;
            if (FilterConst.FilterFields.TryGetValue(propertyName, out filterFieldsDict))
            {
                Type objType = parsedValues.GetType();
                object databaseValue = null;
                var count = System.Convert.ToInt32(objType.GetProperty("Count").GetValue(parsedValues, null));
                for (int i = 0; i < count; i++)
                {
                    var item = objType.GetProperty("Item").GetValue(parsedValues, new object[] { i });

                    if (filterFieldsDict.TryGetValue(item.ToString(), out databaseValue))
                    {
                        objType.GetProperty("Item").SetValue(parsedValues, databaseValue, new object[] { i });
                    }
                }
            }

            ConstantExpression values = Expression.Constant(parsedValues, listType);
            Expression body = null;

            MethodInfo containsMethod = FindReflectedMethod(listType, _filterOperator == ListFilterOperator.NotIn ? "NotContains" : "Contains", _listItemType);
            body = Expression.Call(values, containsMethod, property);

            return body;
        }
    }
}

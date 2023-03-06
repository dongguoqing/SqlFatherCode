using System;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.Filters.ExpressionProviders
{
    /// <summary>
    ///
    /// </summary>
    internal class StringFilterExpressionProvider<TStorageEntity> : FilterExpressionProvider<TStorageEntity>
        where TStorageEntity : class
    {
        private readonly StringFilterOperator _filterOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringFilterExpressionProvider{TStorageEntity}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="filterOperator">The filter operation.</param>
        internal StringFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type, bool convertToRawWhereClause = false)
            : base(value, parameterExpression, type)
        {
            StringFilterOperator operation;
            if (!Enum.TryParse<StringFilterOperator>(filterOperator, out operation))
            {
                throw new InvalidOperationException("Invalid filter operation for a string '" + filterOperator + "'.");
            }

            _filterOperator = operation;
        }

        /// <summary>
        /// Creates the expression body.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected override Expression InitializeExpressionBody(MemberExpression property, string propertyName, string rawValue, bool convertToRawWhereClause = false)
        {
            Expression body = null;
            ConstantExpression value = Expression.Constant(rawValue, property.Type);
            var leftProperty = Expression.Call(property, typeof(string).GetMethod("ToUpper", Type.EmptyTypes));
            var rightValue = Expression.Call(value, typeof(string).GetMethod("ToUpper", Type.EmptyTypes));
            switch (_filterOperator)
            {
                case StringFilterOperator.GreaterThan:
                case StringFilterOperator.LessThan:
                case StringFilterOperator.GreaterThanOrEqual:
                case StringFilterOperator.LessThanOrEqual:
                    var method = value.Type.GetMethod("CompareTo", new[] { typeof(string) });
                    var zero = Expression.Constant(0);
                    var result = Expression.Call(leftProperty, method, rightValue);
                    body = Expression.MakeBinary((ExpressionType)Enum.Parse(typeof(ExpressionType), _filterOperator.ToString(), true), result, zero);
                    break;
                case StringFilterOperator.EqualTo:
                    body = Expression.Equal(leftProperty, rightValue);
                    body = Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, typeof(string))), body);
                    break;
                case StringFilterOperator.Contains:
                    MethodInfo containsMethod = FindReflectedMethod<string>("Contains");
                    body = Expression.Call(leftProperty, containsMethod, rightValue);
                    body = Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, typeof(string))), body);
                    break;
                case StringFilterOperator.DoesNotContain:
                    if (convertToRawWhereClause)
                    {
                        MethodInfo containMethod = FindReflectedMethod<string>("NotContains");
                        body = Expression.Call(null, containMethod, leftProperty, rightValue);
                    }
                    else
                    {
                        MethodInfo containMethod = FindReflectedMethod<string>("Contains");
                        body = Expression.Not(Expression.Call(leftProperty, containMethod, rightValue));
                    }

                    body = Expression.OrElse(Expression.Equal(property, Expression.Constant(null, typeof(string))), body);
                    break;
                case StringFilterOperator.EndsWith:
                    MethodInfo endsWithMethod = FindReflectedMethod<string>("EndsWith");
                    body = Expression.Call(leftProperty, endsWithMethod, rightValue);
                    body = Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, typeof(string))), body);
                    break;
                case StringFilterOperator.IsNull:
                    // If the operator is checking for null, ignore the provided value type.
                    value = Expression.Constant(null, typeof(string));
                    body = Expression.OrElse(Expression.Equal(property, value), Expression.Equal(leftProperty, Expression.Constant(string.Empty, typeof(string))));
                    break;
                case StringFilterOperator.IsNotNull:
                    value = Expression.Constant(null, typeof(string));
                    body = Expression.AndAlso(Expression.NotEqual(property, value), Expression.NotEqual(leftProperty, Expression.Constant(string.Empty, typeof(string))));
                    break;
                case StringFilterOperator.NotEqualTo:
                    body = Expression.NotEqual(leftProperty, rightValue);
                    body = Expression.OrElse(Expression.Equal(property, Expression.Constant(null, typeof(string))), body);
                    break;
                case StringFilterOperator.StartsWith:
                    MethodInfo startsWithMethod = FindReflectedMethod<string>("StartsWith");
                    body = Expression.Call(leftProperty, startsWithMethod, rightValue);
                    body = Expression.AndAlso(Expression.NotEqual(property, Expression.Constant(null, typeof(string))), body);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a string.");
            }

            return body;
        }
    }
}

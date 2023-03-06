using System;
using System.Linq.Expressions;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.ExpressionProviders
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class IntegerFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly NumericFilterOperator _filterOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerFilterExpressionProvider{TStorageEntity}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The paramter expression.</param>
        /// <param name="filterOperator">The filter operation.</param>
        internal IntegerFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type)
            : base(value, parameterExpression, filterOperator, type)
        {
            if (!Enum.TryParse<NumericFilterOperator>(filterOperator, out var numericfilterOperator))
            {
                throw new InvalidOperationException("Invalid filter operation for a number '" + numericfilterOperator + "'.");
            }

            _filterOperator = numericfilterOperator;
        }

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;
            object value = BlueearthConvert.ChangeType(rawValue, property.Type);
            var valueExpression = Expression.Constant(value, property.Type);
            switch (_filterOperator)
            {
                case NumericFilterOperator.EqualTo:
                    body = Expression.Equal(property, valueExpression);
                    break;
                case NumericFilterOperator.NotEqualTo:
                    body = Expression.NotEqual(property, valueExpression);
                    break;
                case NumericFilterOperator.GreaterThan:
                    body = Expression.GreaterThan(property, valueExpression);
                    break;
                case NumericFilterOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(property, valueExpression);
                    break;
                case NumericFilterOperator.LessThan:
                    body = Expression.LessThan(property, valueExpression);
                    break;
                case NumericFilterOperator.LessThanOrEqualTo:
                    body = Expression.LessThanOrEqual(property, valueExpression);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }
    }
}

using System;
using System.Linq.Expressions;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.Filters.ExpressionProviders
{
    /// <summary>
    ///
    /// </summary>
    internal class BooleanFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly BooleanFilterOperator _filterOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFilterExpressionProvider{TEntity}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="filterOperator">The filter operation.</param>
        internal BooleanFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type)
            : base(value, parameterExpression, filterOperator, type)
        {
            BooleanFilterOperator operation;
            if (!Enum.TryParse<BooleanFilterOperator>(filterOperator, out operation))
            {
                throw new InvalidOperationException("Invalid filter operation for a string '" + filterOperator + "'.");
            }

            _filterOperator = operation;
        }

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;
            switch (_filterOperator)
            {
                case BooleanFilterOperator.IsTrue:
                    body = Expression.Equal(property, Expression.Constant(true, property.Type));
                    break;
                case BooleanFilterOperator.IsFalse:
                    body = Expression.Equal(property, Expression.Constant(false, property.Type));
                    break;
                case BooleanFilterOperator.EqualTo:
                    rawValue = rawValue == "1" ? "true" : rawValue == "0" ? "false" : rawValue;
                    bool value = (bool)BlueearthConvert.ChangeType(rawValue, property.Type);
                    Expression valueExpression = Expression.Constant(value, property.Type);
                    body = Expression.Equal(property, valueExpression);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a boolean.");
            }

            return body;
        }
    }
}

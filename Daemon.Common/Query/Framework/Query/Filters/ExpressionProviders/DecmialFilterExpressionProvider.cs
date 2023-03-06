using System;
using System.Linq.Expressions;
using Daemon.Common.Query.Framework.Filters.Operators;
using Daemon.Common;

namespace Daemon.Common.Query.Filters.ExpressionProviders
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class DecimalFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly NumericFilterOperator _filterOperator;
        private readonly int _numberPrecision;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalFilterExpressionProvider{TEntity}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The paramter expression.</param>
        /// <param name="filterOperator">The filter operation.</param>
        internal DecimalFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, int numberPrecision, Type type)
            : base(value, parameterExpression, filterOperator, type)
        {
            _numberPrecision = numberPrecision;
            NumericFilterOperator numericfilterOperator;
            if (!Enum.TryParse<NumericFilterOperator>(filterOperator, out numericfilterOperator))
            {
                throw new InvalidOperationException("Invalid filter operation for a number '" + numericfilterOperator + "'.");
            }

            _filterOperator = numericfilterOperator;
        }

        protected override Expression RawValueTypeSpecficExpression(MemberExpression property, string rawValue)
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

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;
            decimal number;
            decimal.TryParse(rawValue, out number);
            if ((property.Member.Name.ToLower().Contains("xcoord") || property.Member.Name.ToLower().Contains("ycoord")) && (number >= 1000 || number <= -1000))
            {
                rawValue = "999.99";
            }

            decimal.TryParse(rawValue, out number);
            object value = BlueearthConvert.ChangeType(rawValue, property.Type);
            var valueExpression = Expression.Constant(value, property.Type);

            decimal lowValue = number - (decimal)(Math.Pow(10, -1 * _numberPrecision) / 2);
            decimal highValue = number + (decimal)(Math.Pow(10, -1 * _numberPrecision) / 2);

            Expression lowerBound = Expression.Constant(BlueearthConvert.ChangeType(lowValue, property.Type), property.Type);
            Expression higherBound = Expression.Constant(BlueearthConvert.ChangeType(highValue, property.Type), property.Type);

            switch (_filterOperator)
            {
                case NumericFilterOperator.EqualTo:
                    if (IsFullDigitalsMatch(property))
                    {
                        body = Expression.Equal(property, valueExpression);
                    }
                    else
                    {
                        body = Expression.AndAlso(
                        Expression.GreaterThanOrEqual(property, lowerBound),
                        Expression.LessThan(property, higherBound));
                    }

                    break;
                case NumericFilterOperator.NotEqualTo:
                    body = Expression.OrElse(
                        Expression.LessThan(property, lowerBound),
                        Expression.GreaterThan(property, higherBound));
                    break;
                case NumericFilterOperator.GreaterThan:
                    body = Expression.GreaterThan(property, higherBound);
                    break;
                case NumericFilterOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(property, lowerBound);
                    break;
                case NumericFilterOperator.LessThan:
                    body = Expression.LessThan(property, lowerBound);
                    break;
                case NumericFilterOperator.LessThanOrEqualTo:
                    body = Expression.LessThanOrEqual(property, higherBound);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }

        /// <summary>
        /// xcoord and ycoord are both six decimal digits in database and front, use equal function to match the number.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool IsFullDigitalsMatch(MemberExpression property)
        {
            if (property.Member != null)
            {
                switch (property.Member.Name)
                {
                    case "Ycoord":
                    case "Xcoord":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}

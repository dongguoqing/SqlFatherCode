using System;
using System.Linq.Expressions;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.ExpressionProviders
{
    internal class DateTimeFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly DateFilterOperator _filterOperator;
        private readonly bool _secondSensitive;

        internal DateTimeFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type, bool secondSensitive = false)
            : base(value, parameterExpression, filterOperator, type)
        {
            DateFilterOperator dateFilterOperator;
            if (!Enum.TryParse(filterOperator, out dateFilterOperator))
            {
                throw new InvalidOperationException("Invalid filter operation for a number '" + filterOperator + "'.");
            }

            _filterOperator = dateFilterOperator;
            _secondSensitive = secondSensitive;
        }

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            var rawDateTime = (BlueearthConvert.ChangeType(rawValue, property.Type) as DateTime?).Value;
            var endTime = rawDateTime;
            var startTime = rawDateTime;
            if (!_secondSensitive)
            {
                startTime = new DateTime(rawDateTime.Year, rawDateTime.Month, rawDateTime.Day, rawDateTime.Hour, rawDateTime.Minute, 0);
                endTime = startTime.AddMinutes(1).AddMilliseconds(-1);
            }

            Expression startMinute = Expression.Constant(startTime, property.Type);
            Expression endMinute = Expression.Constant(endTime, property.Type);

            Expression body;
            switch (_filterOperator)
            {
                case DateFilterOperator.EqualTo:
                    if (_secondSensitive)
                    {
                        body = Expression.Equal(property, startMinute);
                    }
                    else
                    {
                        body = Expression.AndAlso(
                            Expression.GreaterThanOrEqual(property, startMinute),
                            Expression.LessThan(property, endMinute));
                    }

                    break;
                case DateFilterOperator.NotEqualTo:
                    if (_secondSensitive)
                    {
                        body = Expression.NotEqual(property, startMinute);
                    }
                    else
                    {
                        body = Expression.OrElse(
                            Expression.LessThan(property, startMinute),
                            Expression.GreaterThanOrEqual(property, endMinute));
                    }

                    break;
                case DateFilterOperator.After:
                case DateFilterOperator.GreaterThan:
                    body = Expression.GreaterThan(property, startMinute);
                    break;
                case DateFilterOperator.OnOrAfter:
                case DateFilterOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(property, startMinute);
                    break;
                case DateFilterOperator.Before:
                case DateFilterOperator.LessThan:
                    body = Expression.LessThan(property, startMinute);
                    break;
                case DateFilterOperator.OnOrBefore:
                case DateFilterOperator.LessThanOrEqualTo:
                    body = Expression.LessThanOrEqual(property, endMinute);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }
    }
}

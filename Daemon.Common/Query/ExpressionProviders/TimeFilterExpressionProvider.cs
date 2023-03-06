using System;
using System.Linq.Expressions;
using Daemon.Common.Query.Framework.Filters.Operators;

namespace Daemon.Common.Query.ExpressionProviders
{
    internal class TimeFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly DateFilterOperator _filterOperator;

        internal TimeFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type)
            : base(value, parameterExpression, filterOperator, type)
        {
            DateFilterOperator dateFilterOperator;
            if (!Enum.TryParse<DateFilterOperator>(filterOperator, out dateFilterOperator))
            {
                throw new InvalidOperationException("Invalid filter operation for a number '" + filterOperator + "'.");
            }

            _filterOperator = dateFilterOperator;
        }

        protected override Expression RawValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;

            Expression startMinute, endMinute;
            if (property.Type.FullName.Contains("TimeSpan"))
            {
                DateTime rawDateTime = (BlueearthConvert.ChangeType(rawValue, typeof(DateTime)) as DateTime?).Value;
                startMinute = Expression.Constant(rawDateTime.TimeOfDay, property.Type);
                endMinute = Expression.Constant(rawDateTime.AddMinutes(1).TimeOfDay, property.Type);
            }
            else
            {
                DateTime rawDateTime = (BlueearthConvert.ChangeType(rawValue, property.Type) as DateTime?).Value;
                DateTime time = new DateTime(1899, 12, 30, rawDateTime.Hour, rawDateTime.Minute, 0);

                startMinute = Expression.Constant(time, property.Type);
                endMinute = Expression.Constant(time.AddMinutes(1), property.Type);
            }

            switch (_filterOperator)
            {
                case DateFilterOperator.EqualTo:
                    body = Expression.And(
                        Expression.GreaterThanOrEqual(property, startMinute),
                        Expression.LessThan(property, endMinute));
                    break;
                case DateFilterOperator.NotEqualTo:
                    body = Expression.NotEqual(property, startMinute);
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
                    body = Expression.LessThanOrEqual(property, startMinute);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;
            Expression targetMinute, resolvedMinute;
            if (property.Type.FullName.Contains("TimeSpan"))
            {
                DateTime rawDateTime = (BlueearthConvert.ChangeType(rawValue, typeof(DateTime)) as DateTime?).Value;
                targetMinute = Expression.Constant(rawDateTime.TimeOfDay, property.Type);
                resolvedMinute = Expression.Constant(rawDateTime.AddMinutes(1).TimeOfDay, property.Type);
            }
            else
            {
                DateTime rawDateTime = (BlueearthConvert.ChangeType(rawValue, property.Type) as DateTime?).Value;
                DateTime time = new DateTime(1899, 12, 30, rawDateTime.Hour, rawDateTime.Minute, 0);

                targetMinute = Expression.Constant(time, property.Type);
                resolvedMinute = Expression.Constant(time.AddMinutes(1), property.Type);
            }

            switch (_filterOperator)
            {
                case DateFilterOperator.EqualTo:
                    body = Expression.AndAlso(
                        Expression.GreaterThanOrEqual(property, targetMinute),
                        Expression.LessThan(property, resolvedMinute));
                    break;
                case DateFilterOperator.NotEqualTo:
                    body = Expression.OrElse(
                        Expression.LessThan(property, targetMinute),
                        Expression.GreaterThanOrEqual(property, resolvedMinute));
                    break;
                case DateFilterOperator.After:
                case DateFilterOperator.GreaterThan:
                    body = Expression.GreaterThan(property, targetMinute);
                    break;
                case DateFilterOperator.OnOrAfter:
                case DateFilterOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(property, targetMinute);
                    break;
                case DateFilterOperator.Before:
                case DateFilterOperator.LessThan:
                    body = Expression.LessThan(property, targetMinute);
                    break;
                case DateFilterOperator.OnOrBefore:
                case DateFilterOperator.LessThanOrEqualTo:
                    body = Expression.LessThanOrEqual(property, targetMinute);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }
    }
}

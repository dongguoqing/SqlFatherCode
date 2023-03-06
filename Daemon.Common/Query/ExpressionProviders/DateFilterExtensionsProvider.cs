using Daemon.Common.Query.ExpressionProviders;
using Daemon.Common.Query.Framework.Filters.Operators;
using System.Linq.Expressions;
using System;
using Daemon.Data.Substructure.Helpers;
namespace Daemon.Common.Query.ExpressionProviders
{
    public static class DateFilterExtensions
    {
        public const string DATE_FILTER_ISWITHIN_NAME = "IsWithin";
        public const string DATE_FILTER_ISWITHIN_NAME_FOR_EXPRESSION = "<=>";

        public static bool IsWithin(this DateTime? dateTime, string days)
        {
            if (double.TryParse(days, out double parameter))
            {
                return dateTime <= (DateTime.Now).AddDays(parameter);
            }

            return dateTime <= (DateTime.Now);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal class DateFilterExpressionProvider<TEntity> : ValueTypeFilterExpressionProviderBase<TEntity>
        where TEntity : class
    {
        private readonly DateFilterOperator _filterOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateFilterExpressionProvider{TEntity}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The paramter expression.</param>
        /// <param name="filterOperator">The filter operation.</param>
        internal DateFilterExpressionProvider(string value, ParameterExpression parameterExpression, string filterOperator, Type type)
            : base(value, parameterExpression, filterOperator, type)
        {
            DateFilterOperator dateFilterOperator;
            if (!Enum.TryParse(filterOperator, out dateFilterOperator))
            {
                throw new InvalidOperationException("Invalid filter operation for a number '" + filterOperator + "'.");
            }

            _filterOperator = dateFilterOperator;
        }

        protected override Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            Expression body = null;
            if (_filterOperator == DateFilterOperator.IsWithIn)
            {
                double.TryParse(rawValue, out double days);
                var clientDateTime = DateTime.Now;
                DateTime.TryParse(clientDateTime.ToShortDateString(), out DateTime currentDateTime);
                DateTime.TryParse(clientDateTime.AddDays(days).ToShortDateString(), out DateTime queryDate);
                if (DateTime.Compare(queryDate, currentDateTime) > 0)
                {
                    return Expression.AndAlso(Expression.LessThanOrEqual(property, Expression.Constant(queryDate, property.Type)), Expression.GreaterThanOrEqual(property, Expression.Constant(currentDateTime, property.Type)));
                }
                else if (DateTime.Compare(queryDate, currentDateTime) < 0)
                {
                    return Expression.AndAlso(Expression.LessThanOrEqual(property, Expression.Constant(currentDateTime, property.Type)), Expression.GreaterThanOrEqual(property, Expression.Constant(queryDate, property.Type)));
                }
                else
                {
                    return Expression.AndAlso(Expression.GreaterThanOrEqual(property, Expression.Constant(currentDateTime, property.Type)), Expression.LessThan(property, Expression.Constant(queryDate.AddDays(1), property.Type)));
                }
            }

            DateTime? rawDateTime = (BlueearthConvert.ChangeType(rawValue, property.Type) as DateTime?).Value.Date;
            DateTime? nextDay = rawDateTime.Value.AddDays(1);

            Expression currentDayStart = Expression.Constant(rawDateTime, property.Type);
            Expression nextDayStart = Expression.Constant(nextDay, property.Type);

            switch (_filterOperator)
            {
                case DateFilterOperator.EqualTo:
                    body = Expression.AndAlso(Expression.GreaterThanOrEqual(property, currentDayStart), Expression.LessThan(property, nextDayStart));
                    break;
                case DateFilterOperator.NotEqualTo:
                    body = Expression.OrElse(Expression.OrElse(Expression.LessThan(property, currentDayStart), Expression.GreaterThanOrEqual(property, nextDayStart)), Expression.Equal(Expression.Convert(property, typeof(DateTime?)), Expression.Constant(null, typeof(DateTime?))));
                    break;
                case DateFilterOperator.After:
                case DateFilterOperator.GreaterThan:
                    body = Expression.GreaterThanOrEqual(property, nextDayStart);
                    break;
                case DateFilterOperator.OnOrAfter:
                case DateFilterOperator.GreaterThanOrEqualTo:
                    body = Expression.GreaterThanOrEqual(property, currentDayStart);
                    break;
                case DateFilterOperator.Before:
                case DateFilterOperator.LessThan:
                    body = Expression.LessThan(property, currentDayStart);
                    break;
                case DateFilterOperator.OnOrBefore:
                case DateFilterOperator.LessThanOrEqualTo:
                    body = Expression.LessThan(property, nextDayStart);
                    break;
                default:
                    throw new InvalidOperationException("The filter operation '" + _filterOperator.ToString() + "' is not valid for a number.");
            }

            return body;
        }

        protected override Expression RawValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            if (_filterOperator == DateFilterOperator.IsWithIn)
            {
                var rightValue = Expression.Constant(rawValue);
                var methodinfo = ExtensionMethodHelper.GetExtensionMethods(typeof(DateFilterExtensions).Assembly, typeof(DateTime?), DateFilterExtensions.DATE_FILTER_ISWITHIN_NAME);
                return Expression.Call(methodinfo, property, rightValue);
            }

            return ValueTypeSpecficExpression(property, rawValue);
        }
    }
}

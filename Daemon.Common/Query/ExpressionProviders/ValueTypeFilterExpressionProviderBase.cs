namespace Daemon.Common.Query.ExpressionProviders
{
    using System;
    using System.Linq.Expressions;
    using Daemon.Common.Reflection;
    internal abstract class ValueTypeFilterExpressionProviderBase<TEntity> : FilterExpressionProvider<TEntity>
         where TEntity : class
    {
        private readonly string _filterOperator;

        public ValueTypeFilterExpressionProviderBase(string value, ParameterExpression parameterExpression, string filterOperator, Type type)
            : base(value, parameterExpression, type)
        {
            _filterOperator = filterOperator;
        }

        protected sealed override Expression InitializeExpressionBody(MemberExpression property, string propertyName, string rawValue, bool convertToRawWhereClause = false)
        {
            Expression body = null;
            if (_filterOperator.ToLower() == "isnull" || _filterOperator.ToLower() == "isnotnull")
            {
                if (!property.Type.IsNullable())
                {
                    if (_filterOperator.ToLower() == "isnull")
                    {
                        Expression<Func<bool>> exp = () => false;
                        body = exp.Body;
                    }
                    else
                    {
                        Expression<Func<bool>> exp = () => true;
                        body = exp.Body;
                    }
                }
                else
                {
                    if (_filterOperator.ToLower() == "isnull")
                    {
                        // If the operator is checking for null, ignore the provided value type.
                        Expression valueExpression = Expression.Constant(null, property.Type);
                        body = Expression.Equal(property, valueExpression);
                    }
                    else
                    {
                        Expression valueExpression = Expression.Constant(null, property.Type);
                        body = Expression.NotEqual(property, valueExpression);
                    }
                }
            }
            else
            {
                if (convertToRawWhereClause)
                {
                    body = RawValueTypeSpecficExpression(property, rawValue);
                }
                else
                {
                    body = ValueTypeSpecficExpression(property, rawValue);
                }
            }

            return body;
        }

        protected virtual Expression RawValueTypeSpecficExpression(MemberExpression property, string rawValue)
        {
            return ValueTypeSpecficExpression(property, rawValue);
        }

        protected abstract Expression ValueTypeSpecficExpression(MemberExpression property, string rawValue);
    }
}

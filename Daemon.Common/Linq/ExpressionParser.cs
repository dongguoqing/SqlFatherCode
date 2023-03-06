namespace Daemon.Common.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Daemon.Common.Reflection;

    /// <summary>
    /// Contains methods that related to Linq expression parsing.
    /// </summary>
    public static class ExpressionParser
    {
        ///// <summary>
        ///// Creates the nested property expression.
        ///// </summary>
        ///// <param name="type">The type.</param>
        ///// <param name="propertyName">Name of the property.</param>
        ///// <returns></returns>
        // public static LambdaExpression CreateNestedPropertyExpression(Type type, string propertyName)
        // {
        // ParameterExpression parameterExpression = Expression.Parameter(type);
        // Expression body = parameterExpression;
        // foreach (string member in propertyName.Split('.'))
        // {
        // body = Expression.PropertyOrField(body, member);
        // }
        // return Expression.Lambda(body, parameterExpression);
        // }

        /// <summary>
        /// Creates the nested property expression.
        /// </summary>
        /// <param name="propertyMap">The property map.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="parameter">The parameted of the entire expression.</param>
        /// <returns></returns>
        public static MemberExpression CreateNestedPropertyExpression(PropertyMap propertyMap, string propertyName, ParameterExpression parameter)
        {
            IList<PropertyInfo> properties = propertyMap.FindNestedProperty(propertyName).ToList();

            MemberExpression property = Expression.Property(parameter, properties.First());
            for (int i = 1; i < properties.Count; i++)
            {
                property = Expression.Property(property, properties[i]);
            }

            return property;
        }

        /// <summary>
        /// Creates the property chain.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static string CreatePropertyChain(LambdaExpression expression)
        {
            MemberInfo propertyMemberInfo;
            return CreatePropertyChainAndMemberInfo(expression, out propertyMemberInfo);
        }

        /// <summary>
        /// Creates the property chain.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="propertyMemberInfo"></param>
        /// <returns></returns>
        public static string CreatePropertyChainAndMemberInfo(LambdaExpression expression, out MemberInfo propertyMemberInfo)
        {
            MemberExpression memberExpression = StripUnaryExpressions(expression.Body);

            MemberInfo firstMemberInfo = memberExpression.Member;
            MemberInfo lastMemberInfo = null;
            IList<string> propertyChain = new List<string>();

            while (memberExpression != null)
            {
                propertyChain.Add(memberExpression.Member.Name);
                lastMemberInfo = memberExpression.Member;

                Expression nextExpression = memberExpression.Expression;
                memberExpression = nextExpression as MemberExpression;
            }

            if (lastMemberInfo == null || propertyChain.Count == 0)
            {
                throw new InvalidOperationException("The lambda expression must be defined in terms of nested properties only.");
            }

            propertyMemberInfo = firstMemberInfo;
            return string.Join(".", propertyChain.Reverse().ToArray());
        }

        /// <summary>
        /// Strips the unary expressions from the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>A member expression.</returns>
        private static MemberExpression StripUnaryExpressions(Expression expression)
        {
            Expression returnValue = expression;
            while (expression != null && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked))
            {
                UnaryExpression unaryExpression = expression as UnaryExpression;
                if (unaryExpression != null)
                {
                    expression = unaryExpression.Operand;
                    returnValue = expression;
                }
            }

            return returnValue as MemberExpression;
        }
    }
}

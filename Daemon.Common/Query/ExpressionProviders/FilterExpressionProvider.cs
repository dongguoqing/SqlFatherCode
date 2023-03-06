using System;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Reflection;
using Daemon.Common.ExtensionMethod;
using Daemon.Common.Linq;
namespace Daemon.Common.Query.ExpressionProviders
{
    /// <summary>
    /// Contains members related to providing filter expressions.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    internal abstract class FilterExpressionProvider<TEntity>
        where TEntity : class
    {
        private readonly ParameterExpression _parameterExpression;
        private readonly PropertyMap _propertyMap;
        private readonly string _rawValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterExpressionProvider{TEntity}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        protected FilterExpressionProvider(string value, ParameterExpression parameterExpression, Type type)
        {
            _rawValue = value;
            _propertyMap = new PropertyMap(type ?? typeof(TEntity));
            _parameterExpression = parameterExpression;
        }

        /// <summary>
        /// Generates the expression body.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public Expression GenerateExpressionBody(string propertyName, bool convertToRawWhereClause)
        {
            MemberExpression propertyExpression = CreatePropertyExpression(propertyName);
            return InitializeExpressionBody(propertyExpression, propertyName, _rawValue, convertToRawWhereClause);
        }

        /// <summary>
        /// Generates the filter expression that can be evaluated by the ORM.
        /// </summary>
        /// <returns>An expression that can be evaluated by the ORM.</returns>
        public Expression<Func<TEntity, bool>> GenerateWhereExpression(string propertyName)
        {
            // Get the type of the expression parameter.
            // This is the type of 'o' in the expression 'o => o.SomeValue'.
            MemberExpression propertyExpression = CreatePropertyExpression(propertyName);
            Expression body = InitializeExpressionBody(propertyExpression, propertyName, _rawValue);

            if (body != null)
            {
                return Expression.Lambda<Func<TEntity, bool>>(body, _parameterExpression);
            }

            return null;
        }

        /// <summary>
        /// Finds the reflected method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Method information.</returns>
        protected MethodInfo FindReflectedMethod<T>(string methodName)
        {
            return FindReflectedMethod(typeof(T), methodName, typeof(T));
        }

        /// <summary>
        /// Finds the reflected method.
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="elementType"></param>
        /// <returns>
        /// Method information.
        /// </returns>
        protected MethodInfo FindReflectedMethod(Type listType, string methodName, Type elementType)
        {
            MethodInfo info = listType.GetMethod(methodName, new[] { elementType });
            if (info == null && listType == typeof(string))
            {
                info = typeof(StringExtensionMethod).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            }

            return info;
        }

        /// <summary>
        /// Initializes the expression body.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected abstract Expression InitializeExpressionBody(MemberExpression property, string propertyName, string rawValue, bool convertToRawWhereClause = false);

        /// <summary>
        /// Creates the property expression.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private MemberExpression CreatePropertyExpression(string propertyName)
        {
            return ExpressionParser.CreateNestedPropertyExpression(_propertyMap, propertyName, _parameterExpression);
        }
    }
}

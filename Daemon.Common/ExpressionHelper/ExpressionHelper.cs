namespace Daemon.Common.ExpressionHelper
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;

	public class ExpressionHelper
	{
		public static Expression GetQueryExpression<T>(Expression expression, ParameterExpression parameterExpression, T field, LinqOperatorEnum linqOperatorEnum)
		{
			ConstantExpression rightExpression = Expression.Constant(field, typeof(T));
			MemberExpression leftExpression = Expression.Property(parameterExpression, (PropertyInfo)GetMemberExpression(expression).Member);
			Expression binaryExpression = null;
			switch (linqOperatorEnum)
			{
				case LinqOperatorEnum.Equal:
					binaryExpression = Expression.Equal(leftExpression, rightExpression);
					break;
				case LinqOperatorEnum.NotEqual:
					binaryExpression = Expression.NotEqual(leftExpression, rightExpression);
					break;
				case LinqOperatorEnum.Contains:
					binaryExpression = Expression.Call(leftExpression, GetMethod<string>("Contains"), rightExpression);
					break;
				case LinqOperatorEnum.EndsWith:
					binaryExpression = Expression.Call(leftExpression, GetMethod<string>("EndsWith"), rightExpression);
					break;
				case LinqOperatorEnum.StartsWith:
					binaryExpression = Expression.Call(leftExpression, GetMethod<string>("StartsWith"), rightExpression);
					break;
			}

			return binaryExpression;
		}

		public static Expression GetQueryExpression<T>(Expression expression, ParameterExpression parameterExpression, IList<T> list, LinqOperatorEnum linqOperatorEnum)
		{
			ConstantExpression leftExpression = Expression.Constant(list);
			Expression rightExpression = Expression.Convert(Expression.Property(parameterExpression, (PropertyInfo)GetMemberExpression(expression).Member), typeof(object));
			Expression binaryExpression = null;
			switch (linqOperatorEnum)
			{
				case LinqOperatorEnum.Contains:
					binaryExpression = Expression.Call(leftExpression, GetMethod<IList>("Contains"), rightExpression);
					break;
				case LinqOperatorEnum.NotContains:
					binaryExpression = Expression.Not(Expression.Call(leftExpression, GetMethod<IList>("Contains"), rightExpression));
					break;
			}

			return binaryExpression;
		}

		public static MemberExpression GetMemberExpression(Expression expression)
		{
			if (expression is MemberExpression)
			{
				return (MemberExpression)expression;
			}
			else
			{
				return (MemberExpression)((UnaryExpression)expression).Operand;
			}
		}

		public static string GetMemberName(Expression expression)
		{
			return GetMemberExpression(expression).Member.Name;
		}

		public static Expression<Func<T, bool>> GetIdContainsLambda<T>(List<int> ids, Expression body)
		{
			return GetIdContainsExpression<T>(ids, body, true);
		}

		public static Expression<Func<T, bool>> GetIdNotContainsLambda<T>(List<int> ids, Expression body)
		{
			return GetIdContainsExpression<T>(ids, body, false);
		}

		private static Expression<Func<T, bool>> GetIdContainsExpression<T>(List<int> ids, Expression body, bool contain)
		{
			ConstantExpression values = Expression.Constant(ids);
			ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
			MemberExpression memberExp = Expression.Property(parameterExpression, (PropertyInfo)((MemberExpression)body).Member);
			MethodInfo containsMethod = ids.GetType().GetMethod("Contains", new[] { typeof(int) });
			MethodCallExpression methodCallExperssion = Expression.Call(values, containsMethod, memberExp);
			Expression<Func<T, bool>> lambda;
			if (contain)
			{
				lambda = Expression.Lambda<Func<T, bool>>(methodCallExperssion, parameterExpression);
			}
			else
			{
				lambda = Expression.Lambda<Func<T, bool>>(Expression.Not(methodCallExperssion), parameterExpression);
			}

			return lambda;
		}

		private static MethodInfo GetMethod<T>(string methodName)
		{
			return typeof(T).GetMethod(methodName, new Type[] { typeof(T) });
		}
	}
}

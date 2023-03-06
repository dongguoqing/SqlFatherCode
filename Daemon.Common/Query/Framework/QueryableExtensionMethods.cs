using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Daemon.Common.Query.Framework
{
    public static class IdContainsExtensionMethod
    {
        private const int PAGE_SIZE = 2000;
        private const int MAX_CONTAINS_DEEP = 400;

        private static int MaxContainsCount => PAGE_SIZE * MAX_CONTAINS_DEEP;

        private static int _maxContainsPageSize = 20000;

        public static IQueryable<T> PageContains<T, Td>(this IQueryable<T> queryable, IEnumerable<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            if (generics == null || !generics.Any())
            {
                return queryable.Where(r => false);
            }

            List<Td> list = null;
            IQueryable<T> union = null;
            for (int i = 0; i < generics.Count(); i += MaxContainsCount)
            {
                list = generics.Skip(i).Take(i + MaxContainsCount).ToList();
                if (union == null)
                {
                    union = queryable.Where(PageGetGenericsContainsLambda(list, getGenerics));
                }
                else
                {
                    union = union.Union(queryable.Where(PageGetGenericsContainsLambda(list, getGenerics)));
                }
            }

            return union;
        }

        public static IQueryable<T> Contains<T, Td>(this IQueryable<T> queryable, IEnumerable<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            if (generics == null || !generics.Any())
            {
                return queryable.Where(r => false);
            }

            List<Td> list = generics.ToList();
            var func = getGenerics.Compile();
            if (list.Count > _maxContainsPageSize)
            {
                var records = queryable.ToList();
                HashSet<Td> hashIds = new HashSet<Td>(generics);

                return records.Where(x => hashIds.Contains(func(x))).AsQueryable();
            }

            return queryable.Where(GetContainsExpression(generics, getGenerics));
        }

        public static IQueryable<T> PageNotContains<T, Td>(this IQueryable<T> queryable, IEnumerable<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            if (generics == null || !generics.Any())
            {
                return queryable.Where(r => false);
            }

            List<Td> list = null;
            IQueryable<T> union = null;
            for (int i = 0; i < generics.Count(); i += MaxContainsCount)
            {
                list = generics.Skip(i).Take(i + MaxContainsCount).ToList();
                if (union == null)
                {
                    union = queryable.Where(PageGetGenericsDoesNotContainLambda(list, getGenerics));
                }
                else
                {
                    union = union.Union(queryable.Where(PageGetGenericsDoesNotContainLambda(list, getGenerics)));
                }
            }

            return union;
        }

        private static MemberExpression GetPName<T, Td>(Expression<Func<T, Td>> expression)
        {
            if (expression.Body is MemberExpression)
            {
                return (MemberExpression)expression.Body;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                return (MemberExpression)op;
            }
        }

        private static Expression<Func<T, bool>> GetContainsExpression<T, Td>(IEnumerable<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            ConstantExpression values = Expression.Constant(generics);
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression memberExp = Expression.Property(parameterExpression, (PropertyInfo)GetPName(getGenerics).Member);
            var converted = Expression.Convert(memberExp, typeof(Td));
            MethodInfo containsMethod = generics.GetType().GetMethod("Contains", new[] { typeof(Td) });
            MethodCallExpression methodCallExpression = Expression.Call(values, containsMethod, converted);
            return Expression.Lambda<Func<T, bool>>(methodCallExpression, parameterExpression);
        }

        private static Expression<Func<T, bool>> GetGenericsContainsExpressionPaging<T, Td>(List<Td> generics, Expression<Func<T, Td>> getGenerics, bool contains)
        {
            int batchCount = 0;
            Expression<Func<T, bool>> lambda = null;
            int totalRemainingRecords = generics.Count();
            do
            {
                int take = totalRemainingRecords > 2000 ? PAGE_SIZE : totalRemainingRecords;
                List<Td> batch = generics.Skip(PAGE_SIZE * batchCount).Take(take).ToList();
                ConstantExpression values = Expression.Constant(batch);
                ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
                MemberExpression memberExp = Expression.Property(
                    parameterExpression,
                    (PropertyInfo)GetPName(getGenerics).Member);
                var converted = Expression.Convert(memberExp, typeof(Td));
                MethodInfo containsMethod = generics.GetType().GetMethod("Contains", new[] { typeof(Td) });
                MethodCallExpression methodCallExpression =
                    Expression.Call(values, containsMethod, converted);
                if (batchCount == 0)
                {
                    if (contains)
                    {
                        lambda = Expression.Lambda<Func<T, bool>>(
                            methodCallExpression,
                            parameterExpression);
                    }
                    else
                    {
                        lambda = Expression.Lambda<Func<T, bool>>(
                            Expression.Not(methodCallExpression),
                            parameterExpression);
                    }
                }
                else
                {
                    Expression<Func<T, bool>> lambdaBatch = null;
                    if (contains)
                    {
                        lambdaBatch = Expression.Lambda<Func<T, bool>>(
                            methodCallExpression,
                            parameterExpression);
                    }
                    else
                    {
                        lambdaBatch = Expression.Lambda<Func<T, bool>>(
                            Expression.Not(methodCallExpression),
                            parameterExpression);
                    }

                    var paramExpr = Expression.Parameter(typeof(T));
                    var exprBody = Expression.Or(lambda.Body, lambdaBatch.Body);
                    exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);
                    var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);
                    lambda = finalExpr;
                }

                totalRemainingRecords = totalRemainingRecords - PAGE_SIZE;
                ++batchCount;
            }
            while (totalRemainingRecords > 0);

            return lambda;
        }

        private static Expression<Func<T, bool>> PageGetGenericsContainsLambda<T, Td>(List<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            return GetGenericsContainsExpressionPaging(generics, getGenerics, true);
        }

        private static Expression<Func<T, bool>> PageGetGenericsDoesNotContainLambda<T, Td>(List<Td> generics, Expression<Func<T, Td>> getGenerics)
        {
            return GetGenericsContainsExpressionPaging(generics, getGenerics, false);
        }
    }

    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(_parameter);
        }
    }
}

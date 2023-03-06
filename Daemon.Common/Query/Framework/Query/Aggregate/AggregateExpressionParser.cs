using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Daemon.Common.Reflection;

namespace Daemon.Common.Query.Framework.Query.Aggregate
{
    public class AggregateExpressionParser<TEntity>
        where TEntity : class
    {
        private readonly PropertyMap _propertyMap;

        public AggregateExpressionParser(PropertyMap propertyMap)
        {
            _propertyMap = propertyMap;
        }

        public object GetAggregationResult(IQueryable<TEntity> queryable, AggregateItem aggregateItem)
        {
            string originalfiledName = aggregateItem.FieldName; // single fields in the DB may be split into two fields in Viewfinder,save the field name in viewfinder.
            aggregateItem.FieldName = TranslateDividedFieldName(aggregateItem.FieldName, typeof(TEntity).Name); // single fields in the DB being split into two fields in Viewfinder,translate the field name to the name in db (fix View-890)
            PropertyInfo property = _propertyMap.FindNestedProperty(aggregateItem.FieldName).First();
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            Expression propertyExpression = Expression.Property(parameterExpression, property);

            if (property.PropertyType == typeof(string))
            {
                MethodInfo trim = typeof(string).GetMethod("Trim", Type.EmptyTypes);
                propertyExpression = Expression.Call(propertyExpression, trim);
            }

            Expression conversion = Expression.Convert(propertyExpression, typeof(object));
            Expression<Func<TEntity, object>> projection = Expression.Lambda<Func<TEntity, object>>(conversion, parameterExpression);
            IQueryable<object> queryable2 = queryable.Select(projection);

            queryable2 = queryable2.Where(c => c != null);
            if (queryable2.Count() > 0 && queryable2.ToList().FirstOrDefault().GetType().Name.Equals("String"))
            {
                queryable2 = queryable2.Where(c => c as string != string.Empty);
            }

            return GetAggregateResult(queryable2, originalfiledName, aggregateItem);
        }

        public object GetAggregationResultEntityFramework(IQueryable<TEntity> queryable, AggregateItem aggregateItem)
        {
            string originalfiledName = aggregateItem.FieldName; // single fields in the DB may be split into two fields in Viewfinder,save the field name in viewfinder.
            aggregateItem.FieldName = TranslateDividedFieldName(aggregateItem.FieldName, typeof(TEntity).Name); // single fields in the DB being split into two fields in Viewfinder,translate the field name to the name in db (fix View-890)
            PropertyInfo property = _propertyMap.FindNestedProperty(aggregateItem.FieldName).First();

            var parameter = Expression.Parameter(typeof(TEntity), "o");

            Dictionary<string, PropertyInfo> sourceProperties = new Dictionary<string, PropertyInfo>();
            sourceProperties[aggregateItem.FieldName] = property;
            Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType(sourceProperties.Values);

            IEnumerable<MemberBinding> bindings = dynamicType.GetFields().Select(p => Expression.Bind(p, Expression.Property(parameter, sourceProperties[p.Name]))).OfType<MemberBinding>();
            var newExpression = Expression.New(dynamicType);
            var memberInit = Expression.MemberInit(Expression.New(dynamicType.GetConstructor(Type.EmptyTypes)), bindings);
            var lambda = Expression.Lambda<Func<TEntity, object>>(memberInit, parameter);

            IQueryable<object> queryable2 = queryable.Select(lambda).ToList().Select(x => dynamicType.GetField(aggregateItem.FieldName).GetValue(x)).AsQueryable();
            queryable2 = queryable2.Where(c => c != null);
            var isString = property.PropertyType == typeof(string);
            if (isString)
            {
                queryable2 = queryable2.Select(x => x.ToString().Trim());
            }

            if (isString && queryable2.Count() > 0)
            {
                queryable2 = queryable2.Where(c => c as string != string.Empty);
            }

            return GetAggregateResult(queryable2, originalfiledName, aggregateItem);
        }

        private object GetAggregateResult(IQueryable<object> queryable2, string originalfiledName, AggregateItem aggregateItem)
        {
            object result = null;
            int count = queryable2.Count();
            queryable2 = SeparateFieldValue(originalfiledName, typeof(TEntity).Name, queryable2);

            switch (aggregateItem.AggregateOperator)
            {
                case AggregateOperator.Count:
                    result = count;
                    break;
                case AggregateOperator.DistinctCount:
                    result = queryable2.Distinct().ToList().Count();
                    break;
                case AggregateOperator.Distinct100:
                    result = queryable2.Distinct().Take(100).ToList();
                    break;
                case AggregateOperator.Distinct:
                    result = queryable2.Distinct().ToList();
                    break;
                case AggregateOperator.Min:
                    result = queryable2.Min();
                    break;
                case AggregateOperator.Max:
                    result = queryable2.Max();
                    break;
                case AggregateOperator.Average:
                    if (count != 0)
                    {
                        result = queryable2.ToList().Average(c => (double)BlueearthConvert.ChangeType(c, typeof(double)));
                    }
                    else
                    {
                        result = 0;
                    }

                    break;
                case AggregateOperator.Sum:
                    result = queryable2.ToList().Sum(c => (double)BlueearthConvert.ChangeType(c, typeof(double)));
                    break;
                case AggregateOperator.Range:
                    if (count != 0)
                    {
                        result = new List<object>() { queryable2.Min(), queryable2.Max() };
                    }
                    else
                    {
                        result = new List<object>() { 0, 0 };
                    }

                    break;
            }

            return result;
        }

        /// <summary>
        /// single fields in the DB being split into two fields in Viewfinder,so split the value to fit the field(fix View-890).
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="entityName"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        private IQueryable<object> SeparateFieldValue(string fieldName, string entityName, IQueryable<object> queryable)
        {
            if (entityName != "FieldTripGridEntity")
            {
                return queryable;
            }

            var dateList = queryable.ToList();
            switch (fieldName.ToUpper())
            {
                case "DEPARTDATE":
                case "RETURNDATE":
                    for (int i = 0; i < dateList.Count(); i++)
                    {
                        dateList[i] = DateTime.Parse(((DateTime)dateList[i]).ToString("MM/dd/yyyy") + " 00:00:00"); // need Full time format to compare
                    }

                    return dateList.AsQueryable();
                case "DEPARTTIME":
                case "RETURNTIME":
                    for (int i = 0; i < dateList.Count(); i++)
                    {
                        dateList[i] = DateTime.Parse("2016/4/29 " + ((DateTime)dateList[i]).ToString("hh:mm tt")); // need Full time format to compare
                    }

                    return dateList.AsQueryable();
                default:
                    return queryable;
            }
        }

        /// <summary>
        /// single fields in the DB being split into two fields in Viewfinder,translate the field name to the name in db (fix View-890).
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string TranslateDividedFieldName(string fieldName, string entityName)
        {
            if (entityName != "FieldTripGridEntity")
            {
                return fieldName;
            }

            switch (fieldName.ToUpper())
            {
                case "DEPARTDATE":
                case "DEPARTTIME":
                    return "DepartDateTime";
                case "RETURNDATE":
                case "RETURNTIME":
                    return "EstimatedReturnDateTime";
                default:
                    return fieldName;
            }
        }
    }
}

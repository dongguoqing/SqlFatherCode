using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
namespace Daemon.Common.Query.Framework
{
    public static class ExtensionMethods
    {
        public static decimal NullableSum(this IEnumerable<float?> values)
        {
            return values.ToList().Select(v => v.HasValue ? decimal.Parse(v.ToString()) : 0).Sum();
        }

        public static decimal NullableSum(this IEnumerable<decimal> values)
        {
            return values.Sum();
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, List<string> skipfields)
            where T : class
        {
            var type = typeof(T);
            var dataTable = new DataTable(type.Name);
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(r => !skipfields.Contains(r.Name)).ToArray();
            foreach (var prop in propertyInfos)
            {
                dataTable.Columns.Add(prop.Name);
            }

            var propertyCount = propertyInfos.Length;
            foreach (T item in collection)
            {
                var values = dataTable.NewRow();
                for (int i = 0; i < propertyCount; i++)
                {
                    values[i] = propertyInfos[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }
    }
}

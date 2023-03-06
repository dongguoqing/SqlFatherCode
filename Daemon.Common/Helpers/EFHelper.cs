using System.Collections.Generic;
using System;
using Daemon.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Linq;
namespace Daemon.Common.Helpers
{
    public static class Helper
    {
        public static List<T> RawSqlQuery<T>(this ApiDBContent context, string query, object[] parameters, Func<DbDataReader, T> map)
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.Add(parameters);
                }
                context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }

        public static IEnumerable<T> Select<T>(this IDataReader reader,
                                       Func<IDataReader, T> projection)
        {
            while (reader.Read())
            {
                yield return projection(reader);
            }
        }

        public static IQueryable<string> ExtColumn<TEntity>(this ApiDBContent context)
        {
            var entityType = context.Model.GetEntityTypes().Where(a => a.Name.IndexOf("." + typeof(TEntity).Name) > -1).FirstOrDefault();
            var properties = entityType.GetProperties().Select(r => r.Name);
            var allProperties = typeof(TEntity).GetProperties();
            var extProperties = allProperties.Where(r => !properties.Contains(r.Name)).Select(r => r.Name);
            return extProperties.AsQueryable();
        }

        public static IQueryable<string> Column<TEntity>(this ApiDBContent context)
        {
            var entityType = context.Model.GetEntityTypes().Where(a => a.Name.IndexOf("." + typeof(TEntity).Name) > -1).FirstOrDefault();
            var properties = entityType.GetProperties();
            List<string> allColumns = properties
                 .Select(x => x.Name)
                 .ToList();
            return allColumns.AsQueryable();
        }
    }
}

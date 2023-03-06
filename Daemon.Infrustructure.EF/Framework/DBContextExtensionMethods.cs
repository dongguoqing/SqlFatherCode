using Daemon.Model.Entities.DBContextExtension;
using Daemon.Model;
using Daemon.Common.Middleware;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata;
namespace Daemon.Infrustructure.EF.Framework
{
    public static class DBContextExtensionMethods
    {
        private static IDbContextExtensionImplementation _defaultImplementation = new DbContextExtensionImplementation();

        public static IDbContextExtensionImplementation Implementation { get; set; } = _defaultImplementation;

        public static TableMapping GetMapping<T>(this ApiDBContent context)
        {
            return Implementation.GetMapping<T>(context);
        }

        public static TableMapping GetMapping(this ApiDBContent context, string tableName)
        {
            return Implementation.GetMapping(context, tableName);
        }

        public static EntityPersistenceMap GetPersistenceMap<T>(this ApiDBContent context)
        {
            return Implementation.GetPersistenceMap<T>(context);
        }

        public static EntityPersistenceMap GetPersistenceMap(this ApiDBContent context, Type type)
        {
            return Implementation.GetPersistenceMap(context, type);
        }

        public static List<string> GetColumnNames<T>(this ApiDBContent context)
             where T : class
        {
            return Implementation.GetColumnNames<T>(context);
        }

        public static string GetTableName<T>(this ApiDBContent context)
             where T : class
        {
            return Implementation.GetTableName<T>(context);
        }

        public static List<TElement> SqlQuery<TElement>(this ApiDBContent context, string sql, params object[] parameters) where TElement : class, new()
        {
            return Implementation.SqlQuery<TElement>(context, sql, parameters);
        }
    }

    public class DbContextExtensionImplementation : IDbContextExtensionImplementation
    {
        private const string CACHE_NAME = "EnterpriseTableMapping";
        private static readonly Lazy<IDictionary<Type, Type>> _entityRepositories = new Lazy<IDictionary<Type, Type>>(GetMappings);

        private Dictionary<Type, EntityPersistenceMap> PersistenceMaps { get; set; } = new Dictionary<Type, EntityPersistenceMap>();

        public EntityPersistenceMap GetPersistenceMap<T>(ApiDBContent context)
        {
            var type = typeof(T);
            return context.GetPersistenceMap(type);
        }

        public EntityPersistenceMap GetPersistenceMap(ApiDBContent context, Type type)
        {
            lock (PersistenceMaps)
            {
                if (PersistenceMaps.ContainsKey(type))
                {
                    return PersistenceMaps[type];
                }

                EntityPersistenceMap entityPersistenceMap = new EntityPersistenceMap();
                var entitySet = context.ChangeTracker.Entries().Where(r => r.GetType() == type).Select(r => r.Entity);
                var entityType = context.ChangeTracker.Entries().Select(r => r.Metadata).FirstOrDefault(r => r.GetType() == type);

                entityPersistenceMap.TableName = entityType.GetTableName();
                var columnNameMapping = entityType.GetProperties();

                foreach (var propertyMapping in columnNameMapping)
                {
                    if (!entityPersistenceMap.PersistenceMap.ContainsKey(propertyMapping.Name))
                    {
                        entityPersistenceMap.PersistenceMap[propertyMapping.Name] = propertyMapping;
                    }
                }

                return entityPersistenceMap;
            }
        }

        public TableMapping GetMapping<T>(ApiDBContent context)
        {
            return GetMappings(context, typeof(T)).FirstOrDefault();
        }

        public TableMapping GetMapping(ApiDBContent context, string tableName)
        {
            return GetMappings(context).FirstOrDefault(i => i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetColumnNames<T>(ApiDBContent context)
             where T : class
        {
            return context.Model.FindEntityType(Daemon.Common.Const.SystemConst.Model_Name_Space + typeof(T).Name).GetProperties().Select(x => x.Name).ToList();
        }

        public string GetTableName<T>(ApiDBContent context)
             where T : class
        {
            var entityType = context.Model.FindEntityType(Daemon.Common.Const.SystemConst.Model_Name_Space + typeof(T).Name);
            return entityType.GetTableName();
        }

        public List<TElement> SqlQuery<TElement>(ApiDBContent context, string sql, params object[] parameters) where TElement : class, new()
        {
            var dbSet = context.Set<TElement>();
            return dbSet.FromSqlRaw(sql, parameters).ToList();
        }

        private static IEnumerable<TableMapping> GetMappings(ApiDBContent context, Type type = null)
        {
            List<TableMapping> tableMappings = new List<TableMapping>();
            var entityTypes = context.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                string entityName = entityType.DisplayName();
                if (type != null && entityName != type.Name)
                {
                    continue;
                }

                TableMapping tableMapping = GetFromCache(entityName);

                if (tableMapping == null)
                {
                    tableMapping = new TableMapping()
                    {
                        EntityName = entityName,
                        TableName = entityType.GetTableName(),
                        ColumnMappings = entityType.GetProperties().Select(r => new ColumnMapping()
                        {
                            ColumnName = r.GetColumnBaseName(),
                            PropertyName = r.Name,
                            PropertyInfo = r.PropertyInfo
                        }),
                        EntityType = entityType.GetType(),
                    };

                    if (_entityRepositories.Value.TryGetValue(entityType.GetType(), out Type repositoryType))
                    {
                        tableMapping.RepositoryType = repositoryType;
                    }

                    InsertToCache(entityName, tableMapping);
                }

                tableMappings.Add(tableMapping);
            }

            return tableMappings;
        }

        private static IDictionary<Type, Type> GetMappings()
        {
            var baseRepositoryType = typeof(Repository<,>);
            var repAssembly = Assembly.GetAssembly(baseRepositoryType);
            var repositories = repAssembly.GetTypes().Select(i =>
            {
                if (!i.GetGenericArguments().Any() && IsSubclassOfRawGeneric(baseRepositoryType, i, out Type[] genericArguments) && genericArguments.Length == 3)
                {
                    return new KeyValuePair<Type, Type>(genericArguments[1], i);
                }

                return default;
            }).Where(i => i.Key != null);

            var map = new Dictionary<Type, List<Type>>();
            foreach (var r in repositories)
            {
                if (!map.TryGetValue(r.Key, out List<Type> types))
                {
                    types = new List<Type>();
                    map.Add(r.Key, types);
                }

                types.Add(r.Value);
            }

            return map.ToDictionary(i => i.Key, i => i.Value.First());
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck, out Type[] genericArguments)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    genericArguments = toCheck.GetGenericArguments();
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            genericArguments = Array.Empty<Type>();
            return false;
        }

        private static void InsertToCache(string entityName, TableMapping tableMapping)
        {
            ServiceLocator.Resolve<IMemoryCache>().Set(entityName + CACHE_NAME, tableMapping, DateTimeOffset.Now.AddSeconds(24 * 3600));
        }

        private static TableMapping GetFromCache(string entityName)
        {
            return ServiceLocator.Resolve<IMemoryCache>().Get(entityName) as TableMapping;
        }
    }

    public interface IDbContextExtensionImplementation
    {
        TableMapping GetMapping<T>(ApiDBContent context);

        TableMapping GetMapping(ApiDBContent context, string tableName);

        EntityPersistenceMap GetPersistenceMap<T>(ApiDBContent context);

        EntityPersistenceMap GetPersistenceMap(ApiDBContent context, Type type);

        string GetTableName<T>(ApiDBContent context)
            where T : class;

        List<string> GetColumnNames<T>(ApiDBContent context)
            where T : class;

        List<TElement> SqlQuery<TElement>(ApiDBContent context, string sql, params object[] parameters) where TElement : class, new();
    }
}

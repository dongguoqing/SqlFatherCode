using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
namespace Daemon.Model
{
    public class PersistenceMapProvider
    {
        public static Dictionary<Type, EntityPersistenceMap> PersistenceMaps { get; set; } =
            new Dictionary<Type, EntityPersistenceMap>();

        public static EntityPersistenceMap GetPersistenceMap<T>(ApiDBContent context)
        {
            var type = typeof(T);
            return GetPersistenceMap(type, context);
        }

        public static EntityPersistenceMap GetPersistenceMap(Type type, ApiDBContent context)
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
                entityPersistenceMap.TableName =  entityType.GetTableName(); ;

                var columnNameMapping = entityType.GetProperties();

                foreach (var scalarPropertyMapping in columnNameMapping)
                {
                    if (!entityPersistenceMap.PersistenceMap.ContainsKey(scalarPropertyMapping.Name))
                    {
                        entityPersistenceMap.PersistenceMap[scalarPropertyMapping.Name] = scalarPropertyMapping;
                    }
                }

                PersistenceMaps[type] = entityPersistenceMap;
                return entityPersistenceMap;
            }
        }
    }

    public class EntityPersistenceMap
    {
        public string TableName { get; set; }

        public Dictionary<string, IProperty> PersistenceMap { get; set; }

        public EntityPersistenceMap()
        {
            PersistenceMap = new Dictionary<string, IProperty>();
        }
    }
}

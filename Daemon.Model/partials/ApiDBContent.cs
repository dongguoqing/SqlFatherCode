using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using Daemon.Data.Substructure.Helpers;

namespace Daemon.Model
{
    public partial class ApiDBContent
    {
        public override int SaveChanges()
        {
            var entities = ChangeTracker.Entries().ToArray();
            var addedEntities = GetEntities(entities.Where(p => p.State == EntityState.Added));
            var changedEntities = GetEntities(entities.Where(p => p.State == EntityState.Modified));
            var deletedEntities = GetEntities(entities.Where(p => p.State == EntityState.Deleted));

            if (deletedEntities.Any())
            {
                NotificationHelper.Instance.OnEntitiesDeleted?.Invoke(deletedEntities);
            }

            if (addedEntities.Any())
            {
                NotificationHelper.Instance.OnEntitiesAdded?.Invoke(addedEntities);
            }

            if (changedEntities.Any())
            {
                NotificationHelper.Instance.OnEntitiesChanged?.Invoke(changedEntities);
            }
            return base.SaveChanges();
        }

        private Dictionary<Type, List<object>> GetEntities(IEnumerable<EntityEntry> sourceEntities)
        {
            var entitiesDict = new Dictionary<Type, List<object>>();

            foreach (var entity in sourceEntities.Select(r => r.Entity))
            {
                var entityType = entity.GetType();
                if (!entitiesDict.TryGetValue(entityType, out List<object> list))
                {
                    list = new List<object>() { entity };
                    entitiesDict.Add(entityType, list);
                    continue;
                }

                list.Add(entity);
            }

            return entitiesDict;
        }
    }
}
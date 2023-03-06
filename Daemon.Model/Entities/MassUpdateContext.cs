using System;
using System.Collections.Generic;
using System.Linq;
using Daemon.Model.Entities.DBContextExtension;

namespace Daemon.Model.Entities
{
    public class MassUpdateContext<T>
    {
        private readonly Func<T, int> _expressionGetter;

        public MassUpdateContext(MassUpdateSettings settings, List<T> entities, Func<T, int> expressionGetter, TableMapping tableMapping)
        {
            Settings = settings;
            Entities = entities;
            _expressionGetter = expressionGetter;
            TableMapping = tableMapping;
            PropertyColumnMap = tableMapping.ColumnMappings.ToDictionary(i => i.PropertyName, i => i.ColumnName, StringComparer.OrdinalIgnoreCase);
        }

        public MassUpdateResult Result { get; } = new MassUpdateResult();

        public TableMapping TableMapping { get; }

        public bool Failed { get; set; }

        public MassUpdateSettings Settings { get; }

        public List<T> Entities { get; }

        public ApiDBContent DataContext { get; }

        public IDictionary<string, string> PropertyColumnMap { get; }

        public int GetId(T entity)
        {
            return _expressionGetter(entity);
        }

        public void MarkAllUpdated()
        {
            Result.UpdatedIds = Result.UpdatedIds.Union(Settings.Ids).ToList();
        }

        public void MarkAllFailed()
        {
            Result.FailedIds = Result.FailedIds.Union(Settings.Ids).ToList();
        }

        public void MarkUpdated(IEnumerable<int> ids)
        {
            Result.UpdatedIds = Result.UpdatedIds.Union(ids).ToList();
        }

        public void MarkUpdated(IEnumerable<T> entities)
        {
            var ids = entities.Select(GetId);
            MarkUpdated(ids);
        }

        public void MarkFailed(IEnumerable<int> ids)
        {
            Result.FailedIds = Result.FailedIds.Union(ids).ToList();
        }

        public void MarkFailed(IEnumerable<T> entities)
        {
            var ids = entities.Select(GetId);
            MarkFailed(ids);
        }

        public void MarkIgnored(IEnumerable<T> entities)
        {
            var ids = entities.Select(GetId);
            Result.IgnoredIds.AddRange(ids);
        }

        public void Terminate(string message)
        {
            var all = Settings.Ids;
            var rest = all.Where(i => !Result.UpdatedIds.Contains(i) && !Result.FailedIds.Contains(i) && !Result.IgnoredIds.Contains(i));
            Result.IgnoredIds.AddRange(rest);
            Result.Error = message;
            Failed = true;
        }
    }
}

using System;
using System.Collections.Generic;
namespace Daemon.Model.Entities.DBContextExtension
{
    public class TableMapping
    {
        public IEnumerable<ColumnMapping> ColumnMappings { get; set; }

        public string EntityName { get; set; }

        public string TableName { get; set; }

        public Type EntityType { get; set; }

        public Type RepositoryType { get; set; }
    }
}

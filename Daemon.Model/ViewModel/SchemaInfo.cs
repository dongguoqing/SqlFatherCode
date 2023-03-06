using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daemon.Model.ViewModel
{
    public class SchemaInfo
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string DatabaseName { get; set; }

        public string FieldType{get;set;}
    }
}
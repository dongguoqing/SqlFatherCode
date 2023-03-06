namespace Daemon.Model.ViewModel
{
    using System.Collections.Generic;
    public class TableInfo
    {
        public string TableName { get; set; }

        public string Comment { get; set; }

        public int MockNum { get; set; }

        public List<Field> FieldList { get; set; }

        public string DbName { get; set; }
    }

    public class Field
    {
        public bool PrimaryKey { get; set; }

        public bool OnUpdate { get; set; }

        public bool NotNull { get; set; }

        public bool MockType { get; set; }

        public bool MockParams { get; set; }

        public bool FieldType { get; set; }

        public bool FieldName { get; set; }

        public bool DefaultValue { get; set; }

        public bool Comment { get; set; }

        public bool AutoIncrement { get; set; }
    }
}

using System.Reflection;
namespace Daemon.Model.Entities.DBContextExtension
{
    public class ColumnMapping
    {
        public string PropertyName { get; set; }

        public string ColumnName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }
}

using Daemon.Data.Substructure.Enums;
using System.Collections.Generic;
namespace Daemon.Model.Entities
{
    public class MassUpdateSettings
    {
        public DataTypeEnum DataType { get; set; }

        public List<int> Ids { get; set; }

        public string TargetField { get; set; }

        public string SourceField { get; set; }

        public object Value { get; set; }

        public int? TargetUdfId { get; set; }

        public int? SourceUdfId { get; set; }

        public bool NeedConversion { get; set; }

        public bool IsMultiply { get; set; }

        public decimal ConversionParameter { get; set; }
    }
}

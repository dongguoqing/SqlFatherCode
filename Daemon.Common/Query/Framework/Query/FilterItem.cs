namespace Daemon.Common.Query.Framework
{
    /// <summary>
    /// A filter item that consist of one filtering condition.
    /// </summary>
    public class FilterItem
    {
        /// <summary>
        /// The FieldName of the filtering target.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The IsReduceRecordsFilterSet of the filtering target.
        /// </summary>
        public bool IsReduceRecordsFilterSet { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets the type hint.
        /// </summary>
        public string TypeHint { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the value list.
        /// </summary>
        public string ValueList { get; set; }

        /// <summary>
        /// The number (decimal, float, double) data precision.
        /// </summary>
        public int? Precision { get; set; }
    }
}

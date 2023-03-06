namespace Daemon.Common.Query.Framework
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Contains FilterItems and nested FilterSets.
    /// </summary>
    public class FilterSet
    {
        /// <summary>
        /// Set of simple conditions.
        /// </summary>
        public List<FilterItem> FilterItems { get; set; }

        /// <summary>
        /// Set of complex conditions consist of nested FilterSet.
        /// </summary>
        public List<FilterSet> FilterSets { get; set; }

        /// <summary>
        /// The flag of is reduce records filterSet applys to all child FilterItems and FilterSets of current FilterSet.
        /// </summary>
        public bool IsReduceRecordsFilterSet { get; set; }

        /// <summary>
        /// The flag of whether convert to raw where clause.
        /// </summary>
        public bool IsConvertToRawWhereClause { get; set; } = true;

        /// <summary>
        /// The Logical Operator applys to all child FilterItems and FilterSets of current FilterSet.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LogicalOperator LogicalOperator { get; set; }
    }
}

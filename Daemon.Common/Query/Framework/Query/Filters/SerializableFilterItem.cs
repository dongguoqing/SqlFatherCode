namespace Daemon.Common.Query.Framework.Filters
{
    using Daemon.Common.Query.Framework.Query;
    /// <summary>
    /// A filter item whose properties are in a serializable state.
    /// </summary>
    public class SerializableFilterItem
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the type hint.
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
        /// Gets or sets the calculated value.
        /// </summary>
        public SerializableCalculatedFilterValue CalculatedValue { get; set; }
    }
}

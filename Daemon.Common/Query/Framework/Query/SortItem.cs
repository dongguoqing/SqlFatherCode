namespace Daemon.Common.Query.Framework
{
    /// <summary>
    /// Contains members related to a sortable property of an entity.
    /// </summary>
    public class SortItem : ISortItem
    {
        /// <summary>
        /// Gets the direction.
        /// </summary>
        public SortDirection Direction { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; set; }
    }
}

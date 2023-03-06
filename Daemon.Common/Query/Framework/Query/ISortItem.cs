namespace Daemon.Common.Query.Framework
{
    public interface ISortItem
    {
        /// <summary>
        /// Gets the direction of the sort.
        /// </summary>
        SortDirection Direction { get; }

        /// <summary>
        /// Gets the name of the data field.
        /// </summary>
        string Name { get; }
    }
}

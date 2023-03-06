using System;
using System.Collections.Generic;

namespace Daemon.Common.Query.Framework.Query
{
    /// <summary>
    ///
    /// </summary>
    public enum QueryEntityState
    {
        None = 0,
        Added,
        Removed,
    }

    /// <summary>
    ///
    /// </summary>
    public class QueryState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryState"/> class.
        /// </summary>
        public QueryState()
        {
            Parents = new Dictionary<string, QueryState>();
            Children = new Dictionary<string, List<QueryState>>();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public Dictionary<string, List<QueryState>> Children { get; private set; }

        /// <summary>
        /// Gets or sets the state of the entity.
        /// </summary>
        public QueryEntityState EntityState { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the parents.
        /// </summary>
        public Dictionary<string, QueryState> Parents { get; private set; }
    }
}

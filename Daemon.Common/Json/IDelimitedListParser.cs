
using System;
using System.Collections.Generic;
using Daemon.Common.Reflection;
namespace Daemon.Common.Json
{

    /// <summary>
    /// Defines a delimited list parser in a generic and non-generic fashion.
    /// </summary>
    public interface IDelimitedListParser
    {
        /// <summary>
        /// Parses a delimited list into a concrete type.
        /// </summary>
        /// <typeparam name="TItemType">Type of the items in the list.</typeparam>
        /// <param name="delimitedList">The delimited list.</param>
        /// <returns>A List of the specified type.</returns>
        ListWithParameter<TItemType> ParseList<TItemType>(string delimitedList, Dictionary<string, object> arguments);

        /// <summary>
        /// Parses the delimited list into a concrete type (List&lt;T&gt;) using the specified runtime type.
        /// </summary>
        /// <param name="itemType">Type of the items in the list.</param>
        /// <param name="delimitedList">The delimited list.</param>
        /// <returns>
        /// A (List&lt;T&gt;) as an object.
        /// </returns>
        object ParseList(Type itemType, string delimitedList, Dictionary<string, object> arguments);
    }
}

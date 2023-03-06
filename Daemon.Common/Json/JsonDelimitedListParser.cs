using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Daemon.Common.Reflection;
namespace Daemon.Common.Json
{
    /// <summary>
	/// Handles parsing a list from JSON format to C# syntax.
	/// </summary>
	public class JsonDelimitedListParser : IDelimitedListParser
    {
        /// <summary>
        /// Parses a delimited list into a concrete type.
        /// </summary>
        /// <typeparam name="TItemType">Type of the items in the list.</typeparam>
        /// <param name="delimitedList">The delimited list.</param>
        /// <returns>A List of the specified type.</returns>
        public ListWithParameter<TItemType> ParseList<TItemType>(string delimitedList, Dictionary<string, object> arguments)
        {
            ListWithParameter<TItemType> instance = JsonConvert.DeserializeObject<ListWithParameter<TItemType>>(delimitedList);
            SetArgumentsToTheList(instance, arguments);
            return instance;
        }

        /// <summary>
        /// Parses the delimited list into a concrete type (List&lt;T&gt;) using the specified runtime type.
        /// </summary>
        /// <param name="itemType">Type of the items in the list.</param>
        /// <param name="delimitedList">The delimited list.</param>
        /// <returns>
        /// A (List&lt;T&gt;) as an object.
        /// </returns>
        public object ParseList(Type itemType, string delimitedList, Dictionary<string, object> arguments)
        {
            Type listType = typeof(ListWithParameter<>).MakeGenericType(new Type[] { itemType });

            object instance = JsonConvert.DeserializeObject(delimitedList, listType);

            SetArgumentsToTheList(instance, arguments);

            return instance;
        }

        private void SetArgumentsToTheList(object instance, Dictionary<string, object> arguments)
        {
            Type t = instance.GetType();
            if (arguments != null && arguments.Count > 0)
            {
                PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo info in props)
                {
                    if (info.CanWrite && arguments.ContainsKey(info.Name))
                    {
                        info.SetValue(instance, arguments[info.Name]);
                    }
                }
            }
        }
    }
}

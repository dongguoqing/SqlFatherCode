using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Daemon.Common.Reflection
{
    public static class PropertyParser
    {
        /// <summary>
        /// Finds a property on the specified type by name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string FindProperty<T>(string name)
        {
            return FindProperty(typeof(T), name);
        }

        /// <summary>
        /// Finds a property on the specified type by name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string FindProperty(Type type, string name)
        {
            PropertyInfo propertyInfo = type.GetProperties().FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("The class type '" + type.Name + "' does not contain a property named '" + name + "'.");
            }

            return propertyInfo.Name;
        }
    }
}

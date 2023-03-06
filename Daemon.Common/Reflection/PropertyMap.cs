namespace Daemon.Common.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    /// <summary>
    ///
    /// </summary>
    public class PropertyMap
    {
        private readonly Type _entityType;
        private readonly Dictionary<string, List<PropertyInfo>> _propertyDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMap"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public PropertyMap(Type entityType)
        {
            _propertyDictionary = new Dictionary<string, List<PropertyInfo>>();
            _entityType = entityType;
        }

        /// <summary>
        /// Finds the property.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<PropertyInfo> FindNestedProperty(string propertyName)
        {
            if (!_propertyDictionary.ContainsKey(propertyName))
            {
                List<PropertyInfo> propertyInformationList = new List<PropertyInfo>();

                Type type = _entityType;

                PropertyInfo property = type.GetProperty(propertyName);
                if (property != null)
                {
                    propertyInformationList.Add(property);
                }
                else
                {
                    throw new InvalidOperationException("The property '" + propertyName + "' is not found on the '" + type.Name + "' object.");
                }

                type = property.PropertyType;

                _propertyDictionary[propertyName] = propertyInformationList;
            }

            if (_propertyDictionary[propertyName] != null)
            {
                return _propertyDictionary[propertyName].AsReadOnly();
            }

            return null;
        }
    }
}

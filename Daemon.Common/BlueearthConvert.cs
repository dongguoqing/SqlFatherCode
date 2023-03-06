using System;
using Daemon.Common.Reflection;
namespace Daemon.Common
{
    public static class BlueearthConvert
    {
        /// <summary>
        /// Changes the to the specified type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="conversionType">Type of the conversion.</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversionType)
        {
            if (conversionType.IsNullable())
            {
                if (value != null)
                {
                    conversionType = Nullable.GetUnderlyingType(conversionType);
                }
                else
                {
                    return null;
                }
            }

            return Convert.ChangeType(value, conversionType);
        }
    }
}

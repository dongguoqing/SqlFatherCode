namespace Daemon.Common.Reflection
{
    using System;
    public static class TypeExtensions
    {
        /// <summary>
        /// Finds the underlying type if nullable; otherwise returns the specified type.
        /// </summary>
        /// <param name="type">The type definition.</param>
        /// <returns>The underlying type.</returns>
        public static Type FindUnderlyingTypeIfNullable(this Type type)
        {
            Type returnValue = type;
            if (type.IsNullable())
            {
                returnValue = Nullable.GetUnderlyingType(type);
            }

            return returnValue;
        }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type definition.</param>
        /// <returns>Value of true if nullable; otherwise false.</returns>
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}

namespace Daemon.Data.Substructure.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public static class ExtensionMethodHelper
	{
		public static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType)
		{
			var query = from type in assembly.GetTypes()
						where type.IsSealed && !type.IsGenericType && !type.IsNested
						from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						where method.IsDefined(typeof(ExtensionAttribute), false)
						where method.GetParameters()[0].ParameterType == extendedType
						select method;
			return query;
		}

		public static MethodInfo GetExtensionMethods(Assembly assembly, Type extendedType, string name)
		{
			return GetExtensionMethods(assembly, extendedType).FirstOrDefault(r => r.Name == name);
		}
	}
}

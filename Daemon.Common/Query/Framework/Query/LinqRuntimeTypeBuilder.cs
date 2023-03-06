using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Daemon.Common.Query.Framework.Query
{
    public static class LinqRuntimeTypeBuilder
    {
        private static AssemblyName _assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        private static ModuleBuilder _moduleBuilder = null;
        private static Dictionary<string, Type> _builtTypes = new Dictionary<string, Type>();

        static LinqRuntimeTypeBuilder()
        {
            _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(_assemblyName.Name);
        }

        public static Type GetDynamicType(Dictionary<string, Type> fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException("fields");
            }

            if (fields.Count == 0)
            {
                throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");
            }

            try
            {
                Monitor.Enter(_builtTypes);
                string className = GetTypeKey(fields);

                if (_builtTypes.ContainsKey(className))
                {
                    return _builtTypes[className];
                }

                TypeBuilder typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

                foreach (var field in fields)
                {
                    typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);
                }

                _builtTypes[className] = typeBuilder.CreateTypeInfo();

                return _builtTypes[className];
            }
            catch (Exception)
            {
            }
            finally
            {
                Monitor.Exit(_builtTypes);
            }

            return null;
        }

        public static Type GetDynamicType(IEnumerable<PropertyInfo> fields)
        {
            return GetDynamicType(fields.ToDictionary(f => f.Name, f => f.PropertyType));
        }

        public static Type GetDynamicType(Dictionary<string, PropertyInfo> fields)
        {
            return GetDynamicType(fields.ToDictionary(f => f.Key, f => f.Value.PropertyType));
        }

        private static string GetTypeKey(Dictionary<string, Type> fields)
        {
            // TODO: optimize the type caching -- if fields are simply reordered, that doesn't mean that they're actually different types, so this needs to be smarter
            string key = string.Empty;
            foreach (var field in fields)
            {
                key += field.Key + ";" + field.Value.Name + ";";
            }

            // Use MD5 value to solve issue: Type name was too long. The fully qualified type name must be less than 1,024 characters.
            return GetMD5Hash(key);
        }

        private static string GetTypeKey(IEnumerable<PropertyInfo> fields)
        {
            return GetTypeKey(fields.ToDictionary(f => f.Name, f => f.PropertyType));
        }

        private static string GetMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}

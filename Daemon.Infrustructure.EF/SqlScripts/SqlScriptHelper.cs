namespace Daemon.Infrustructure.EF.SqlScripts
{
    using System.Text;
    /// <summary>
    /// The sql script helper class.
    /// </summary>
    public static class SqlScriptHelper
    {
        /// <summary>
        /// Convert to string, if original data is null, returns "null".
        /// </summary>
        /// <typeparam name="T">The generics type.</typeparam>
        /// <param name="generics">The original data.</param>
        /// <returns>Returns converted data.</returns>
        public static string GenericsToString<T>(T generics)
        {
            if (generics == null)
            {
                return "null";
            }

            if (generics is byte[])
            {
                var genericeByte = generics as byte[];
                StringBuilder gStrBuilder = new StringBuilder((genericeByte.Length * 2) + 2);
                gStrBuilder.Append("0x");
                int pos = 0;
                while (pos < genericeByte.Length - 1000000)
                {
                    gStrBuilder.Append(System.BitConverter.ToString(genericeByte, pos, 1000000)).Replace("-", string.Empty);
                    pos += 1000000;
                }

                gStrBuilder.Append(System.BitConverter.ToString(genericeByte, pos)).Replace("-", string.Empty);
                return gStrBuilder.ToString();
            }

            if (generics is string)
            {
                var strGenerics = (generics as string).Replace("'", "''");
                return "'" + strGenerics + "'";
            }

            return "'" + generics.ToString() + "'";
        }
    }
}

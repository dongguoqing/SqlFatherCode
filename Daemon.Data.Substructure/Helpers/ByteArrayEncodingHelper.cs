namespace Daemon.Data.Substructure.Helpers
{
    using System;
    public static class ByteArrayEncodingHelper
    {
        public static byte[] Decode(string value)
        {
            return Convert.FromBase64String(value);
        }

        public static string Encode(byte[] value)
        {
            return Convert.ToBase64String(value);
        }
    }
}

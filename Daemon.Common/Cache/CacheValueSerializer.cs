namespace Daemon.Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using Newtonsoft.Json;
    public static class CacheValueSerializer
    {
        private static JsonSerializerSettings SerializerOptions { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static T Deserialize<T>(byte[] cachedBytes)
        {
            if (cachedBytes == null)
            {
                return default(T);
            }

            var cachedString = Encoding.UTF8.GetString(cachedBytes, 0, cachedBytes.Length);
            if (String.IsNullOrEmpty(cachedString))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(cachedString, SerializerOptions);
        }

        public static byte[] Serialize<T>(T value)
        {
            var cacheString = JsonConvert.SerializeObject(value, SerializerOptions);
            return Encoding.UTF8.GetBytes(cacheString);
        }

        public static double? ToSeconds(DateTime? dateTime) => dateTime.HasValue ? ToSeconds(dateTime.Value) : default(double?);

        public static double ToSeconds(DateTime dateTime) => (dateTime - DateTime.MinValue).TotalSeconds;

        public static DateTime? FromSeconds(double? seconds) => seconds.HasValue ? FromSeconds(seconds.Value) : default(DateTime?);

        public static DateTime FromSeconds(double seconds) => DateTime.MinValue.AddSeconds(seconds);
    }
}

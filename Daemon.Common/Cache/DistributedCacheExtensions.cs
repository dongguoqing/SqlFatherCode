namespace Daemon.Common.Cache
{
    using System.Text;
    using Newtonsoft.Json;
    using Microsoft.Extensions.Caching.Distributed;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    /// <summary>
    /// Distributed Cache Extensions
    /// </summary>
    public static class DistributedCacheExtensions
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Asynchronously sets cache entry value with given key using default serialization of specified type.
        /// </summary>
        /// <param name="distributedCache">The cache in which to store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="value">Instance of type <typeparamref name="T"/> to be serialized and set as entry value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <typeparam name="T">The type of the object to be serialized and set as the entry value.</typeparam>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value,
            CancellationToken token = default)
        {
            await distributedCache.SetAsync(key, value, new DistributedCacheEntryOptions(), default).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously sets cache entry value with given key using default serialization of specified type.
        /// </summary>
        /// <param name="distributedCache">The cache in which to store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="value">Instance of type <typeparamref name="T"/> to be serialized and set as entry value.</param>
        /// <param name="options">The cache options for the entry.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <typeparam name="T">The type of the object to be serialized and set as the entry value.</typeparam>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value,
            DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var cacheString = JsonConvert.SerializeObject(value, _settings);
            var cacheBytes = Encoding.UTF8.GetBytes(cacheString);
            await distributedCache.SetAsync(key, cacheBytes, options, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets cache entry value with given key using default serialization of specified type.
        /// </summary>
        /// <param name="distributedCache">The cache in which to store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="value">Instance of type <typeparamref name="T"/> to be serialized and set as entry value.</param>
        /// <typeparam name="T">The type of the object to be serialized and set as the entry value.</typeparam>
        public static void Set<T>(this IDistributedCache distributedCache, string key, T value)
        {
            distributedCache.Set(key, value, new DistributedCacheEntryOptions());
        }

        /// <summary>
        /// Sets cache entry value with given key using default serialization of specified type.
        /// </summary>
        /// <param name="distributedCache">The cache in which to store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="value">Instance of type <typeparamref name="T"/> to be serialized and set as entry value.</param>
        /// <param name="options">The cache options for the entry.</param>
        /// <typeparam name="T">The type of the object to be serialized and set as the entry value.</typeparam>
        public static void Set<T>(this IDistributedCache distributedCache, string key, T value,
            DistributedCacheEntryOptions options)
        {
            var cacheString = JsonConvert.SerializeObject(value, _settings);
            var cacheBytes = Encoding.UTF8.GetBytes(cacheString);
            distributedCache.Set(key, cacheBytes, options);
        }

        /// <summary>
        /// Sets cache entry value with given key using default serialization of specified type.
        /// </summary>
        /// <param name="distributedCache">The cache in which to store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="value">Instance of type <typeparamref name="T"/> to be serialized and set as entry value.</param>
        /// <param name="expiredAfter">Specify timespan when to get expired.</param>
        /// <typeparam name="T">The type of the object to be serialized and set as the entry value.</typeparam>
        public static void Set<T>(this IDistributedCache distributedCache, string key, T value, TimeSpan expiredAfter)
        {
            distributedCache.Set(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiredAfter
            });
        }

        /// <summary>
        /// Gets cache entry value with key <paramref name="key"/> and deserializes to instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="distributedCache">The cache from which to retrieve the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <typeparam name="T">The type to deserialize the cache entry value to.</typeparam>
        /// <returns>The <see cref="Task"/> that gets the deserialized <typeparamref name="T"/> instance or <see langword="null"/> if the entry was not found or could not be deserialized.</returns>
        public static async Task<T> GetAsync<T>(this IDistributedCache distributedCache, string key,
            CancellationToken token = default)
        {
            var cachedBytes = await distributedCache.GetAsync(key, token).ConfigureAwait(false);
            if (cachedBytes == null)
            {
                return default(T);
            }

            var cachedString = Encoding.UTF8.GetString(cachedBytes, 0, cachedBytes.Length);
            if (String.IsNullOrEmpty(cachedString))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(cachedString, _settings);
        }

        /// <summary>
        /// Gets cache entry value with key <paramref name="key"/> and deserializes to instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="distributedCache">The cache from which to retrieve the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <typeparam name="T">The type to deserialize the cache entry value to.</typeparam>
        /// <returns>The deserialized <typeparamref name="T"/> instance or <see langword="null"/> if the entry was not found or could not be deserialized.</returns>
        public static T Get<T>(this IDistributedCache distributedCache, string key)
        {
            var cachedBytes = distributedCache.Get(key);
            if (cachedBytes == null)
            {
                return default(T);
            }

            var cachedString = Encoding.UTF8.GetString(cachedBytes, 0, cachedBytes.Length);
            if (String.IsNullOrEmpty(cachedString))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(cachedString, _settings);
        }

        /// <summary>
        /// Attempts to get value of cache entry with specified key. If the requested cache entry does not exist,
        /// a new instance of <typeparamref name="T"/> is created using provided factory method and a new cache entry
        /// is created with the specified key and value of this instance.
        /// </summary>
        /// <param name="distributedCache">The cache from which to retrieve and store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="factory">A factory method that produces the instance to provide cache entry value.</param>
        /// <typeparam name="T">The type to deserialize the cache entry value to.</typeparam>
        /// <returns>
        /// The deserialized <typeparamref name="T"/> instance of existing cache entry value or newly created instance
        /// produced by the provided factory method.
        /// </returns>
        public static T GetOrCreate<T>(this IDistributedCache distributedCache, string key, Func<T> factory)
        {
            var value = distributedCache.Get<T>(key);
            if (value != null)
            {
                return value;
            }

            value = factory();
            distributedCache.Set<T>(key, value);
            return value;
        }

        /// <summary>
        /// Attempts to asynchronously get value of cache entry with specified key. If the requested cache entry does
        /// not exist, a new instance of <typeparamref name="T"/> is asynchronously created using provided factory
        /// method and a new cache entry is asynchronously created with the specified key and value of this instance.
        /// </summary>
        /// <param name="distributedCache">The cache from which to retrieve and store the data.</param>
        /// <param name="key">A string identifying requested cache entry.</param>
        /// <param name="factory">A factory method that produces the instance to provide cache entry value.</param>
        /// <typeparam name="T">The type to deserialize the cache entry value to.</typeparam>
        /// <returns>
        /// The deserialized <typeparamref name="T"/> instance of existing cache entry value or newly created instance
        /// produced by the provided factory method.
        /// </returns>
        public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache distributedCache, string key, Func<Task<T>> factory)
        {
            var value = await distributedCache.GetAsync<T>(key).ConfigureAwait(false);
            if (value != null)
            {
                return value;
            }

            value = await factory().ConfigureAwait(false);
            await distributedCache.SetAsync(key, value).ConfigureAwait(false);
            return value;
        }
    }
}

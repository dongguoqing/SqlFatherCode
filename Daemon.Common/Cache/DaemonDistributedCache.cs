namespace Daemon.Common.Cache
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using CSRedis;
    using System.Linq;
    public class DaemonDistributedCache : IDaemonDistributedCache
    {
        private readonly IDistributedCache _cache;
        private IRedisClient _redisConnection;
        private readonly IConfiguration _configuration;
        private RedisConnectionConfig _redisConnectionConfig;
        // Distinct locks per cache key to provide thread safety for hashes in local memory cache mode.
        // Since MemoryDistributedCache ctor internally creates a new MemoryCache (not Default), this is not static.
        private readonly ConcurrentDictionary<string, object> _localHashLocks = new ConcurrentDictionary<string, object>();

        public DaemonDistributedCache(IConfiguration configuration)
        {
            _configuration = configuration;
            _redisConnectionConfig = configuration.GetSection("RedisConfig").Get<RedisConnectionConfig>();

            if (string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            }
            else
            {
                _cache = new DaemonRedisCache(_redisConnectionConfig);
            }
        }

        public byte[] Get(string key)
        {
            return _cache.Get(key);
        }

        public bool Exists(string key)
        {
            var item = _cache.Get(key);
            return item != null;
        }

        /// <inheritdoc cref="IDistributedCache.GetAsync"/>
		public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return await _cache.GetAsync(key, token);
        }

        /// <inheritdoc cref="IDistributedCache.Set"/>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _cache.Set(key, value, options);
        }

        /// <inheritdoc cref="IDistributedCache.SetAsync"/>
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return _cache.SetAsync(key, value, options, token);
        }

        /// <inheritdoc cref="IDistributedCache.Refresh"/>
        public void Refresh(string key)
        {
            _cache.Refresh(key);
        }

        /// <inheritdoc cref="IDistributedCache.RefreshAsync"/>
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return _cache.RefreshAsync(key, token);
        }

        /// <inheritdoc cref="IDistributedCache.Remove"/>
        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        /// <inheritdoc cref="IDistributedCache.RemoveAsync"/>
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _cache.RemoveAsync(key, token);
        }

        public bool HashExists<T>(string key, string field)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                var value = RedisHelper.HGet(key, field);
                return string.IsNullOrEmpty(value);
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var memoryCacheEntries = _cache.Get<IEnumerable<(string Key, T Value)>>(key);
                if (memoryCacheEntries == null)
                {
                    return false;
                }

                return memoryCacheEntries.Any(e => e.Key == field);
            }
        }

        public Dictionary<string, T> HashGetAll<T>(string key)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                return RedisHelper.HGetAll<T>(key);
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var memoryCacheEntries = _cache.Get<Dictionary<string, T>>(key);
                if (memoryCacheEntries == null)
                {
                    return new Dictionary<string, T>();
                }

                return memoryCacheEntries;
            }
        }

        public List<T> HashGet<T>(string key, List<string> fields)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                var fieldsArray = fields.Select(f => f).ToArray();
                return RedisHelper.HMGet<T>(key, fieldsArray).ToList();
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var memoryCacheEntries = _cache.Get<Dictionary<string, T>>(key);
                if (memoryCacheEntries == null)
                {
                    return new List<T>(fields.Count);
                }

                return fields.Select(f => memoryCacheEntries.TryGetValue(f, out T value) ? value : default(T)).ToList();
            }
        }

        public T HashGet<T>(string key, string field)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                return RedisHelper.HGet<T>(key, field);
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var memoryCacheEntries = _cache.Get<Dictionary<string, T>>(key);
                if (memoryCacheEntries == null)
                {
                    return default(T);
                }

                return memoryCacheEntries.FirstOrDefault(e => e.Key == field).Value;
            }
        }

        public void HashSet<T>(string key, Dictionary<string, T> entries)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                foreach (var field in entries.Keys)
                {
                    RedisHelper.HSet(key, field, entries[field]);
                }
                return;
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var currentEntries = _cache.Get<Dictionary<string, T>>(key);
                if (currentEntries == null)
                {
                    _cache.Set(key, entries);
                    return;
                }

                var newEntries = entries.Union(currentEntries, new HashEntryKeyComparer<T>());
                var newDictionary = newEntries.ToDictionary(e => e.Key, e => e.Value);
                _cache.Set(key, newDictionary);
            }
        }

        public void HashSet<T>(string key, string field, T value)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                RedisHelper.HSet(key, field, value);
                return;
            }

            var entries = new Dictionary<string, T> { { field, value } };

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var currentEntries = _cache.Get<Dictionary<string, T>>(key);
                if (currentEntries == null)
                {
                    _cache.Set(key, entries);
                    return;
                }

                var newEntries = entries.Union(currentEntries, new HashEntryKeyComparer<T>());
                var newDictionary = newEntries.ToDictionary(e => e.Key, e => e.Value);
                _cache.Set(key, newDictionary);
            }
        }

        public long HashDelete<T>(string key, IEnumerable<string> fields)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                return RedisHelper.HDel(key, fields.ToArray());
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var currentEntries = _cache.Get<Dictionary<string, T>>(key);
                if (currentEntries != null)
                {
                    var existingEntries = currentEntries.ToList();
                    var newEntries = existingEntries.Where(e => !fields.Contains(e.Key)).ToList();
                    var newDictionary = newEntries.ToDictionary(e => e.Key, e => e.Value);
                    _cache.Set(key, newDictionary);
                    return existingEntries.Count - newEntries.Count;
                }

                return 0;
            }
        }

        public bool HashDelete<T>(string key, string field)
        {
            if (!string.IsNullOrEmpty(_redisConnectionConfig?.ConnectionStr))
            {
                return RedisHelper.HDel(key, new string[] { field }) == 1;
            }

            lock (_localHashLocks.GetOrAdd(key, k => new object()))
            {
                var currentEntries = _cache.Get<Dictionary<string, T>>(key);
                if (currentEntries != null)
                {
                    var existingEntries = currentEntries.ToList();
                    var newEntries = existingEntries.Where(e => e.Key != field).ToList();
                    var newDictionary = newEntries.ToDictionary(e => e.Key, e => e.Value);

                    _cache.Set(key, newDictionary);
                    return newEntries.Count != existingEntries.Count;
                }

                return false;
            }
        }

        private sealed class HashEntryKeyComparer<T> : IEqualityComparer<KeyValuePair<string, T>>
        {
            public bool Equals(KeyValuePair<string, T> x, KeyValuePair<string, T> y)
            {
                return x.Key == y.Key;
            }

            public int GetHashCode(KeyValuePair<string, T> obj)
            {
                return obj.Key.GetHashCode();
            }
        }

        private T Deserialize<T>(byte[] cachedBytes)
        {
            return CacheValueSerializer.Deserialize<T>(cachedBytes);
        }

        private byte[] Serialize<T>(T value)
        {
            return CacheValueSerializer.Serialize<T>(value);
        }
    }
}

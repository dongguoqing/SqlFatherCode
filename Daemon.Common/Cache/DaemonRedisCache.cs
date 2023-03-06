namespace Daemon.Common.Cache
{
    using CSRedis;
    using Microsoft.Extensions.Caching.Distributed;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Daemon.Common.Middleware;
    using Microsoft.Extensions.Configuration;
    public class DaemonRedisCache : IDistributedCache
    {
        private static IDistributedCache Instance => ServiceLocator.Resolve<IDistributedCache>();
        private static readonly Task CompletedTask = Task.FromResult<object>(null);
        private static CSRedisClient _instance;
        public DaemonRedisCache(RedisConnectionConfig redisConnectionConfig)
        {
            if (_instance == null)
            {
                if (redisConnectionConfig.IsSentinel)
                {
                    try
                    {
                        _instance = new CSRedisClient(redisConnectionConfig.ConnectionStr, redisConnectionConfig.Sentinel.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    _instance = new CSRedisClient(redisConnectionConfig.ConnectionStr);
                }
            }

            RedisHelper.Initialization(_instance);
        }

        public byte[] Get(string key)
        {
            if (RedisHelper.Exists(key))
            {
                return RedisHelper.Get<byte[]>(key);
            }

            return null;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            // if (RedisHelper.Exists(key))
            // {
            //     var content = RedisHelper.GetAsync<byte[]>(key);
            //     return content;
            // }

            return await RedisHelper.GetAsync<byte[]>(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RedisHelper.Set(key, value);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Set(key, value, options);
            return CompletedTask;
        }

        public void Remove(string key) => Instance.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => Instance.RemoveAsync(key, token);

        public void Refresh(string key) => Instance.Refresh(key);

        public Task RefreshAsync(string key, CancellationToken token = default) => Instance.RefreshAsync(key, token);

        private static void ValidateCacheKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
        }
    }
}

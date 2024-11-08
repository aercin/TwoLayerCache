using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace TwoLayerCache.Services.RedisCache
{
    public class RedisCacheProvider : IRedisCacheProvider
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _connMultiplexer;
        private readonly IDatabase _database;

        public RedisCacheProvider(IDistributedCache distributedCache, IConnectionMultiplexer connMultiplexer)
        {
            this._distributedCache = distributedCache;
            this._connMultiplexer = connMultiplexer;
            this._database = this._connMultiplexer.GetDatabase();
        }

        public T Get<T>(string key)
        {
            var cacheItem = this._distributedCache.GetString(key);

            if (cacheItem != null)
            {
                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(cacheItem, typeof(T));
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(cacheItem);
                }
            }
            return default(T);
        }

        public void Set<T>(string key, T item, Action<CacheSettings> config)
        {
            string itemStringRepresentation;

            if (typeof(T).IsValueType)
            {
                itemStringRepresentation = item.ToString();
            }
            else
            {
                itemStringRepresentation = JsonSerializer.Serialize(item);
            }

            var cacheSettings = new CacheSettings();

            config(cacheSettings);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(cacheSettings.Expiry)
            };

            this._distributedCache.SetString(key, itemStringRepresentation, options);
        }

        public int RemainingExpiry(string key)
        {
            int remainingExpiry = 0;
            var keyTTL = this._database.KeyTimeToLive(key);
            if (keyTTL != null)
            {
                remainingExpiry = (int)keyTTL.Value.TotalMilliseconds;
            }
            return remainingExpiry;
        }

        public ISubscriber GetSubscriber()
        {
            return this._connMultiplexer.GetSubscriber();
        }
    }
}

using StackExchange.Redis;

namespace TwoLayerCache.Services.RedisCache
{
    public interface IRedisCacheProvider
    {
        T Get<T>(string key);

        void Set<T>(string key, T item, Action<CacheSettings> config);

        int RemainingExpiry(string key);

        ISubscriber GetSubscriber();
    }
}

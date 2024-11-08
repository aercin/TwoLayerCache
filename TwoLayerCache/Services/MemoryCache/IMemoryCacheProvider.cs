namespace TwoLayerCache.Services.MemoryCache
{
    public interface IMemoryCacheProvider
    {
        void Set<T>(string key, T value, Action<CacheSettings> options);

        T Get<T>(string key);
    }
}

using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;

namespace TwoLayerCache.Services.MemoryCache
{
    public class MemoryCacheProvider : IMemoryCacheProvider
    {
        private readonly IMemoryCache _memoryCache;
        public MemoryCacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set<T>(string key, T value, Action<CacheSettings> options)
        {
            string itemStringRepresentation;

            if (typeof(T).IsValueType)
            {
                itemStringRepresentation = value.ToString();
            }
            else
            {
                itemStringRepresentation = JsonSerializer.Serialize(value);
            }

            int byteSizeUtf8 = Encoding.UTF8.GetByteCount(itemStringRepresentation);

            var cacheOptions = new CacheSettings();
            options(cacheOptions);

            this._memoryCache.Set(key, itemStringRepresentation, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(cacheOptions.Expiry),
                Size = byteSizeUtf8
            });
        }

        public T Get<T>(string key)
        {
            if (this._memoryCache.TryGetValue(key, out string value))
            {
                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(value);
                }
            }
            return default(T);
        }  
    }
}

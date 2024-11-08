using System.Text.Json;
using TwoLayerCache.Models;
using TwoLayerCache.Services.MemoryCache;
using TwoLayerCache.Services.RedisCache;

namespace TwoLayerCache.Services.TwoLayerCache
{
    public class TwoLayerCacheService : ITwoLayerCache
    {
        private readonly IMemoryCacheProvider _memoryCacheProvider;
        private readonly IRedisCacheProvider _redisCacheProvider;
        private readonly IConfiguration _configuration;

        public TwoLayerCacheService(IRedisCacheProvider redisCacheProvider,
                             IMemoryCacheProvider memoryCacheProvider,
                             IConfiguration config)
        {
            this._memoryCacheProvider = memoryCacheProvider;
            this._configuration = config;
            this._redisCacheProvider = redisCacheProvider;
        }

        public string QueryResource(string path)
        {
            var responsePayload = this._memoryCacheProvider.Get<string>(path);
            if (!string.IsNullOrEmpty(responsePayload))
            {
                return responsePayload;
            }

            responsePayload = this._redisCacheProvider.Get<string>(path);
            if (!string.IsNullOrEmpty(responsePayload))
            {
                #region in-memory cache synchronization  
                if (this._redisCacheProvider.RemainingExpiry(path) > this._configuration.GetValue<int>("Caching:InMemory:SyncThreshold"))
                {
                    this._memoryCacheProvider.Set<string>(path, responsePayload, x =>
                    {
                        x.Expiry = this._configuration.GetValue<int>("Caching:InMemory:Expiry");
                    });
                }
                #endregion

                return responsePayload;
            }

            responsePayload = Guid.NewGuid().ToString();//Datastore'a gitmiş gibi farzet.

            this._redisCacheProvider.Set<string>(path, responsePayload, x =>
            {
                x.Expiry = this._configuration.GetValue<int>("Caching:Redis:Expiry");
            });

            #region Data Store'dan alınan bilgi inmemory cache'e senkronize edilmek üzere pub/sub feature kullanılmaktadır
            this._redisCacheProvider.GetSubscriber().PublishAsync(this._configuration.GetValue<string>("Caching:Redis:PubSubChannel"), JsonSerializer.Serialize(new SyncronizationMessage
            {
                Key = path,
                Value = responsePayload
            }), StackExchange.Redis.CommandFlags.FireAndForget);
            #endregion 

            return responsePayload;
        }
    }
}

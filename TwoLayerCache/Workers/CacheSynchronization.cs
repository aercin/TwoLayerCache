
using System.Text.Json;
using TwoLayerCache.Models;
using TwoLayerCache.Services.MemoryCache;
using TwoLayerCache.Services.RedisCache;

namespace TwoLayerCache.Workers
{
    public class CacheSynchronization : BackgroundService
    {
        private readonly IRedisCacheProvider _redisCacheProvider;
        private readonly IMemoryCacheProvider _memoryCacheProvider;
        private readonly IConfiguration _configuration;
        public CacheSynchronization(IRedisCacheProvider redisCacheProvider, IMemoryCacheProvider memoryCacheProvider, IConfiguration configuration)
        {
            this._redisCacheProvider = redisCacheProvider;
            this._memoryCacheProvider = memoryCacheProvider;
            this._configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this._redisCacheProvider.GetSubscriber().SubscribeAsync(this._configuration.GetValue<string>("Caching:Redis:PubSubChannel"), (channel, message) =>
            {
                Console.WriteLine($"Message:{message}");

                var syncMsg = JsonSerializer.Deserialize<SyncronizationMessage>(message);
                this._memoryCacheProvider.Set<string>(syncMsg.Key, syncMsg.Value, x =>
                {
                    x.Expiry = this._configuration.GetValue<int>("Caching:InMemory:Expiry");
                });
            });
        }
    }
}

using StackExchange.Redis;
using TwoLayerCache.Services.MemoryCache;
using TwoLayerCache.Services.RedisCache;
using TwoLayerCache.Services.TwoLayerCache;
using TwoLayerCache.Workers;

namespace TwoLayerCache.Services
{
    public static class ServiceRegistration
    {
        public static void AddDependencies(this IServiceCollection services, IConfiguration config)
        {
            #region In-Memory Cache Dependencies 
            services.AddMemoryCache(x =>
            {
                x.SizeLimit = config.GetValue<int>("Caching:InMemory:SizeLimit");
            });
            services.AddSingleton<IMemoryCacheProvider, MemoryCacheProvider>();
            #endregion

            #region Redis Dependencies
            var redisConfigOption = ConfigurationOptions.Parse($"{config.GetValue<string>("Caching:Redis:Host")}:{config.GetValue<string>("Caching:Redis:Port")}");
            redisConfigOption.Password = config.GetValue<string>("Caching:Redis:Password");
            redisConfigOption.DefaultDatabase = config.GetValue<int>("Caching:Redis:Database");

            services.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(redisConfigOption));

            services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>());
            });

            services.AddSingleton<IRedisCacheProvider, RedisCacheProvider>();
            #endregion

            services.AddSingleton<ITwoLayerCache, TwoLayerCacheService>();
            services.AddHostedService<CacheSynchronization>();
        }
    }
}

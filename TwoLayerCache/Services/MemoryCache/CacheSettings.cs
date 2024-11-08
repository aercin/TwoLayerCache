namespace TwoLayerCache.Services.MemoryCache
{
    public class CacheSettings
    {
        public int Expiry { get; set; } //milisecond
        public int SizeLimit { get; set; }
    }
}

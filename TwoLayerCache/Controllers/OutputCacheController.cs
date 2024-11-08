using Microsoft.AspNetCore.Mvc;
using TwoLayerCache.Services.TwoLayerCache;

namespace TwoLayerCache.Controllers
{
    [Route("api")]
    [ApiController]
    public class OutputCacheController : ControllerBase
    {
        private readonly ITwoLayerCache _twoLayerCacheSvc;

        public OutputCacheController(ITwoLayerCache twoLayerCacheSvc)
        {
            this._twoLayerCacheSvc = twoLayerCacheSvc;
        }

        [Route("/v1/orders/{id}")]
        [HttpGet]
        public IActionResult GetOrders(int id)
        {
            return Ok(this._twoLayerCacheSvc.QueryResource($"{Request.Path}{Request.QueryString}"));
        }
    }
}

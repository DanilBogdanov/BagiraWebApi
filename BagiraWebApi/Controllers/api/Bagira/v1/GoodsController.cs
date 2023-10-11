using BagiraWebApi.Services.Bagira;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("api/bagira/v1/[controller]")]
    [ApiController]
    public class GoodsController : Controller
    {
        private readonly BagiraService _bagiraService;

        public GoodsController(BagiraService bagiraService)
        {
            _bagiraService = bagiraService;
        }

        [HttpGet("{goodId}")]
        public async Task<IActionResult> GetGoodAsync(int goodId)
        {
            var goodDto = await _bagiraService.Goods.GetGoodAsync(goodId);
            if (goodDto == null)
            {
                return NotFound();
            }
            return Ok(goodDto);
        }
    }
}

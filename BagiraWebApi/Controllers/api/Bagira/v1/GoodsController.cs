using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Bagira.DataModels;
using BagiraWebApi.Services.Exchanges;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("api/bagira/v1/[controller]")]
    [ApiController]
    public class GoodsController : Controller
    {
        private readonly GoodService _goodService;
        private Exchange1C _exchange;

        public GoodsController(GoodService bagiraService, Exchange1C exchange1C)
        {
            _goodService = bagiraService;
            _exchange = exchange1C;
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("{goodId}")]
        public async Task<IActionResult> GetGoodAsync(int goodId)
        {
            try
            {
                var goodDto = await _goodService.GetGoodAsync(goodId);
                if (goodDto == null)
                {
                    return NotFound();
                }
                return Ok(goodDto);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("cats")]
        public async Task<IActionResult> GetCatGoodsAsync(
            int? groupId,
            int? take,
            int? skip)
        {
            var queryProps = new BagiraQueryProps
            {
                GroupId = groupId,
                Take = take,
                Skip = skip
            };

            var catGoodsDto = await _goodService.GetCatGoodsAsync(queryProps);

            return Ok(catGoodsDto);
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("dogs")]
        public async Task<IActionResult> GetDogGoodsOfGroupAsync(
            int? groupId,
            int? take,
            int? skip)
        {
            var queryProps = new BagiraQueryProps
            {
                GroupId = groupId,
                Take = take,
                Skip = skip
            };
            var dogGoodsDto = await _goodService.GetDogGoodsAsync(queryProps);

            return Ok(dogGoodsDto);
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("others")]
        public async Task<IActionResult> GetOtherGoodsOfGroupAsync(
            int? groupId,
            int? take,
            int? skip)
        {
            var queryProps = new BagiraQueryProps
            {
                GroupId = groupId,
                Take = take,
                Skip = skip
            };
            var otherGoodsDto = await _goodService.GetOtherGoodsAsync(queryProps);

            return Ok(otherGoodsDto);
        }
    }
}

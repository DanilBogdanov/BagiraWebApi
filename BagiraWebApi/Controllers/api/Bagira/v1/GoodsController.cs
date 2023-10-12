using BagiraWebApi.Services.Bagira;
using BagiraWebApi.Services.Bagira.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("api/bagira/v1/[controller]")]
    [ApiController]
    public class GoodsController : Controller
    {
        private readonly GoodService _goodService;

        public GoodsController(GoodService bagiraService)
        {
            _goodService = bagiraService;
        }

        [HttpGet("{goodId}")]
        public async Task<IActionResult> GetGoodAsync(int goodId)
        {
            var goodDto = await _goodService.GetGoodAsync(goodId);
            if (goodDto == null)
            {
                return NotFound();
            }
            return Ok(goodDto);
        }

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

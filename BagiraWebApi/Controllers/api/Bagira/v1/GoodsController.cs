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

        public GoodsController(GoodService bagiraService, Exchange1C exchange1C)
        {
            _goodService = bagiraService;
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(
            string query,
            int? take,
            int? skip)
        {
            try
            {
                var queryProps = new BagiraQueryProps
                {
                    Query = query,
                    Take = take,
                    Skip = skip
                };
                var goodsDto = await _goodService.SearchAsync(queryProps);

                return Ok(goodsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("groups")]
        public async Task<IActionResult> GetGoodGroupsAsync()
        {
            try
            {
                var goodGroupsDto = await _goodService.GetGoodGroupsAsync();

                return Ok(goodGroupsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
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
                return StatusCode(500, ex.ToString());
            }
        }

        //[OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("list")]
        public async Task<IActionResult> GetGoodsListAsync(string ids)
        {
            List<int> idsList;
            try
            {
                idsList = ids.Split(",").Select(id => int.Parse(id)).ToList();
            }
            catch
            {
                return BadRequest("Ids must be ints separated by coma");
            }

            var goods = await _goodService.GetGoodsByIds(idsList);

            return Ok(goods);
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet]
        public async Task<IActionResult> GetGoodsAsync(
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

            var catGoodsDto = await _goodService.GetGoodsAsync(queryProps);

            return Ok(catGoodsDto);
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
        public async Task<IActionResult> GetDogGoodsAsync(
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
        public async Task<IActionResult> GetOtherGoodsAsync(
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

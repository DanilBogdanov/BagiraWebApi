using BagiraServer.Services.Parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BagiraWebApi.Controllers.api.Parser.v1
{
    [Route("api/parser/v1/bagira")]
    [ApiController]
    public class ParserBagiraGoodController : ControllerBase
    {
        private readonly ParserService _parserService;

        public ParserBagiraGoodController(ParserService parserService)
        {
            _parserService = parserService;
        }

        [OutputCache(PolicyName = "GoodsTag")]
        [HttpGet("goods/names")]
        public async Task<IActionResult> GetBagiraGoodNamesAsync()
        {
            var names = await _parserService.GetBagiraGoodNamesAsync();

            return Ok(names);
        }

        [HttpGet("menu")]
        public async Task<IActionResult> GetBagiraMenuAsync()
        {
            var menu = await _parserService.GetBagiraMenuAsync();

            return Ok(menu);
        }

        [HttpGet("goods")]
        public async Task<IActionResult> GetGoods(int? parentId, int take = 20, int skip = 0)
        {
            var goods = await _parserService.GetBagiraGoodsAsync(parentId, take, skip);

            return Ok(goods);
        }
    }
}

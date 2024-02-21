using BagiraServer.Services.Parser;
using BagiraWebApi.Services.Parser.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Parser.v1
{
    [Route("api/parser/v1")]
    [ApiController]
    public class ParserGoodsController : ControllerBase
    {
        private readonly ParserService _parserService;

        public ParserGoodsController(ParserService parserService)
        {
            _parserService = parserService;
        }

        [HttpGet("companies/{parserCompanyId}/goods")]
        public async Task<IActionResult> GetGoodsAsync(int parserCompanyId, string brand, [FromQuery] ParserGoodRequestParam param)
        {
            var goods = await _parserService.GetParserGoodsAsync(parserCompanyId, brand, param);

            return Ok(goods);
        }

        [HttpPut("companies/{parserCompanyId}/goods/{parserGoodId}/bagiraGoodId")]
        public IActionResult SetLinkToBagiraGood(int parserCompanyId, int parserGoodId, int bagiraGoodId)
        {
            var setBagiraGoodId = _parserService.SetLinkToBagira(parserCompanyId, parserGoodId, bagiraGoodId);

            if (setBagiraGoodId == null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }    
}

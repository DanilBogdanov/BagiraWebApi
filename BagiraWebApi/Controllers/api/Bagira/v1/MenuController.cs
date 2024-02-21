using BagiraWebApi.Services.Bagira;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("api/bagira/v1/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private MenuService _bagiraService;

        public MenuController(MenuService bagiraService)
        {
            _bagiraService = bagiraService;
        }

        [OutputCache(PolicyName = "GoodsMenuTag")]
        [HttpGet]
        public async Task<IActionResult> GetMenuAsync()
        {
            return Ok(await _bagiraService.GetMenuAsync(null));
        }

        [OutputCache(PolicyName = "GoodsMenuTag")]
        [HttpGet("cats")]
        public async Task<IActionResult> GetCatMenuAsync()
        {
            return Ok(await _bagiraService.GetCatMenuAsync());
        }

        [OutputCache(PolicyName = "GoodsMenuTag")]
        [HttpGet("dogs")]
        public async Task<IActionResult> GetDogMenuAsync()
        {
            return Ok(await _bagiraService.GetDogMenuAsync());
        }

        [OutputCache(PolicyName = "GoodsMenuTag")]
        [HttpGet("others")]
        public async Task<IActionResult> GetOtherMenuAsync()
        {
            return Ok(await _bagiraService.GetOtherMenuAsync());
        }
    }
}

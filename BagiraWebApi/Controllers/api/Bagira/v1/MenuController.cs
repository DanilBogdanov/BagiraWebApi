using BagiraWebApi.Services.Bagira;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("cat")]
        public async Task<IActionResult> GetCatMenuAsync()
        {
            return Ok(await _bagiraService.GetCatMenuAsync());
        }
        
        [HttpGet("dog")]
        public async Task<IActionResult> GetDogMenuAsync()
        {
            return Ok(await _bagiraService.GetDogMenuAsync());
        }

        [HttpGet("other")]
        public async Task<IActionResult> GetOtherMenuAsync()
        {
            return Ok(await _bagiraService.GetOtherMenuAsync());
        }
    }
}

using BagiraWebApi.Services.Bagira;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("api/bagira/v1/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private BagiraService _bagiraService;

        public MenuController(BagiraService bagiraService)
        {
            _bagiraService = bagiraService;
        }

        [HttpGet("cat")]
        public IActionResult GetCatMenu()
        {
            return Ok(_bagiraService.Menu.GetCatMenu());
        }
        
        [HttpGet("dog")]
        public IActionResult GetDogMenu()
        {
            return Ok(_bagiraService.Menu.GetDogMenu());
        }

        [HttpGet("other")]
        public IActionResult GetOtherMenu()
        {
            return Ok(_bagiraService.Menu.GetOtherMenu());
        }
    }
}

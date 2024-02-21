using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Bagira.v1
{
    [Route("/catalog/{**slug}")]
    [ApiController]
    public class SpaController : ControllerBase
    {
        [HttpGet()]
        public VirtualFileResult GetIndex()
        {
            return File("~/index.html", "text/html");
        }
    }
}

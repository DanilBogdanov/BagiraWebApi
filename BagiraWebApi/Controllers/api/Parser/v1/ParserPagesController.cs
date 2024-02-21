using BagiraServer.Services.Parser;
using BagiraWebApi.Models.Parser;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Parser.v1
{
    [Route("api/parser/v1")]
    [ApiController]
    public class ParserPagesController : ControllerBase
    {
        private readonly ParserService _parserService;

        public ParserPagesController(ParserService parserService)
        {
            _parserService = parserService;
        }

        [HttpGet("companies/{parserCompanyId}/pages")]
        public IActionResult Get(int parserCompanyId)
        {
            var pages = _parserService.GetParserPages(parserCompanyId);
            return Ok(pages);
        }

        [HttpPost("companies/{parserCompanyId}/pages")]
        public IActionResult Post(ParserPage parserPage)
        {
            var createdPage = _parserService.AddParserPage(parserPage);
            return Ok(createdPage);
        }

        [HttpPut("companies/{parserCompanyId}/pages/{pageId}")]
        public IActionResult Put(int pageId, ParserPage parserPage)
        {
            var updatedPage = _parserService.UpdateParserPage(parserPage);

            if (updatedPage == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("companies/{parserCompanyId}/pages/{pageId}")]
        public IActionResult DeletePage(int pageId)
        {
            var deletedPage = _parserService.DeleteParserPage(pageId);
            if (deletedPage != null)
            {
                return Ok(deletedPage);
            }
            else
            {
                return NoContent();
            }
        }

        [HttpPut("companies/{parserCompanyId}/pages/{pageId}/is-active")]
        public IActionResult PutPageIsAction(int pageId, bool isActive)
        {
            var pageIsActive = _parserService.UpdatePageIsActive(pageId, isActive);
            if (pageIsActive != null)
            {
                return NoContent();
            }
            else
            {
                return NotFound($"Not found page with id : {pageId}");
            }
        }
    }
}

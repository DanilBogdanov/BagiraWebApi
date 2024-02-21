using BagiraServer.Services.Parser;
using BagiraWebApi.Models.Parser;
using Microsoft.AspNetCore.Mvc;

namespace BagiraWebApi.Controllers.api.Parser.v1
{
    [Route("api/parser/v1")]
    [ApiController]
    public class ParserCompaniesController : ControllerBase
    {
        private readonly ParserService _parserService;

        public ParserCompaniesController(ParserService parserService)
        {
            _parserService = parserService;
        }

        [HttpGet("companies")]
        public IActionResult GetCompanies()
        {
            var companies = _parserService.GetParserCompanies();
            return Ok(companies);
        }

        [HttpGet("companies/{parserCompanyId}/brands")]
        public IActionResult GetBrands(int parserCompanyId)
        {
            var result = _parserService.GetCompanyBrands(parserCompanyId);
            return Ok(result);
        }
    }
}

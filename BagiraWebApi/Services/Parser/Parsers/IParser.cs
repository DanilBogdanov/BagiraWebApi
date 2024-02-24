using BagiraWebApi.Models.Parser;

namespace BagiraWebApi.Services.Parser.Parsers
{
    public interface IParser
    {
        int ParserCompanyId { get; }
        Task<List<ParserGood>> ParseAsync(string url);
    }
}

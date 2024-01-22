using BagiraWebApi.Models.Parser;

namespace BagiraServer.Services.Parser
{
    public interface IParser
    {
        int ParserCompanyId { get; }
        Task<List<ParserGood>> ParseAsync(string url);
    }
}

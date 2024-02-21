using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace BagiraWebApi.Services.Parser.DTO
{
    public class ParserGoodResponse<T>
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int Total { get; set; }
        public required T Result { get; set; }
    }
}

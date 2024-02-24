using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Parser;

namespace BagiraWebApi.Services.Parser.Models.DTO
{
    public class ParserBagiraGoodDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public List<ParserGood> ParserGoods { get; set; } = null!;
    }
}

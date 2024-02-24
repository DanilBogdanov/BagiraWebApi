using BagiraWebApi.Models.Parser;

namespace BagiraWebApi.Services.Parser.DTO
{
    public class ParserBagiraGoodDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public List<ParserGood> ParserGoods { get; set; } = null!;
    }
}

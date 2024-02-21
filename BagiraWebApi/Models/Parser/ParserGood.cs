namespace BagiraWebApi.Models.Parser
{
    public class ParserGood
    {
        public int Id { get; set; }
        public int ParserCompanyId { get; set; }
        public int? GoodId { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Brand { get; set; } = "No Name";
        public required string Name { get; set; }
        public string Weight { get; set; } = string.Empty;
        public float Price { get; set; }
        public float SalePrice { get; set; }
        public string? ImgUrl { get; set; }
    }
}

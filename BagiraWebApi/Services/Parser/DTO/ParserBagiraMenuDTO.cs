namespace BagiraWebApi.Services.Parser.DTO
{
    public class ParserBagiraMenuDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Path { get; set; } = null!;   
        public List<ParserBagiraMenuDTO>? Children { get; set; }
    }
}

namespace BagiraWebApi.Models.Parser
{
    public class ParserPage
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public int ParserCompanyId { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
    }
}

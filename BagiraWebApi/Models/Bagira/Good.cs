namespace BagiraWebApi.Models.Bagira
{
    public class Good
    {
        public int Id { get; set; }
        public string DataVersion { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public bool IsGroup { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

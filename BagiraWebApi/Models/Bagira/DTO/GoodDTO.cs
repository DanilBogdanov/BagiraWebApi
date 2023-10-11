namespace BagiraWebApi.Models.Bagira.DTO
{
    public class GoodDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
        public decimal? Price { get; set; }
    }
}

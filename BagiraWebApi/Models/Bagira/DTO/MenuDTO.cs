namespace BagiraWebApi.Models.Bagira.DTO
{
    public class MenuDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<MenuDTO>? Children { get; set; }
    }
}

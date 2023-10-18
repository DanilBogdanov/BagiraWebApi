namespace BagiraWebApi.Models.Bagira.DTO
{
    public class GoodsDTO
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int Count { get; set; }
        public List<GoodDTO> Results { get; set; } = null!;
    }
}

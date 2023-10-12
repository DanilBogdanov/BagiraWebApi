using BagiraWebApi.Models.Bagira.DTO;

namespace BagiraWebApi.Services.Bagira.DataModels
{
    public class BagiraGoodsResponse
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int Count { get; set; }
        public List<GoodDTO> Results { get; set; } = null!;
    }
}

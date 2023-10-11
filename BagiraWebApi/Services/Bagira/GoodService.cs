using BagiraWebApi.Models.Bagira.DTO;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi.Services.Bagira
{
    public class GoodService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public GoodService(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<GoodDTO?> GetGoodAsync(int id)
        {
            var good = await _context.Goods.Where(g => !g.IsGroup)
                .FirstOrDefaultAsync(good => good.Id == id);
            if (good == null)
            {
                return null;
            }
            return new GoodDTO
            {
                Id = good.Id,
                Name = good.FullName,
                Description = good.Description,
                ImgUrl = good.ImgUrl,
            };
        }
    }
}

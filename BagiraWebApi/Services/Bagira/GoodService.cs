using BagiraWebApi.Models.Bagira.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

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

        public async Task<List<GoodDTO>> GetCatGoodsByGroupAsync(int groupId)
        {
            var configPath = "1c:Properties:ValueIds:ForCats";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var catValueIds = configValue.Split(",");

            var goods = await GetGoodsByGroup(groupId, catValueIds);
            return goods;
        }

        public async Task<List<GoodDTO>> GetDogGoodsByGroupAsync(int groupId)
        {
            var configPath = "1c:Properties:ValueIds:ForDogs";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var dogValueIds = configValue.Split(",");

            var goods = await GetGoodsByGroup(groupId, dogValueIds);
            return goods;
        }

        public async Task<List<GoodDTO>> GetOtherGoodsByGroupAsync(int groupId)
        {
            var configPath = "1c:Properties:ValueIds:ForOthers";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var otherValueIds = configValue.Split(",");

            var goods = await GetGoodsByGroup(groupId, otherValueIds);
            return goods;
        }

        public async Task<List<GoodDTO>> GetGoodsByGroup(int groupId, string[] propValues)
        {
            var storageConfigPath = "1c:DefaultStorage";
            var storageConfigValue = _configuration[storageConfigPath]
                ?? throw new Exception($"Not found configuration: {storageConfigPath}");
            var storageId = int.Parse(storageConfigValue);
            
            var priceTypeConfigPath = "1c:DefaultPriceType";
            var priceTypeConfigValue = _configuration[priceTypeConfigPath]
                ?? throw new Exception($"Not found configuration: {priceTypeConfigPath}");
            var priceTypeId = int.Parse(priceTypeConfigValue);

            var animalConfigPath = "1c:Properties:Ids:Animal";
            var animalPropertyId = _configuration[animalConfigPath]
                ?? throw new Exception($"Not found configuration: {animalConfigPath}");

            var goods = await _context.Goods
                .Where(good =>
                    good.Path.Contains($"/{groupId}/")
                    && !good.IsGroup
                    && good.ImgUrl != null
                    && _context.GoodRests
                        .Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == storageId)
                    && _context.GoodPropertyValues
                        .Any(
                            gpv => gpv.GoodId == good.Id
                            && gpv.PropertyId == animalPropertyId
                            && propValues.Contains(gpv.ValueId)
                            )
                    )
                .Join(_context.GoodPrices.Where(gp => gp.PriceTypeId == priceTypeId),
                    good => good.Id,
                    gp => gp.GoodId,
                    (good, goodPrice) => new GoodDTO
                    {
                        Id = good.Id,
                        Name = good.FullName,
                        ImgUrl = good.ImgUrl,
                        Price = goodPrice.Price
                    })
                .ToListAsync();
            return goods;
        }
    }
}

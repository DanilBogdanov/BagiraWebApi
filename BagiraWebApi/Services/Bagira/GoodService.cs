using BagiraWebApi.Models.Bagira.DTO;
using BagiraWebApi.Services.Bagira.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

namespace BagiraWebApi.Services.Bagira
{
    public class GoodService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public GoodService(IConfiguration configuration, ApplicationContext context)
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

        public async Task<GoodsDTO> GetCatGoodsAsync(BagiraQueryProps queryProps)
        {
            var configPath = "1c:Properties:ValueIds:ForCats";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var catValueIds = configValue.Split(",");
            queryProps.PropertyValuesIds = catValueIds;
            var goods = await GetGoods(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetDogGoodsAsync(BagiraQueryProps queryProps)
        {
            var configPath = "1c:Properties:ValueIds:ForDogs";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var dogValueIds = configValue.Split(",");
            queryProps.PropertyValuesIds = dogValueIds;
            var goods = await GetGoods(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetOtherGoodsAsync(BagiraQueryProps queryProps)
        {
            var configPath = "1c:Properties:ValueIds:ForOthers";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var otherValueIds = configValue.Split(",");
            queryProps.PropertyValuesIds = otherValueIds;
            var goods = await GetGoods(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetGoods(BagiraQueryProps queryProps)
        {
            var take = queryProps.Take ?? 20;
            var skip = queryProps.Skip ?? 0;

            var storageConfigPath = "1c:DefaultStorage";
            var storageConfigValue = _configuration[storageConfigPath]
                ?? throw new Exception($"Not found configuration: {storageConfigPath}");
            var storageId = int.Parse(storageConfigValue);
            
            var priceTypeConfigPath = "1c:DefaultPriceType";
            var priceTypeConfigValue = _configuration[priceTypeConfigPath]
                ?? throw new Exception($"Not found configuration: {priceTypeConfigPath}");
            var priceTypeId = int.Parse(priceTypeConfigValue);

            var goodsQuery = _context.Goods
                .Where(good =>
                    (queryProps.GroupId == null || good.Path.Contains($"/{queryProps.GroupId}/"))
                    && !good.IsGroup
                    && good.ImgUrl != null
                    && _context.GoodRests
                        .Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == storageId)
                    && (queryProps.PropertyValuesIds == null || _context.GoodPropertyValues
                        .Any(
                            gpv => gpv.GoodId == good.Id
                            && queryProps!.PropertyValuesIds.Contains(gpv.ValueId)
                            ))
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
                    });
            var goodsCount = await goodsQuery.CountAsync();
            var goods = await goodsQuery
                .OrderBy(g => g.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
            
            return new GoodsDTO 
            { 
                Take = take, 
                Skip = skip,
                Count = goodsCount,
                Results = goods
            };
        }
    }
}

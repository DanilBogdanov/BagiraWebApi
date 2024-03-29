using BagiraWebApi.Models.Bagira.DTO;
using BagiraWebApi.Services.Auth;
using BagiraWebApi.Services.Bagira.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace BagiraWebApi.Services.Bagira
{
    public class GoodService
    {
        const string IMG_400_DIRECTORY = "/bagira/img/400";
        const string IMG_800_DIRECTORY = "/bagira/img/800";
        private readonly ApplicationContext _context;
        private readonly BagiraConfig _bagiraConfig;

        public GoodService(ApplicationContext context, IOptions<BagiraConfig> options)
        {
            _context = context;
            _bagiraConfig = options.Value;
        }

        public async Task<GoodsDTO> SearchAsync(BagiraQueryProps queryProps)
        {
            var goods = await GetGoodsAsync(queryProps);
            return goods;
        }

        public async Task<List<GoodGroupDTO>> GetGoodGroupsAsync()
        {
            var groups = await _context.Goods.Where(g => g.IsGroup)
                .Select(g => new GoodGroupDTO { Id = g.Id, Name = g.Name, Path = g.Path })
                .ToListAsync();
            return groups;
        }

        public async Task<GoodDTO?> GetGoodAsync(int id)
        {
            var good = await _context.Goods.Where(g => !g.IsGroup)
                .FirstOrDefaultAsync(good => good.Id == id);
            if (good == null)
            {
                return null;
            }

            var price = await _context.GoodPrices
                .FirstOrDefaultAsync(pr => 
                    pr.PriceTypeId == _bagiraConfig.DefaultPriceType 
                    && pr.GoodId == id);

            string? imgUrl = good.ImgDataVersion != null 
                ? Path.Combine(IMG_800_DIRECTORY, $"{id}.jpg") 
                : null;

            return new GoodDTO
            {
                Id = good.Id,
                Name = good.FullName,
                Description = good.Description,
                ImgUrl = imgUrl,
                Price = price?.Price
            };
        }

        public async Task<GoodsDTO> GetCatGoodsAsync(BagiraQueryProps queryProps)
        {
            queryProps.PropertyValuesIds = _bagiraConfig.Properties.ValueIds.ForCats;
            var goods = await GetGoodsAsync(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetDogGoodsAsync(BagiraQueryProps queryProps)
        {
            queryProps.PropertyValuesIds = _bagiraConfig.Properties.ValueIds.ForDogs;
            var goods = await GetGoodsAsync(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetOtherGoodsAsync(BagiraQueryProps queryProps)
        {
            queryProps.PropertyValuesIds = _bagiraConfig.Properties.ValueIds.ForOthers;
            var goods = await GetGoodsAsync(queryProps);
            return goods;
        }

        public async Task<GoodsDTO> GetGoodsAsync(BagiraQueryProps queryProps)
        {
            var take = queryProps.Take ?? 20;
            var skip = queryProps.Skip ?? 0;

            string? query = null;
            if (queryProps.Query != null)
            {
                var pattern = @"\s+";
                Regex rgx = new Regex(pattern);
                var matches = rgx.Split(queryProps.Query.Trim());
                query = "\"" + string.Join("*\" and \"", matches) + "*\"";
                Console.WriteLine($"{queryProps.Query}:{query}");
            }

            var goodsQuery = _context.Goods
                .Where(good =>
                    (query == null || good.KeyWords == null || EF.Functions.Contains(good.KeyWords, query))
                    && (queryProps.GroupId == null || good.Path.Contains($"/{queryProps.GroupId}/"))
                    && !good.IsGroup
                    && good.ImgDataVersion != null
                    && _context.GoodRests
                        .Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == _bagiraConfig.DefaultStorage)
                    && (queryProps.PropertyValuesIds == null || _context.GoodPropertyValues
                        .Any(
                            gpv => gpv.GoodId == good.Id
                            && queryProps!.PropertyValuesIds.Contains(gpv.ValueId)
                            ))
                    )
                .Join(_context.GoodPrices.Where(gp => gp.PriceTypeId == _bagiraConfig.DefaultPriceType),
                    good => good.Id,
                    gp => gp.GoodId,
                    (good, goodPrice) => new GoodDTO
                    {
                        Id = good.Id,
                        Name = good.FullName,
                        ImgUrl = good.ImgDataVersion != null ? Path.Combine(IMG_400_DIRECTORY, $"{good.Id}.jpg"): null,
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

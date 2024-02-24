using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Parser;
using BagiraWebApi.Services.Parser.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;

namespace BagiraWebApi.Services.Parser.Services
{
    public class ParserBagiraService
    {
        private readonly ApplicationContext _context;

        public ParserBagiraService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<BagiraGoodNameDTO>> GetGoodNamesAsync()
        {
            var names = await _context.Goods
                .Where(good => !good.IsGroup)
                .Select(good => new BagiraGoodNameDTO { Id = good.Id, Name = good.Name })
                .ToListAsync();

            return names;
        }

        public async Task<ParserGoodResponse<ParserBagiraGoodDTO>> GetGoodsAsync(int? parentId, int take, int skip)
        {
            var goodsQuery = _context.Goods
                .Where(good =>
                    !good.IsGroup
                    && (parentId == null ||
                        good.Path.Contains($"/{parentId}/")
                    )
                    && _context.ParserGoods.Any(parserGood => parserGood.GoodId == good.Id)
                );

            var total = await goodsQuery.CountAsync();

            GoodPrice defaultPrice = new() { Price = 0 };
            var goods = await goodsQuery
                .Select(good => new ParserBagiraGoodDTO
                {
                    Id = good.Id,
                    Name = good.Name,
                    Price = (_context.GoodPrices
                                .FirstOrDefault(gp => gp.GoodId == good.Id && gp.PriceTypeId == 7) ?? defaultPrice).Price,
                    ParserGoods = _context.ParserGoods
                                      .Where(parserGood => parserGood.GoodId == good.Id)
                                      .ToList()
                })
                .OrderBy(good => good.Name)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new ParserGoodResponse<ParserBagiraGoodDTO>
            {
                Take = take,
                Skip = skip,
                Total = total,
                Result = goods
            };
        }

        public async Task<List<ParserBagiraMenuDTO>> GetMenuAsync()
        {
            var groups = await _context.Goods
                .Where(group =>
                    group.IsGroup
                    && _context.Goods.Any
                        (g => !g.IsGroup
                            && g.Path.Contains("/" + group.Id + "/")
                            && _context.ParserGoods.Any(parserGood => parserGood.GoodId == g.Id)
                        )
                    )
                .ToListAsync();

            var menu = MakeMenu(groups, null);

            return menu;
        }

        private static List<ParserBagiraMenuDTO> MakeMenu(List<Good> groups, int? parentId)
        {
            var menu = groups.Where(gr => gr.ParentId == parentId)
                .Select(group => new ParserBagiraMenuDTO { Id = group.Id, Name = group.Name, Path = group.Path })
                .ToList();

            foreach (var item in menu)
            {
                if (groups.Any(gr => gr.ParentId == item.Id))
                {
                    item.Children = MakeMenu(groups, item.Id);
                }
            }

            return menu;
        }
    }
}

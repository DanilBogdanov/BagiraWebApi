using BagiraWebApi.Models.Bagira;
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

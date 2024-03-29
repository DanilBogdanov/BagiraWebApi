using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Bagira.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BagiraWebApi.Services.Bagira
{
    public class MenuService
    {
        private readonly ApplicationContext _context;
        private readonly BagiraConfig _bagiraConfig;

        public MenuService(ApplicationContext context, IOptions<BagiraConfig> options)
        {
            _context = context;
            _bagiraConfig = options.Value;
        }

        public async Task<List<MenuDTO>> GetCatMenuAsync()
        {
            var menu = await GetMenuAsync(_bagiraConfig.Properties.ValueIds.ForCats);

            return menu;
        }

        public async Task<List<MenuDTO>> GetDogMenuAsync()
        {
            var menu = await GetMenuAsync(_bagiraConfig.Properties.ValueIds.ForDogs);

            return menu;
        }

        public async Task<List<MenuDTO>> GetOtherMenuAsync()
        {
            var menu = await GetMenuAsync(_bagiraConfig.Properties.ValueIds.ForOthers);

            return menu;
        }

        public async Task<List<MenuDTO>> GetMenuAsync(string[]? valueIds)
        {
            var groups = await _context.Goods
                .Where(
                    g => g.IsGroup
                    && _context.Goods.Any(
                        good => good.Path.Contains("/" + g.Id + "/")
                        && good.ImgDataVersion != null
                        && (valueIds == null || _context.GoodPropertyValues.Any(
                            gpv => gpv.GoodId == good.Id
                            && gpv.PropertyId == _bagiraConfig.Properties.Ids.Animal
                            && valueIds.Contains(gpv.ValueId)
                            ))
                        && _context.GoodRests.Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == _bagiraConfig.DefaultStorage)
                        )
                    )
                .ToListAsync();
            var menu = MakeMenu(groups, null);

            return menu;
        }

        private static List<MenuDTO> MakeMenu(List<Good> groups, int? parentId)
        {
            var menu = groups.Where(gr => gr.ParentId == parentId)
                .Select(group => new MenuDTO { Id = group.Id, Name = group.Name }).ToList();

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

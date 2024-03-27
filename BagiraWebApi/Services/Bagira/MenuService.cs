using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Bagira.DTO;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi.Services.Bagira
{
    public class MenuService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;

        public MenuService(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<List<MenuDTO>> GetCatMenuAsync()
        {
            var configPath = "1c:Properties:ValueIds:ForCats";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var catValueIds = configValue.Split(",");
            var menu = await GetMenuAsync(catValueIds);

            return menu;
        }

        public async Task<List<MenuDTO>> GetDogMenuAsync()
        {
            var configPath = "1c:Properties:ValueIds:ForDogs";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var dogValueIds = configValue.Split(",");
            var menu = await GetMenuAsync(dogValueIds);

            return menu;
        }

        public async Task<List<MenuDTO>> GetOtherMenuAsync()
        {
            var configPath = "1c:Properties:ValueIds:ForOthers";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var othersValueIds = configValue.Split(",");
            var menu = await GetMenuAsync(othersValueIds);

            return menu;
        }

        public async Task<List<MenuDTO>> GetMenuAsync(string[]? valueIds)
        {
            var storageConfigPath = "1c:DefaultStorage";
            var storageConfigValue = _configuration[storageConfigPath]
                ?? throw new Exception($"Not found configuration: {storageConfigPath}");
            var storageId = int.Parse(storageConfigValue);

            var animalConfigPath = "1c:Properties:Ids:Animal";
            var animalPropertyId = _configuration[animalConfigPath]
                ?? throw new Exception($"Not found configuration: {animalConfigPath}");
            var groups = await _context.Goods
                .Where(
                    g => g.IsGroup
                    && _context.Goods.Any(
                        good => good.Path.Contains("/" + g.Id + "/")
                        && good.ImgDataVersion != null
                        && (valueIds == null || _context.GoodPropertyValues.Any(
                            gpv => gpv.GoodId == good.Id
                            && gpv.PropertyId == animalPropertyId
                            && valueIds.Contains(gpv.ValueId)
                            ))
                        && _context.GoodRests.Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == storageId)
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

using BagiraWebApi.Models.Bagira.DTO;
using BagiraWebApi.Models.Bagira;

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

        public List<MenuDTO> GetCatMenu()
        {
            var configPath = "1c:Properties:ValueIds:ForCats";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var catValueIds = configValue.Split(",");
            var menu = GetMenu(catValueIds);

            return menu;
        }

        public List<MenuDTO> GetDogMenu()
        {
            var configPath = "1c:Properties:ValueIds:ForDogs";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var dogValueIds = configValue.Split(",");
            var menu = GetMenu(dogValueIds);

            return menu;
        }

        public List<MenuDTO> GetOtherMenu()
        {
            var configPath = "1c:Properties:ValueIds:ForOthers";
            var configValue = _configuration[configPath]
                ?? throw new Exception($"Not found configuration: {configPath}");
            var othersValueIds = configValue.Split(",");
            var menu = GetMenu(othersValueIds);

            return menu;
        }

        private List<MenuDTO> GetMenu(string[] valueIds)
        {
            var storageConfigPath = "1c:DefaultStorage";
            var storageConfigValue = _configuration[storageConfigPath]
                ?? throw new Exception($"Not found configuration: {storageConfigPath}");
            var storageId = int.Parse(storageConfigValue);

            var animalConfigPath = "1c:Properties:Ids:Animal";
            var animalPropertyId = _configuration[animalConfigPath]
                ?? throw new Exception($"Not found configuration: {animalConfigPath}");
            var groups = _context.Goods
                .Where(
                    g => g.IsGroup
                    && _context.Goods.Any(
                        good => good.Path.Contains("/" + g.Id + "/")
                        && good.ImgUrl != null
                        && _context.GoodPropertyValues.Any(
                            gpv => gpv.GoodId == good.Id
                            && gpv.PropertyId == animalPropertyId
                            && valueIds.Contains(gpv.ValueId)
                            )
                        && _context.GoodRests.Any(goodRest => goodRest.GoodId == good.Id && goodRest.StorageId == storageId)
                        )
                    )
                .ToList();
            var menu = MakeMenu(groups, null);

            return menu;
        }

        private static List<MenuDTO> MakeMenu(List<Good> groups, int? parentId)
        {
            var menu = groups.Where(gr => gr.ParentId == parentId)
                .Select(group => new MenuDTO { Id = group.Id, Name = group.Name, Path = group.Path }).ToList();

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

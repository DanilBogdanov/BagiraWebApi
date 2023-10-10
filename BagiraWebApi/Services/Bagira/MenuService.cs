using BagiraWebApi.Models.Bagira.DTO;
using BagiraWebApi.Models.Bagira;

namespace BagiraWebApi.Services.Bagira
{
    public class MenuService
    {
        ApplicationContext _context;
        IConfiguration _configuration;

        public MenuService(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public List<MenuDTO> GetCatMenu()
        {
            var catValueId = _configuration["1c:Properties:ValueIds:Cat"];
            var catAndDogValueId = _configuration["1c:Properties:ValueIds:Cat&Dog"];
            var groups = GetGroups(new List<string> { catValueId, catAndDogValueId });
            var menu = GetMenu(groups, null);

            return menu;
        }

        public List<MenuDTO> GetDogMenu()
        {
            var dogValueId = _configuration["1c:Properties:ValueIds:Dog"];
            var catAndDogValueId = _configuration["1c:Properties:ValueIds:Cat&Dog"];
            var groups = GetGroups(new List<string> { dogValueId, catAndDogValueId });
            var menu = GetMenu(groups, null);

            return menu;
        }

        public List<MenuDTO> GetOtherMenu()
        {
            var rodentValueId = _configuration["1c:Properties:ValueIds:Rodent"];
            var horseValueId = _configuration["1c:Properties:ValueIds:Horse"];
            var birdValueId = _configuration["1c:Properties:ValueIds:Bird"];
            var birdAndRodentValueId = _configuration["1c:Properties:ValueIds:BirdAndRodent"];
            var reptileValueId = _configuration["1c:Properties:ValueIds:Reptile"];
            var fishValueId = _configuration["1c:Properties:ValueIds:Fish"];
            var otherValueId = _configuration["1c:Properties:ValueIds:Other"];
            var groups = GetGroups(new List<string> {
                rodentValueId,
                horseValueId,
                birdValueId,
                birdAndRodentValueId,
                reptileValueId,
                fishValueId,
                otherValueId
            });
            var menu = GetMenu(groups, null);

            return menu;
        }

        private List<Good> GetGroups(List<string> valueIds)
        {
            var storageId = int.Parse(_configuration["1c:DefaultStorage"]);
            var animalPropertyId = _configuration["1c:Properties:Ids:Animal"];
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

            return groups;
        }

        private List<MenuDTO> GetMenu(List<Good> groups, int? parentId)
        {
            var menu = groups.Where(gr => gr.ParentId == parentId)
                .Select(group => new MenuDTO { Id = group.Id, Name = group.Name, Path = group.Path }).ToList();

            foreach (var item in menu)
            {
                if (groups.Any(gr => gr.ParentId == item.Id))
                {
                    item.Children = GetMenu(groups, item.Id);
                }
            }

            return menu;
        }
    }
}

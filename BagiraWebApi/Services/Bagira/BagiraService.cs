using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Bagira.DTO;
using System.Linq;

namespace BagiraWebApi.Services.Bagira
{
    public class BagiraService
    {
        ApplicationContext _context;
        IConfiguration _configuration;
        MenuService? _menuService;
        public MenuService Menu
        {
            get
            {
                if (_menuService == null)
                {
                    _menuService = new MenuService(_context, _configuration);
                }
                return _menuService;
            }
        }


        public BagiraService(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        
    }
}

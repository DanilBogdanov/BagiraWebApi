namespace BagiraWebApi.Services.Bagira
{
    public class BagiraService
    {
        private readonly ApplicationContext _context;
        private readonly IConfiguration _configuration;
        private MenuService? _menuService;
        private GoodService? _goodService;

        public MenuService Menu
        {
            get
            {
                _menuService ??= new MenuService(_context, _configuration);
                return _menuService;
            }
        }

        public GoodService Goods
        {
            get
            {
                _goodService ??= new GoodService(_context, _configuration);
                return _goodService;
            }
        }

        public BagiraService(ApplicationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }        
    }
}

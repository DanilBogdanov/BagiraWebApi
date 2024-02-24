using BagiraWebApi;
using BagiraWebApi.Models.Bagira;
using BagiraWebApi.Models.Bagira.DTO;
using BagiraWebApi.Models.Parser;
using BagiraWebApi.Services.Parser.Models;
using BagiraWebApi.Services.Parser.Models.DTO;
using BagiraWebApi.Services.Parser.Parsers;
using BagiraWebApi.Services.Parser.Services;
using Microsoft.EntityFrameworkCore;

namespace BagiraServer.Services.Parser
{
    public class ParserService
    {
        private readonly ApplicationContext _appContext;
        private readonly ILogger<ParserService> _logger;
        public static readonly ParserCompany Petshop = new() { Id = 1, Name = "Petshop" };
        public static readonly ParserCompany Vetna = new() { Id = 2, Name = "Vetna" };
        
        private ParserBagiraService? bagiraService;
        private ParserBagiraService BagiraService
        {
            get
            {
                bagiraService ??= new ParserBagiraService(_appContext);

                return bagiraService;
            }
        }

        public ParserService(ApplicationContext appContext, ILogger<ParserService> logger)
        {
            _appContext = appContext;
            _logger = logger;
        }

        public async Task UpdateParserGoodsAsync()
        {
            var petshopParser = new PetshopParser(Petshop.Id);
            var vetnaParser = new VetnaParser(Vetna.Id);

            _logger.LogInformation($"*** Start parsing");
            await ParseAsync(petshopParser);
            await ParseAsync(vetnaParser);
            _logger.LogInformation($"*** Stop parsing");
        }

        public List<ParserCompany> GetParserCompanies()
        {
            return new List<ParserCompany> { Petshop, Vetna };
        }

        public List<string> GetCompanyBrands(int parserCompanyId)
        {
            return _appContext.ParserGoods.AsNoTracking().Where(x => x.ParserCompanyId == parserCompanyId).Select(x => x.Brand).Distinct().ToList();
        }

        public async Task<ParserGoodResponse<List<ParserGood>>> GetParserGoodsAsync(int parserCompanyId, string brand, ParserGoodRequestParam param)
        {
            var parserGoodQuery = _appContext.ParserGoods.AsNoTracking()
                .Where(x =>
                    x.ParserCompanyId == parserCompanyId
                    && x.Brand == brand
                    && (param.HasLinkToBagira == null
                        || (param.HasLinkToBagira == true && x.GoodId != null)
                        || (param.HasLinkToBagira == false && x.GoodId == null)
                    )
                );

            var total = await parserGoodQuery.CountAsync();

            var goods = await parserGoodQuery
                .OrderBy(x => x.Name)
                .Skip(param.Skip)
                .Take(param.Take)
                .ToListAsync();

            return new ParserGoodResponse<List<ParserGood>>
            {
                Take = param.Take,
                Skip = param.Skip,
                Total = total,
                Result = goods
            };
        }

        public int? SetLinkToBagira(int parserCompanyId, int parserGoodId, int goodId)
        {
            var parserGood = _appContext.ParserGoods.Find(parserGoodId, parserCompanyId);

            if (parserGood != null)
            {
                parserGood.GoodId = goodId;
                _appContext.SaveChanges();
                return parserGood.GoodId;
            }

            return null;
        }

        public List<ParserPage> GetParserPages(int parserCompanyId)
        {
            return _appContext.ParserPages.AsNoTracking().Where(x => x.ParserCompanyId == parserCompanyId).ToList();
        }

        public ParserPage AddParserPage(ParserPage parserPage)
        {
            var entity = _appContext.Add(parserPage).Entity;
            _appContext.SaveChanges();
            return entity;
        }

        public ParserPage? UpdateParserPage(ParserPage parserPage)
        {
            var pageExist = _appContext.ParserPages.AsNoTracking().Any(page => page.Id == parserPage.Id);
            if (!pageExist) return null;

            var entity = _appContext.Update(parserPage).Entity;
            _appContext.SaveChanges();
            return entity;
        }

        public bool? UpdatePageIsActive(int pageId, bool isActive)
        {
            var page = _appContext.ParserPages.Find(pageId);
            if (page == null) return null;

            page.IsActive = isActive;
            _appContext.SaveChanges();
            return page.IsActive;
        }

        public ParserPage? DeleteParserPage(int parserPageId)
        {
            var page = _appContext.ParserPages.Find(parserPageId);
            if (page != null)
            {
                var entity = _appContext.Remove(page).Entity;
                _appContext.SaveChanges();
                return entity;
            }

            return null;
        }

        public async Task<List<BagiraGoodNameDTO>> GetBagiraGoodNamesAsync() => await BagiraService.GetGoodNamesAsync();

        public async Task<List<ParserBagiraMenuDTO>> GetBagiraMenuAsync() => await BagiraService.GetMenuAsync();


        private async Task ParseAsync(IParser parser)
        {
            var parserPages = _appContext.ParserPages.Where(p => p.ParserCompanyId == parser.ParserCompanyId && p.IsActive).ToList();

            try
            {
                foreach (var page in parserPages)
                {
                    _logger.LogInformation($"Parsing page: {page.Url}");
                    _appContext.ChangeTracker.Clear();
                    var parserGoods = await parser.ParseAsync(page.Url);

                    foreach (var pGood in parserGoods)
                    {
                        var dbPGood = _appContext.ParserGoods.AsNoTracking().SingleOrDefault(x => x.Id == pGood.Id && x.ParserCompanyId == pGood.ParserCompanyId);

                        if (dbPGood != null)
                        {
                            pGood.GoodId = dbPGood.GoodId;
                            _appContext.Update(pGood);
                        }
                        else
                        {
                            _appContext.Add(pGood);
                        }
                    }

                    _appContext.SaveChanges();
                    _logger.LogInformation($"Updated goods: {parserGoods.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}

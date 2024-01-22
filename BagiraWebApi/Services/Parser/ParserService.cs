using BagiraWebApi;
using BagiraWebApi.Models.Parser;
using BagiraWebApi.Services.Parser;
using Microsoft.EntityFrameworkCore;

namespace BagiraServer.Services.Parser
{
    public class ParserService
    {
        private readonly ApplicationContext _appContext;
        private readonly ILogger<ParserService> _logger;
        public static readonly ParserCompany Petshop = new() { Id = 1, Name = "Petshop" };
        public static readonly ParserCompany Vetna = new() { Id = 2, Name = "Vetna" };

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

        public List<ParserGood> GetParserGoods(int parserCompanyId, string? brand)
        {
            return _appContext.ParserGoods.AsNoTracking()
                .Where(x => x.ParserCompanyId == parserCompanyId && (brand == null || x.Brand == brand))
                .OrderBy(x => x.Name)
                .ToList();
        }

        public ParserGood? SetLinkToBagira(int parserGoodId, int parserCompanyId, int goodId)
        {
            var parserGood = _appContext.ParserGoods.Find(parserGoodId, parserCompanyId);

            if (parserGood != null)
            {
                parserGood.GoodId = goodId;
                _appContext.SaveChanges();
                return parserGood;
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
            var page = _appContext.ParserPages.Find(parserPage.Id);
            if (page == null) return null;

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

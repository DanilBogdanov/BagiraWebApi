using AngleSharp;
using AngleSharp.Dom;
using BagiraWebApi.Models.Parser;
using IConfiguration = AngleSharp.IConfiguration;

namespace BagiraWebApi.Services.Parser.Parsers
{
    public class VetnaParser : IParser
    {
        private readonly HttpClient _httpClient;
        private readonly IBrowsingContext _context;

        private const string PAGINATION_PREFIX = "?PAGEN_3=";
        private const string ITEM_SELECTOR = "[data-entity=\"item\"]";
        private const string ITEM_ID_SELECTOR = ".catalogue-item-card1-bottom-block > span:last-child";
        private const string ITEM_NAME_SELECTOR = ".catalogue-item-card1-name";
        private const string ITEM_CURRENT_PRICE_SELECTOR = ".catalogue-item-card1-price > div:first-child > div";
        private const string ITEM_SALE_PRICE_SELECTOR = "[class^=\"action-precent-sum\"]";
        private const string ITEM_IMG_SELECTOR = ".catalogue-item-card1-foto";

        public int ParserCompanyId { get; }

        public VetnaParser(int parserCompanyId)
        {
            ParserCompanyId = parserCompanyId;
            _httpClient = new HttpClient();

            IConfiguration config = Configuration.Default;
            _context = BrowsingContext.New(config);
        }

        public async Task<List<ParserGood>> ParseAsync(string url)
        {
            List<ParserGood> goods = new();
            var page = await LoadPageAsync(url);

            goods.AddRange(GetGoodsFromPage(page));
            var lastPageNumber = GetLastPageNumber(page);

            if (lastPageNumber != null)
            {
                for (int pageNumber = 2; pageNumber <= lastPageNumber; pageNumber++)
                {
                    page = await LoadPageAsync($"{url}{PAGINATION_PREFIX}{pageNumber}");
                    goods.AddRange(GetGoodsFromPage(page));
                }
            }

            return goods;
        }

        private async Task<IDocument> LoadPageAsync(string url)
        {
            try
            {
                var res = await _httpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();
                var html = await res.Content.ReadAsStringAsync();
                IDocument document = await _context.OpenAsync(req => req.Content(html));

                return document;
            }
            catch (Exception ex)
            {
                throw new Exception($"VetnaParser: Error of load page: {url}", ex);
            }
        }

        private List<ParserGood> GetGoodsFromPage(IDocument page)
        {
            List<ParserGood> goods = new();
            var items = page.QuerySelectorAll(ITEM_SELECTOR);

            foreach (var item in items)
            {
                goods.Add(GetGoodFromItem(item));
            }

            return goods;
        }

        private ParserGood GetGoodFromItem(IElement item)
        {
            var id = GetId(item);
            var name = GetName(item);
            var brand = GetBrandFromName(name);
            var imgUrl = GetImgUrl(item);
            var price = GetPrice(item);

            return new ParserGood
            {
                Id = id,
                ParserCompanyId = ParserCompanyId,
                Name = name,
                Brand = brand,
                ImgUrl = imgUrl,
                LastUpdated = DateTime.UtcNow.AddHours(5),
                Price = price.price,
                SalePrice = price.salePrice,
            };
        }

        private static int GetId(IElement item)
        {
            string idValue = item.QuerySelector(ITEM_ID_SELECTOR)?.Text()
                ?? throw new Exception($"VetnaParser: Can't get id by selector: {ITEM_ID_SELECTOR}");

            try
            {
                idValue = idValue.Split(":")[1].Trim().Replace("00-", "");
                int id = int.Parse(idValue);

                return id;
            }
            catch
            {
                throw new Exception($"VetnaParser: Can't get id by value: {idValue}");
            }
        }

        private static string GetName(IElement item)
        {
            string name = item.QuerySelector(ITEM_NAME_SELECTOR)?.Text()
                ?? throw new Exception($"VetnaParser: Can't get name by selector: {ITEM_NAME_SELECTOR}");

            return name;
        }

        private static string GetBrandFromName(string name)
        {
            return name.Split(" ")[0];
        }

        private static string? GetImgUrl(IElement item)
        {
            var imgUrl = item.QuerySelector(ITEM_IMG_SELECTOR)?.GetAttribute("data-src");
            
            if (imgUrl != null && imgUrl != "")
            {
                return $"https://vetna.info{imgUrl}";
            }

            return null;
        }

        private static (float price, float salePrice) GetPrice(IElement item)
        {
            var currentPrice = item.QuerySelector(ITEM_CURRENT_PRICE_SELECTOR)?.Text()
                ?? throw new Exception($"VetnaParser: Can't get current price by selector: {ITEM_CURRENT_PRICE_SELECTOR}");
            currentPrice = currentPrice
                .Replace(" ", "")
                .Replace("Р", "")
                .Trim();
            var oldPrice = item.QuerySelector(ITEM_SALE_PRICE_SELECTOR)?.Text();

            float salePrice = 0;
            float price;

            if (oldPrice != null)
            {
                oldPrice = oldPrice
                    .Replace(" ", "")
                    .Replace("Р", "")
                    .Trim();
                price = float.Parse(oldPrice);
                salePrice = float.Parse(currentPrice);
            }
            else
            {
                price = float.Parse(currentPrice);
            }

            return (price, salePrice);
        }

        private static int? GetLastPageNumber(IDocument document)
        {
            var endPaginationLink = document.QuerySelector(".catalogue-pagination-prev-next a")
                ?.GetAttribute("href");

            if (endPaginationLink != null)
            {
                string charBeforePageNumber = "=";
                string lastPageNumberString = endPaginationLink[(endPaginationLink.LastIndexOf(charBeforePageNumber) + 1)..];

                if (int.TryParse(lastPageNumberString, out int lastPageNumber))
                {
                    return lastPageNumber;
                }
                else
                {
                    throw new Exception($"Vetna Parser : Can't get last page number from href - {endPaginationLink}");
                }
            }

            return null;
        }


    }
}

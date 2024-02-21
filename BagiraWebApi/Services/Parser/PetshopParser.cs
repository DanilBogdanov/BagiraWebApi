using BagiraWebApi.Models.Parser;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BagiraServer.Services.Parser
{
    public class PetshopParser : IParser
    {
        public int ParserCompanyId { get; }
        private readonly HttpClient _httpClient;
        private readonly HtmlDocument _htmlDocument;
        private readonly string _cookie = "geo_city_id=a%3A2%3A%7Bs%3A7%3A%22city_id%22%3Bi%3A4%3Bs%3A13%3A%22is_determined%22%3Bb%3A1%3B%7D";
        private const string START_URL = "https://www.petshop.ru";

        public PetshopParser(int parserCompanyId)
        {
            ParserCompanyId = parserCompanyId;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Cookie", _cookie);

            _htmlDocument = new HtmlDocument();
        }

        public async Task<List<ParserGood>> ParseAsync(string url)
        {
            await LoadHtmlAsync(url);
            CheckCityUfa();
            var goods = new List<ParserGood>();

            var petshopPage = GetPetshopPage();

            if (petshopPage.Products.TotalPages > 0)
            {
                for (int i = 1; i <= petshopPage.Products.TotalPages; i++)
                {
                    await LoadHtmlAsync(url + $"?page={i}&sb=name&so=asc&pctgr=");
                    petshopPage = GetPetshopPage();
                    goods.AddRange(ParsePage(petshopPage.Products.Products));
                }
            }

            return goods;
        }

        private async Task LoadHtmlAsync(string url)
        {
            var res = await _httpClient.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var html = await res.Content.ReadAsStringAsync();

            _htmlDocument.LoadHtml(html);
        }


        private PetshopPage GetPetshopPage()
        {
            var productsNode = _htmlDocument.DocumentNode.SelectSingleNode("//script[contains(@id,'stt-/petshop/components/ps/catalog-products-')]");
            var line = productsNode.InnerText;
            var petshopPage = JsonConvert.DeserializeObject<PetshopPage>(line)
                ?? throw new Exception("Petshop Parser: parser invalid");

            return petshopPage;
        }

        private List<ParserGood> ParsePage(List<Product> products)
        {
            List<ParserGood> parserGoods = new();

            foreach (var product in products)
            {
                foreach (var offer in product.Offers)
                {
                    ParserGood parserGood = new()
                    {
                        Id = offer.Id,
                        ParserCompanyId = ParserCompanyId,
                        LastUpdated = DateTime.UtcNow.AddHours(5),
                        Brand = product.Brand,
                        Name = product.Title,
                        Weight = (offer.Weight / 1000.0) + "кг",
                        ImgUrl = product.Photo?.Src
                    };

                    if (offer.Prices.DiscountPercent == 0)
                    {
                        parserGood.Price = offer.Prices.Price;
                    }
                    else
                    {
                        parserGood.Price = offer.Prices.OldPrice;
                        parserGood.SalePrice = offer.Prices.Price;
                    }

                    parserGoods.Add(parserGood);
                }
            }

            return parserGoods;
        }

        private void CheckCityUfa()
        {
            var cityNode = _htmlDocument.DocumentNode.SelectSingleNode("//button[@data-testid=\"City\"]");
            string cityName = cityNode.Attributes["title"].Value;

            if (cityName != "Уфа")
            {
                throw new Exception($"Petshop Parser incorect city :{cityName}");
            }
        }

        private class PetshopPage
        {
            public ProductsPage Products { get; set; } = null!;
        }

        private class ProductsPage
        {
            public List<Product> Products { get; set; } = new List<Product>();
            public int TotalPages { get; set; }
        }

        private class Product
        {
            public string Title { get; set; } = String.Empty;
            public string Brand { get; set; } = String.Empty;
            public Photo? Photo { get; set; }
            public List<Offer> Offers { get; set; } = new List<Offer>();
        }

        private class Photo
        {
            public string Src { get; set; } = String.Empty;
        }

        private class Offer
        {
            public int Id { get; set; }
            public int Weight { get; set; }
            public Prices Prices { get; set; } = null!;
        }

        private class Prices
        {
            public int Price { get; set; }
            public int OldPrice { get; set; }
            public int DiscountPercent { get; set; }
        }
    }
}

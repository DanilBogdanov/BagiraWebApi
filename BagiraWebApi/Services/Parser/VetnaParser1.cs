using BagiraWebApi.Models.Parser;
using HtmlAgilityPack;
using System.Globalization;

namespace BagiraServer.Services.Parser
{
    public class VetnaParser1
    {
        public int ParserCompanyId { get; }
        private HttpClient _httpClient;
        private HtmlDocument _htmlDocument;
        private const string START_URL = "https://vetna.info";

        public VetnaParser1(int parserCompanyId)
        {
            ParserCompanyId = parserCompanyId;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.ConnectionClose = true;

            _htmlDocument = new HtmlDocument();
        }

        public List<ParserGood> ParseAsync(string url)
        {
            List<ParserGood> goods = new();
            LoadHtml(url);

            goods.AddRange(ParsePage());

            // href="/catalog/korm-suhoy-vlazhniy/?PAGEN_3=58"
            var lastPage = _htmlDocument.DocumentNode.SelectSingleNode("//li[contains(@class, 'catalogue-pagination-prev-next')]/a")?.Attributes["href"].Value;

            if (lastPage != null)
            {
                //href="/catalog/korm-suhoy-vlazhniy/?PAGEN_3=58"  ==> 58
                int pageCount = int.Parse(lastPage[(lastPage.LastIndexOf("=") + 1)..]);

                for (int i = 2; i <= pageCount; i++)
                {
                    LoadHtml(url + "?PAGEN_3=" + i);
                    goods.AddRange(ParsePage());
                }
            }

            return goods;
        }

        private List<ParserGood> ParsePage()
        {
            List<ParserGood> goods = new();

            var items = _htmlDocument.DocumentNode.SelectNodes("//div[@data-entity='item']");

            if (items != null)
            {
                foreach (var item in items)
                {
                    var imgUrl = item.SelectSingleNode("./a[contains(@class, 'catalogue-item-card1-foto')]")
                        .Attributes["data-src"].Value;
                    imgUrl = START_URL + imgUrl;
                    //<input type="hidden" class="commerc_brand_109817" value="Феликс ( Felix)">
                    var brand = item.SelectSingleNode("./input[contains(@class, 'commerc_brand')]").Attributes["value"].Value;
                    //<input type="hidden" class="commerc_name_109817" value="Felix (Феликс) 1,5 кг">
                    if (brand == "")
                    {
                        brand = "No Name";
                    }
                    var name = item.SelectSingleNode("./input[contains(@class, 'commerc_name')]").Attributes["value"].Value;
                    //<input type="hidden" class="commerc_price_109817" value="539.00">
                    var currentPriceString = item.SelectSingleNode("./input[contains(@class, 'commerc_price')]").Attributes["value"].Value;
                    //remove space 1 555 -> 1555
                    currentPriceString = currentPriceString.Replace(" ", "");
                    //parse with dot
                    float currentPrice = float.Parse(currentPriceString, NumberStyles.Any, CultureInfo.InvariantCulture);
                    var oldPriceString = item.SelectSingleNode(".//div[contains(@class, 'action-precent-sum')]")?.InnerText;

                    float price = 0;
                    float salePrice = 0;

                    //Check if contains old price
                    //<div class="action-precent-sum-2020">599 </div>
                    if (oldPriceString != null)
                    {
                        //remove space 1 555 -> 1555
                        oldPriceString = oldPriceString.Replace(" ", "");
                        price = float.Parse(oldPriceString, NumberStyles.Any, CultureInfo.InvariantCulture);
                        salePrice = currentPrice;
                    }
                    else
                    {
                        price = currentPrice;
                    }

                    //string artNumber = item.SelectSingleNode(".//span[contains(text(), 'Артикул')]")?.InnerText ?? String.Empty;

                    //if (artNumber != String.Empty)
                    //{
                    //    artNumber = artNumber.Split(":")[1].Trim();
                    //}

                    var idString = item.SelectSingleNode(".//span[contains(text(), 'Код:')]").InnerText;
                    idString = idString.Split(":")[1].Trim();
                    var id = int.Parse(idString);

                    var good = new ParserGood
                    {
                        Id = id,
                        ParserCompanyId = ParserCompanyId,
                        LastUpdated = DateTime.UtcNow.AddHours(5),
                        Brand = brand,
                        Name = name,
                        Price = price,
                        SalePrice = salePrice,
                        ImgUrl = imgUrl,
                    };

                    goods.Add(good);
                }
            }

            return goods;
        }

        private void LoadHtml(string url)
        {
            var res = _httpClient.GetAsync(url).Result;
            res.EnsureSuccessStatusCode();
            var html = res.Content.ReadAsStringAsync().Result;
            _htmlDocument.LoadHtml(html);
        }
    }
}

﻿using BagiraWebApi.Services.Exchange1C.DataModels;
using Newtonsoft.Json;
using System.Net;

namespace BagiraWebApi.Services.Exchange1C
{
    /// <summary>
    /// Url of Bagira service should be like this: http://{host}/WebServices/ws/bagira_app.1cws
    /// </summary>
    public class Soap1C
    {
        private readonly HttpClient _httpClient;
        private const string SERVICES_FOLDER = "WebServices";
        private const string SERVICES_NAME = "bagira_app";
        private readonly string _soapServiceUrl;

        public Soap1C(IConfiguration configuration)
        {
            string host = configuration["1c:host"];
            string login = configuration["1c:login"];
            string password = configuration["1c:password"];
            _soapServiceUrl = $"http://{host}/{SERVICES_FOLDER}/ws/{SERVICES_NAME}.1cws";

            HttpClientHandler httpHandler = new()
            {
                Credentials = new NetworkCredential(login, password)
            };
            _httpClient = new HttpClient(httpHandler);
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }

        public async Task<IEnumerable<GoodStorage1C>> GetGoodStorages()
        {
            string requestBody = "<bag:GetStorages/>";
            string line = await GetSoapResponse(requestBody);
            var storages = JsonConvert.DeserializeObject<IEnumerable<GoodStorage1C>>(line)
                ?? throw new Exception("Error of get 'storages' from 1c!");
            return storages;
        }

        public async Task<IEnumerable<GoodDataVersion1C>> GetGoodsDataVersions()
        {
            string requestBody = "<bag:GetGoodsDataVersions/>";
            string line = await GetSoapResponse(requestBody);
            var goodsDataVersions = JsonConvert.DeserializeObject<IEnumerable<GoodDataVersion1C>>(line)
                ?? throw new Exception("Error of get 'goodDataVersion' from 1c!");
            return goodsDataVersions;
        }

        public async Task<IEnumerable<Good1C>> GetGoods(IEnumerable<int> goodIds)
        {
            string ids = String.Join(',', goodIds);
            string requestBody = "<bag:GetGoods>"
                + $"<bag:ids>{ids}</bag:ids>"
                + "</bag:GetGoods>";

            string line = await GetSoapResponse(requestBody);
            var goods = JsonConvert.DeserializeObject<IEnumerable<Good1C>>(line)
                ?? throw new Exception("Error of get 'goods' from 1c!");
            return goods;
        }

        public async Task<IEnumerable<GoodRest1C>> GetRestOfGoods()
        {
            string requestBody = "<bag:GetRestOfGoods/>";
            string line = await GetSoapResponse(requestBody);
            var goodRests = JsonConvert.DeserializeObject<IEnumerable<GoodRest1C>>(line)
                ?? throw new Exception("Error of get 'restOfGoods' from 1c!");
            return goodRests;
        }

        public async Task<IEnumerable<GoodPriceType1C>> GetPriceTypes()
        {
            string requestBody = "<bag:GetPriceTypes/>";
            string line = await GetSoapResponse(requestBody);
            var goodPriceTypes = JsonConvert.DeserializeObject<IEnumerable<GoodPriceType1C>>(line)
                ?? throw new Exception("Error of get 'goodPriceTypes' from 1c!");
            return goodPriceTypes;
        }

        public async Task<IEnumerable<GoodPrice1C>> GetPrices()
        {
            string requestBody = "<bag:GetPrices/>";
            string line = await GetSoapResponse(requestBody);
            var goodPrices = JsonConvert.DeserializeObject<IEnumerable<GoodPrice1C>>(line)
                ?? throw new Exception("Error of get 'goodPrices' from 1c!");
            return goodPrices;
        }

        public async Task<string> GetImage(string goodId)
        {
            string data = "<bag:GetImage>"
                + $"<bag:id>{goodId}</bag:id>"
                + "</bag:GetImage>";

            return await GetSoapResponse(data);
        }

        private async Task<string> GetSoapResponse(string requestBody)
        {
            string requestData = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:bag=\"BagiraApp\">"
                + "<soapenv:Header/>"
                + "<soapenv:Body>"
                + requestBody
                + "</soapenv:Body>"
                + "</soapenv:Envelope>";

            StringContent content = new(requestData);

            HttpResponseMessage response = await _httpClient.PostAsync(_soapServiceUrl, content);
            var responseString = response.Content.ReadAsStringAsync().Result;

            int startIndex = responseString.IndexOf("<m:return");
            startIndex = responseString.IndexOf(">", startIndex) + 1;
            int endIndex = responseString.LastIndexOf("</m:return>");
            string result = responseString[startIndex..endIndex];
            return result;
        }
    }
}
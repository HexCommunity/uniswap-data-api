using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UniswapDataApi.Interfaces;
using UniswapDataApi.Models;

namespace UniswapDataApi.Functions
{
    public class GetUniswapOrderBook
    {
        private readonly HttpClient _client;
        private readonly IOrderBookFactory _orderBookFactory;
        private readonly int _cacheExpirationSecs;
        private readonly string _getUniswapDataUri;

        private static DateTime _lastUpdated = DateTime.MinValue;
        private static IDictionary<string, OrderBook> _orderBooks = new Dictionary<string, OrderBook>();

        public GetUniswapOrderBook(IHttpClientFactory httpClientFactory, IOrderBookFactory orderBookFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
            _orderBookFactory = orderBookFactory;
            _cacheExpirationSecs = int.TryParse(config["CacheExpirationSecs"], out var cacheExpSecs)
                ? cacheExpSecs
                : 30;
            _getUniswapDataUri = config["GetUniswapDataUri"];
            if (string.IsNullOrEmpty(_getUniswapDataUri))
                throw new ApplicationException("'GetUniswapDataUri' must be defined in your configuration");
        }

        [FunctionName("orderBook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route= "orderBook/{tokenSymbol}")] HttpRequest req,
            string tokenSymbol,
            ILogger log)
        {
            try
            {
                if (OrderBooksAreStale())
                    await UpdateOrderBooks();

                _orderBooks.TryGetValue(tokenSymbol.ToLower(), out var orderBook);
                return new OkObjectResult(orderBook);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("cmcOrderBook")]
        public async Task<IActionResult> CmcOrderBook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cmc/orderBook/{tokenSymbol}")] HttpRequest req,
            string tokenSymbol,
            ILogger log)
        {
            try
            {
                if (OrderBooksAreStale())
                    await UpdateOrderBooks();

                // Drop _ETH
                _orderBooks.TryGetValue(tokenSymbol.Replace("_ETH", string.Empty).ToLower(), out var orderBook);
                return new OkObjectResult(orderBook.ConvertToCmcFormat());
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary data for CMC: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        private async Task UpdateOrderBooks()
        {
            var response = await _client.GetAsync(_getUniswapDataUri);
            var dtoString = await response.Content.ReadAsStringAsync();
            var pairs = JsonConvert.DeserializeObject<IEnumerable<UniswapPair>>(dtoString);

            var orderBooks = new Dictionary<string, OrderBook>();
            foreach (var pair in pairs)
            {
                var tokenSymbol = pair.TokenSymbol();
                if (orderBooks.ContainsKey(tokenSymbol))
                    continue;

                orderBooks.Add(tokenSymbol, _orderBookFactory.CreateOrderBook(pair));
            }

            _orderBooks = orderBooks;
            _lastUpdated = DateTime.UtcNow;
        }

        private bool OrderBooksAreStale()
        {
            return DateTime.UtcNow.Subtract(_lastUpdated) > TimeSpan.FromSeconds(_cacheExpirationSecs);
        }
    }
}

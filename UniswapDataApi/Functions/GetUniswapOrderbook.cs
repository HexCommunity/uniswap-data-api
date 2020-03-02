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
using UniswapDataApi.Models;

namespace UniswapDataApi.Functions
{
    public class GetUniswapOrderBook
    {
        private readonly HttpClient _client;
        private readonly int _cacheExpirationSecs;
        private readonly string _getUniswapDataUri;

        private const string DecimalFormatter = "0.##########";

        private static DateTime _lastUpdated = DateTime.MinValue;
        private static IDictionary<string, OrderBook> _orderBooks = new Dictionary<string, OrderBook>();

        public GetUniswapOrderBook(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
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

                orderBooks.Add(tokenSymbol, PairToOrderBook(pair));
            }

            _orderBooks = orderBooks;
            _lastUpdated = DateTime.UtcNow;
        }

        private static OrderBook PairToOrderBook(UniswapPair pair)
        {
            var ethLiquidity = double.Parse(pair.EthLiquidity);
            var tokenLiquidity = double.Parse(pair.TokenLiquidity);

            return new OrderBook
            {
                Bids = GetBids(ethLiquidity, tokenLiquidity),
                Asks = GetAsks(tokenLiquidity, ethLiquidity)
            };
        }

        private static List<Ask> GetAsks(double tokenLiquidity, double ethLiquidity)
        {
            var tokenOrderSize = tokenLiquidity * 0.005;
            var tokenOutput = tokenOrderSize;
            var currentPriceEth = ethLiquidity / tokenLiquidity;

            var asks = new List<Ask>();
            while (tokenOutput <= tokenLiquidity)
            {
                //Avoid infinity
                if (tokenOutput.ToString(DecimalFormatter) == tokenLiquidity.ToString(DecimalFormatter))
                    tokenOutput -= tokenOrderSize * 0.01;

                // Buy ERC20 with ETH
                var numerator = tokenOutput * ethLiquidity;
                var denominator = (tokenLiquidity - tokenOutput);
                var ethInput = numerator / denominator + 1;
                var priceEth = ethInput / tokenOutput;

                var order = new Ask
                {
                    PriceEth = priceEth.ToString(DecimalFormatter),
                    TokenAmount = tokenOrderSize.ToString(DecimalFormatter),
                    EthValue = (tokenOrderSize * currentPriceEth).ToString(DecimalFormatter)
                };
                asks.Add(order);
                tokenOutput += tokenOrderSize;
            }
            return asks;
        }

        private static List<Bid> GetBids(double ethLiquidity, double tokenLiquidity)
        {
            var ethOrderSize = ethLiquidity * 0.005;
            var ethOutput = ethOrderSize;

            var bids = new List<Bid>();
            while (ethOutput <= ethLiquidity)
            {
                // Buy ETH with ERC20
                var numerator = ethOutput * tokenLiquidity;
                var denominator = (ethLiquidity - ethOutput);
                var tokenInput = numerator / denominator + 1;
                var priceEth = ethOutput / tokenInput;

                var order = new Bid
                {
                    PriceEth = priceEth.ToString(DecimalFormatter),
                    EthAmount = ethOrderSize.ToString(DecimalFormatter)
                };
                bids.Add(order);
                ethOutput += ethOrderSize;
            }
            return bids;
        }

        private bool OrderBooksAreStale()
        {
            return DateTime.UtcNow.Subtract(_lastUpdated) > TimeSpan.FromSeconds(_cacheExpirationSecs);
        }
    }
}

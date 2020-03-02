using System;
using System.Collections.Generic;
using System.Linq;
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
using UniswapDataApi.Models.DTOs;

namespace UniswapDataApi.Functions
{
    public class GetUniswapData
    {
        private readonly HttpClient _client;
        private readonly int _cacheExpirationSecs;
        private readonly string _blocklyticsApiKey;

        private const string DecimalFormatter = "0.##########";
        
        private static DateTime _lastUpdated = DateTime.MinValue;
        private static IEnumerable<UniswapPair> _pairs = new List<UniswapPair>();

        public GetUniswapData(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
            _cacheExpirationSecs = int.TryParse(config["CacheExpirationSecs"], out var cacheExpSecs) 
                ? cacheExpSecs
                : 30;
            _blocklyticsApiKey = config["BlocklyticsApiKey"];
            if (string.IsNullOrEmpty(_blocklyticsApiKey))
                throw new ApplicationException("'BlocklyticsApiKey' must be defined in your configuration");
        }

        [FunctionName("market")]
        public async Task<IActionResult> GetMarketData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            try
            {
                if (PairsAreStale())
                    await UpdatePairs();

                return new OkObjectResult(_pairs);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("marketForTokenSymbol")]
        public async Task<IActionResult> GetMarketForTokenSymbol(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="market/{tokenSymbol}")] HttpRequest req,
            string tokenSymbol,
            ILogger log)
        {
            try
            {
                if (PairsAreStale())
                    await UpdatePairs();

                var pair = _pairs
                    .ToList()
                    .FirstOrDefault(p => p.TokenSymbol().Equals(tokenSymbol, StringComparison.InvariantCultureIgnoreCase));
                return new OkObjectResult(pair);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary pair data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        private async Task UpdatePairs()
        {
            var response =
                await _client.GetAsync(
                    $"https://api.blocklytics.org/uniswap/v1/exchanges?key={_blocklyticsApiKey}");
            var dtoString = await response.Content.ReadAsStringAsync();
            var blocklyticsPairs = JsonConvert.DeserializeObject<List<BlocklyticsUniswapPairsDTO>>(dtoString);
            _pairs = blocklyticsPairs
                .Where(pair => pair.Price != null && pair.Price.ToString() != "0")
                .Select(pair => new UniswapPair
                {
                    Pair = $"{pair.TokenSymbol}/ETH",
                    Price = (1 / (double) pair.Price).ToString(DecimalFormatter),
                    EthLiquidity = pair.EthLiquidity.ToString(DecimalFormatter),
                    TokenLiquidity = pair.TokenLiquidity.ToString(DecimalFormatter),
                    Volume24HrEth = pair.EthVolume.ToString(DecimalFormatter)
                })
                .Where(IsActive);
            _lastUpdated = DateTime.UtcNow;
        }

        private bool PairsAreStale()
        {
            return DateTime.UtcNow.Subtract(_lastUpdated) > TimeSpan.FromSeconds(_cacheExpirationSecs);
        }

        private static bool IsActive(UniswapPair pair) => pair.Price != "0" && pair.EthLiquidity != "0" &&
                                                          pair.TokenLiquidity != "0" && pair.Volume24HrEth != "0";
    }
}

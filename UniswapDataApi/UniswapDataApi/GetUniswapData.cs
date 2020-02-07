using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UniswapDataApi.Models;
using UniswapDataApi.Models.DTOs;

namespace UniswapDataApi
{
    public class GetUniswapData
    {
        private readonly HttpClient _client;
        private readonly string _blocklyticsApiKey;

        public GetUniswapData(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _client = httpClientFactory.CreateClient();
            _blocklyticsApiKey = config["BlocklyticsApiKey"];
            if (string.IsNullOrEmpty(_blocklyticsApiKey))
                throw new ApplicationException("'BlocklyticsApiKey' must be defined in your configuration");
        }

        [FunctionName("market")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var response = await _client.GetAsync($"https://api.blocklytics.org/uniswap/v1/exchanges?key={_blocklyticsApiKey}");
                var dtoString = await response.Content.ReadAsStringAsync();
                var blocklyticsPairs = JsonConvert.DeserializeObject<List<BlocklyticsUniswapPairsDTO>>(dtoString);
                var pairs = blocklyticsPairs
                    .Where(bPair => bPair.Price != null && !bPair.EthVolume.Equals(0))
                    .Select(bPair => new UniswapPairSummary
                    {
                        Pair = $"{bPair.TokenSymbol}/ETH",
                        Price = ConvertDoubleToDecimal(1 / (double)bPair.Price),
                        EthLiquidity = ConvertDoubleToDecimal(bPair.EthLiquidity),
                        TokenLiquidity = ConvertDoubleToDecimal(bPair.TokenLiquidity),
                        Volume24HrEth = ConvertDoubleToDecimal(bPair.EthVolume)
                    });
                return new OkObjectResult(pairs);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while processing blocklytics <=> uniswap summary data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        private static decimal ConvertDoubleToDecimal(double value) => Math.Round(Convert.ToDecimal(value), 10, MidpointRounding.ToNegativeInfinity);
    }
}

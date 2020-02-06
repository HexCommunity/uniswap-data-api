using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UniswapDataApi.Models.DTOs;

namespace UniswapDataApi
{
    public class WriteUniswapData
    {
        private readonly HttpClient _client;

        public WriteUniswapData(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName("WriteUniswapData")]
        public void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var response = _client.GetAsync("https://api.coingecko.com/api/v3/exchanges/uniswap/tickers").Result;
            var dtoString = response.Content.ReadAsStringAsync().Result;
            var uniswapTickers = JsonConvert.DeserializeObject<CoinGeckoUniswapTickersDTO>(dtoString);
            //write to azure table for current and yesterday prices?
        }
    }
}

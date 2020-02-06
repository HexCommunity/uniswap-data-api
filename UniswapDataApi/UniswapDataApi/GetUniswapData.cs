using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UniswapDataApi
{
    public class GetUniswapData
    {
        private readonly HttpClient _client;

        public GetUniswapData(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName("hour24Market")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            //Read necessary 24hr price changes from *azure table*
            //Query for latest ticker data from coingecko (see write uniswap data)
            //Combine data into UniswapTickerSummaries and return as json
            //Provided sample https://www.coinsuper.com/v1/api/market/hour24Market
            return new OkObjectResult("just a stub");
        }
    }
}

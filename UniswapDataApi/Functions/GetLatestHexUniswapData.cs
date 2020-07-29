using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Web3;
using Newtonsoft.Json;
using UniswapDataApi.Models;
using UniswapDataApi.Models.DTOs;

namespace UniswapDataApi.Functions
{
    public class GetLatestHexUniswapData
    {
        private readonly HttpClient _client;

        private readonly int _cacheExpirationSecs;

        private const string DecimalFormatter = "0.#########";
        private const string HexUniswapV2Address = "0x55d5c232d921b9eaa6b37b5845e439acd04b4dba";

        private static DateTime _lastUpdated = DateTime.MinValue;
        private static HexPrice _price = new HexPrice();

        private readonly ContractHandler _contractHandler;

        public GetLatestHexUniswapData(IHttpClientFactory httpClientFactory, IWeb3 web3)
        {
            _client = httpClientFactory.CreateClient();
            _contractHandler = web3.Eth.GetContractHandler(HexUniswapV2Address);
            _cacheExpirationSecs = 15;
        }

        [FunctionName("hexPrice")]
        public async Task<IActionResult> GetMarketData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            try
            {
                if (PriceIsStale())
                    await UpdatePrice();

                return new OkObjectResult(_price);
            }
            catch (Exception e)
            {
                log.LogCritical(e, $"Caught an exception while generating HEX price data: {e.Message}");
                return new InternalServerErrorResult();
            }
        }

        private async Task UpdatePrice()
        {
            var uniswapPrice = await GetHexUniswapV2Price();
            var ethBtcPrice = await GetEthBtcPrice();
            var ethUsdPrice = await GetEthUsdPrice();

            _price = new HexPrice
            {
                LastUpdated = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture),
                HexEth = uniswapPrice.ToString(DecimalFormatter),
                HexBtc = (uniswapPrice * ethBtcPrice).ToString(DecimalFormatter),
                HexUsd = (uniswapPrice * ethUsdPrice).ToString(DecimalFormatter)
            };

            _lastUpdated = DateTime.UtcNow;
        }

        private async Task<decimal> GetHexUniswapV2Price()
        { 
            var getReservesOutputDTO = await _contractHandler.QueryDeserializingToObjectAsync<GetReservesFunction, GetReservesOutputDTO>();

            var hexLiquidity = Web3.Convert.FromWei(getReservesOutputDTO.Reserve0, 8);
            var ethLiquidity = Web3.Convert.FromWei(getReservesOutputDTO.Reserve1, 18);

            return ethLiquidity / hexLiquidity;
        }

        private async Task<decimal> GetEthBtcPrice()
        {
            var response = await _client.GetAsync("https://api.binance.com/api/v3/ticker/price?symbol=ETHBTC");
            var ethBtc = JsonConvert.DeserializeObject<BinancePriceDTO>(await response.Content.ReadAsStringAsync());
            return decimal.Parse(ethBtc.Price);
        }

        private async Task<decimal> GetEthUsdPrice()
        {
            var response = await _client.GetAsync("https://api.coinbase.com/v2/prices/ETH-USD/spot");
            var ethUsd = JsonConvert.DeserializeObject<CoinbasePriceDTO>(await response.Content.ReadAsStringAsync());
            return decimal.Parse(ethUsd.Data.Amount);
        }

        private bool PriceIsStale()
        {
            return DateTime.UtcNow.Subtract(_lastUpdated) > TimeSpan.FromSeconds(_cacheExpirationSecs);
        }

        [Function("getReserves", typeof(GetReservesOutputDTO))]
        private class GetReservesFunction : FunctionMessage
        {
        }

        [FunctionOutput]
        private class GetReservesOutputDTO : IFunctionOutputDTO
        {
            [Parameter("uint112", "_reserve0", 1)]
            public virtual BigInteger Reserve0 { get; set; }
            [Parameter("uint112", "_reserve1", 2)]
            public virtual BigInteger Reserve1 { get; set; }
            [Parameter("uint32", "_blockTimestampLast", 3)]
            public virtual uint BlockTimestampLast { get; set; }
        }
    }
}

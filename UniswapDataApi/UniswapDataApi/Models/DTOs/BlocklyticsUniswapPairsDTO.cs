namespace UniswapDataApi.Models.DTOs
{
    using System;
    using Newtonsoft.Json;

    public class BlocklyticsUniswapPairsDTO
    {
        [JsonProperty("ethLiquidity")]
        public double EthLiquidity { get; set; }

        [JsonProperty("ethVolume")]
        public double EthVolume { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("factory")]
        public string Factory { get; set; }

        [JsonProperty("price")]
        public double? Price { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tokenLiquidity")]
        public double TokenLiquidity { get; set; }

        [JsonProperty("tokenName")]
        public string TokenName { get; set; }

        [JsonProperty("tokenSymbol")]
        public string TokenSymbol { get; set; }

        [JsonProperty("tokenVolume")]
        public double TokenVolume { get; set; }
    }
}

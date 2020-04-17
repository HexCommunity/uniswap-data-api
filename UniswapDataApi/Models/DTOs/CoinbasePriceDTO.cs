using Newtonsoft.Json;

namespace UniswapDataApi.Models.DTOs
{
    public class CoinbasePriceDTO
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
    }
}
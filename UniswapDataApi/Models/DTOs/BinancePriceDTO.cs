using Newtonsoft.Json;

namespace UniswapDataApi.Models.DTOs
{
    public class BinancePriceDTO
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }
}
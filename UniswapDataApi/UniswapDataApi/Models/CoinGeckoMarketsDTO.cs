namespace UniswapDataApi.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class CoinGeckoMarketsDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }

        [JsonProperty("current_price")]
        public double CurrentPrice { get; set; }

        [JsonProperty("market_cap")]
        public long MarketCap { get; set; }

        [JsonProperty("market_cap_rank")]
        public long MarketCapRank { get; set; }

        [JsonProperty("total_volume")]
        public double TotalVolume { get; set; }

        [JsonProperty("high_24h")]
        public double High24H { get; set; }

        [JsonProperty("low_24h")]
        public double Low24H { get; set; }

        [JsonProperty("price_change_24h")]
        public double PriceChange24H { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public double PriceChangePercentage24H { get; set; }

        [JsonProperty("market_cap_change_24h")]
        public double MarketCapChange24H { get; set; }

        [JsonProperty("market_cap_change_percentage_24h")]
        public double MarketCapChangePercentage24H { get; set; }

        [JsonProperty("circulating_supply")]
        public double CirculatingSupply { get; set; }

        [JsonProperty("total_supply")]
        public long? TotalSupply { get; set; }

        [JsonProperty("ath")]
        public double Ath { get; set; }

        [JsonProperty("ath_change_percentage")]
        public double AthChangePercentage { get; set; }

        [JsonProperty("ath_date")]
        public DateTimeOffset AthDate { get; set; }

        [JsonProperty("roi")]
        public Roi Roi { get; set; }

        [JsonProperty("last_updated")]
        public DateTimeOffset LastUpdated { get; set; }
    }

    public partial class Roi
    {
        [JsonProperty("times")]
        public double Times { get; set; }

        [JsonProperty("currency")]
        public Currency Currency { get; set; }

        [JsonProperty("percentage")]
        public double Percentage { get; set; }
    }

    public enum Currency { Btc, Eth, Usd };

    public partial class CoinGeckoMarketsDTO
    {
        public static List<CoinGeckoMarketsDTO> FromJson(string json) => JsonConvert.DeserializeObject<List<CoinGeckoMarketsDTO>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<CoinGeckoMarketsDTO> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CurrencyConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class CurrencyConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Currency) || t == typeof(Currency?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "btc":
                    return Currency.Btc;
                case "eth":
                    return Currency.Eth;
                case "usd":
                    return Currency.Usd;
            }
            throw new Exception("Cannot unmarshal type Currency");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Currency)untypedValue;
            switch (value)
            {
                case Currency.Btc:
                    serializer.Serialize(writer, "btc");
                    return;
                case Currency.Eth:
                    serializer.Serialize(writer, "eth");
                    return;
                case Currency.Usd:
                    serializer.Serialize(writer, "usd");
                    return;
            }
            throw new Exception("Cannot marshal type Currency");
        }

        public static readonly CurrencyConverter Singleton = new CurrencyConverter();
    }
}
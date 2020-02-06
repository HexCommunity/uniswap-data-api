using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniswapDataApi.Models.DTOs
{
    public partial class CoinGeckoUniswapTickersDTO
    {
        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("tickers")]
        public List<Ticker> Tickers { get; set; }
    }

    public class Ticker
    {
        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("target")]
        public Target Target { get; set; }

        [JsonProperty("market")]
        public Market Market { get; set; }

        [JsonProperty("last")]
        public double Last { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }

        [JsonProperty("converted_last")]
        public Dictionary<string, double> ConvertedLast { get; set; }

        [JsonProperty("converted_volume")]
        public Dictionary<string, double> ConvertedVolume { get; set; }

        [JsonProperty("trust_score")]
        public object TrustScore { get; set; }

        [JsonProperty("bid_ask_spread_percentage")]
        public object BidAskSpreadPercentage { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("last_traded_at")]
        public DateTimeOffset LastTradedAt { get; set; }

        [JsonProperty("last_fetch_at")]
        public DateTimeOffset LastFetchAt { get; set; }

        [JsonProperty("is_anomaly")]
        public bool IsAnomaly { get; set; }

        [JsonProperty("is_stale")]
        public bool IsStale { get; set; }

        [JsonProperty("trade_url")]
        public object TradeUrl { get; set; }

        [JsonProperty("coin_id")]
        public string CoinId { get; set; }

        [JsonProperty("target_coin_id")]
        public TargetCoinId TargetCoinId { get; set; }
    }

    public class Market
    {
        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("identifier")]
        public Identifier Identifier { get; set; }

        [JsonProperty("has_trading_incentive")]
        public bool HasTradingIncentive { get; set; }
    }

    public enum Name { Uniswap };

    public enum Identifier { Uniswap };

    public enum Target { Eth };

    public enum TargetCoinId { Ethereum };

    public partial class CoinGeckoUniswapTickersDTO
    {
        public static CoinGeckoUniswapTickersDTO FromJson(string json) => JsonConvert.DeserializeObject<CoinGeckoUniswapTickersDTO>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this CoinGeckoUniswapTickersDTO self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                NameConverter.Singleton,
                IdentifierConverter.Singleton,
                TargetConverter.Singleton,
                TargetCoinIdConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "Uniswap")
            {
                return Name.Uniswap;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            if (value == Name.Uniswap)
            {
                serializer.Serialize(writer, "Uniswap");
                return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }

    internal class IdentifierConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Identifier) || t == typeof(Identifier?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "uniswap")
            {
                return Identifier.Uniswap;
            }
            throw new Exception("Cannot unmarshal type Identifier");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Identifier)untypedValue;
            if (value == Identifier.Uniswap)
            {
                serializer.Serialize(writer, "uniswap");
                return;
            }
            throw new Exception("Cannot marshal type Identifier");
        }

        public static readonly IdentifierConverter Singleton = new IdentifierConverter();
    }

    internal class TargetConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Target) || t == typeof(Target?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "ETH")
            {
                return Target.Eth;
            }
            throw new Exception("Cannot unmarshal type Target");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Target)untypedValue;
            if (value == Target.Eth)
            {
                serializer.Serialize(writer, "ETH");
                return;
            }
            throw new Exception("Cannot marshal type Target");
        }

        public static readonly TargetConverter Singleton = new TargetConverter();
    }

    internal class TargetCoinIdConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TargetCoinId) || t == typeof(TargetCoinId?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "ethereum")
            {
                return TargetCoinId.Ethereum;
            }
            throw new Exception("Cannot unmarshal type TargetCoinId");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TargetCoinId)untypedValue;
            if (value == TargetCoinId.Ethereum)
            {
                serializer.Serialize(writer, "ethereum");
                return;
            }
            throw new Exception("Cannot marshal type TargetCoinId");
        }

        public static readonly TargetCoinIdConverter Singleton = new TargetCoinIdConverter();
    }
}

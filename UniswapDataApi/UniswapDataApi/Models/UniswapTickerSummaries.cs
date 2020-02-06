namespace UniswapDataApi.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class UniswapTickerSummaries
    {
        [JsonProperty("code")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Code { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, TickerSummary> Tickers { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public class TickerSummary
    {
        [JsonProperty("baseVolume")]
        public string BaseVolume { get; set; }

        [JsonProperty("high24hr")]
        public string High24Hr { get; set; }

        [JsonProperty("highestBid")]
        public string HighestBid { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("isFrozen")]
        public long IsFrozen { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("low24Hr")]
        public string Low24Hr { get; set; }

        [JsonProperty("lowestAsk")]
        public string LowestAsk { get; set; }

        [JsonProperty("percentChange")]
        public string PercentChange { get; set; }

        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; }
    }

    public partial class UniswapTickerSummaries
    {
        public static UniswapTickerSummaries FromJson(string json) => JsonConvert.DeserializeObject<UniswapTickerSummaries>(json, UniswapDataApi.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this UniswapTickerSummaries self) => JsonConvert.SerializeObject(self, UniswapDataApi.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}

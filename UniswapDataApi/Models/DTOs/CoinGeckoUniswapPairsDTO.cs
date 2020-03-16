using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UniswapDataApi.Models.DTOs
{
    public class CoinGeckoUniswapTickersDTO
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

    public enum Name { Uniswap }

    public enum Identifier { Uniswap }

    public enum Target { Eth }

    public enum TargetCoinId { Ethereum }
}

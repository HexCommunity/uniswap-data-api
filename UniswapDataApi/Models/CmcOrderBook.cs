using System.Collections.Generic;
using System.Linq;

namespace UniswapDataApi.Models
{
    public class CmcOrderBook
    {
        //Formatting guideline
        //https://docs.google.com/document/d/1S4urpzUnO2t7DmS_1dc4EL4tgnnbTObPYXvDeBnukCg/edit#
        public long Timestamp { get; set; }
        public List<string[]> Bids { get; set; } = new List<string[]>();
        public List<string[]> Asks { get; set; } = new List<string[]>();
    }

    public static class CmcOrderBookExtensions
    {
        public static CmcOrderBook ConvertToCmcFormat(this OrderBook orderBook)
        {
            return new CmcOrderBook
            {
                Timestamp = orderBook.Timestamp,
                Bids = orderBook.Bids.Select(bid => new[] { bid.PriceEth, bid.EthAmount }).ToList(),
                Asks = orderBook.Asks.Select(ask => new[] { ask.PriceEth, ask.EthValue }).ToList()
            };
        }
    }
}
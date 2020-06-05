using System.Collections.Generic;

namespace UniswapDataApi.Models
{
    public class OrderBook
    {
        public long Timestamp { get; set; }
        public List<Bid> Bids { get; set; } = new List<Bid>();
        public List<Ask> Asks { get; set; } = new List<Ask>();
    }

    public class Bid
    {
        public string PriceEth { get; set; }
        public string EthAmount { get; set; }
    }

    public class Ask
    {
        public string PriceEth { get; set; }
        public string TokenAmount { get; set; }
        public string EthValue { get; set; }
    }
}
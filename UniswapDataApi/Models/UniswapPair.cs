using UniswapDataApi.Interfaces;

namespace UniswapDataApi.Models
{
    public class UniswapPair : IPairLiquidity
    {
        public long Timestamp { get; set; }
        public string Pair { get; set; }
        public string Price { get; set; }
        public string EthLiquidity { get; set; }
        public string TokenLiquidity { get; set; }
        public string Volume24HrEth { get; set; }
    }

    public static class UniswapPairExtensions
    {
        public static string TokenSymbol(this UniswapPair pair) => pair.Pair.Substring(0, pair.Pair.LastIndexOf("/ETH")).ToLower();
    }
}

namespace UniswapDataApi.Models
{
    public class UniswapPairSummary
    {
        public string Pair { get; set; }
        public string Price { get; set; }
        public string EthLiquidity { get; set; }
        public string TokenLiquidity { get; set; }
        public string Volume24HrEth { get; set; }
    }
}

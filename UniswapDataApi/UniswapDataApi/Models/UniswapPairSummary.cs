namespace UniswapDataApi.Models
{
    public class UniswapPairSummary
    {
        public string Pair { get; set; }
        public decimal Price { get; set; }
        public decimal EthLiquidity { get; set; }
        public decimal TokenLiquidity { get; set; }
        public decimal Volume24HrEth { get; set; }
    }
}

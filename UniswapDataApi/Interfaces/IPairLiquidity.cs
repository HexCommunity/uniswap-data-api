namespace UniswapDataApi.Interfaces
{
    public interface IPairLiquidity
    {
        long Timestamp { get; set; }
        string EthLiquidity { get; set; }
        string TokenLiquidity { get; set; }
    }
}

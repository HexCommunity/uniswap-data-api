namespace UniswapDataApi.Interfaces
{
    public interface IPairLiquidity
    {
        string EthLiquidity { get; set; }
        string TokenLiquidity { get; set; }
    }
}

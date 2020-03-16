using UniswapDataApi.Models;

namespace UniswapDataApi.Interfaces
{
    public interface IOrderBookFactory
    {
        OrderBook CreateOrderBook(IPairLiquidity pair);
    }
}

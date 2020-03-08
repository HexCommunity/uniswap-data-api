using System.Collections.Generic;
using UniswapDataApi.Interfaces;
using UniswapDataApi.Models;

namespace UniswapDataApi.Services
{
    public class OrderBookFactory : IOrderBookFactory
    {
        private const string DecimalFormatter = "0.##########";
        private readonly int _numberOfOrders;

        public OrderBookFactory(int numberOrders)
        {
            _numberOfOrders = numberOrders;
        }

        public OrderBook CreateOrderBook(IPairLiquidity pair)
        {
            var ethLiquidity = double.Parse(pair.EthLiquidity);
            var tokenLiquidity = double.Parse(pair.TokenLiquidity);

            return new OrderBook
            {
                Bids = GetBids(ethLiquidity, tokenLiquidity),
                Asks = GetAsks(tokenLiquidity, ethLiquidity)
            };
        }

        public List<Ask> GetAsks(double outputReserve, double inputReserve)
        {
            var tokenOrderSize = outputReserve / _numberOfOrders;
            var outputAmount = tokenOrderSize;
            var currentPriceEth = inputReserve / outputReserve;

            var asks = new List<Ask>();
            for(var i = 1; i <= _numberOfOrders; i++)
            {
                //Avoid infinity
                if (i == _numberOfOrders)
                    outputAmount -= tokenOrderSize * 0.001;

                // Buy ERC20 with ETH
                var numerator = outputAmount * inputReserve;
                var denominator = (outputReserve - outputAmount);
                var inputAmount = numerator / denominator;

                var priceEth = 1d / (outputAmount / inputAmount);

                var order = new Ask
                {
                    PriceEth = priceEth.ToString(DecimalFormatter),
                    TokenAmount = tokenOrderSize.ToString(DecimalFormatter),
                    EthValue = (tokenOrderSize * currentPriceEth).ToString(DecimalFormatter)
                };
                asks.Add(order);
                outputAmount += tokenOrderSize;
            }
            return asks;
        }

        public List<Bid> GetBids(double outputReserve, double inputReserve)
        {
            var ethOrderSize = outputReserve / _numberOfOrders;
            var outputAmount = ethOrderSize;

            var bids = new List<Bid>();
            for (var i = 1; i <= _numberOfOrders; i++)
            {
                //Avoid zero
                if (i == _numberOfOrders)
                    outputAmount -= ethOrderSize * 0.001;

                // Buy ETH with ERC20
                var numerator = outputAmount * inputReserve;
                var denominator = (outputReserve - outputAmount);
                var tokenInput = numerator / denominator;

                var priceEth = outputAmount / tokenInput;

                var order = new Bid
                {
                    PriceEth = priceEth.ToString(DecimalFormatter),
                    EthAmount = ethOrderSize.ToString(DecimalFormatter)
                };
                bids.Add(order);
                outputAmount += ethOrderSize;
            }
            return bids;
        }
    }
}
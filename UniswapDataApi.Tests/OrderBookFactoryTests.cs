using System;
using NUnit.Framework;
using UniswapDataApi.Models;
using UniswapDataApi.Services;

namespace UniswapDataApi.Tests
{
    public class OrderBookFactoryTests
    {
        private OrderBookFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new OrderBookFactory(10);
        }

        [Test]
        public void Test1()
        {
            // Arrange
            var pair = new UniswapPair
            {
                EthLiquidity = "49.82815291",
                TokenLiquidity = "48745.233764732"
            };
            var orderBook = _factory.CreateOrderBook(pair);

            // Act
            Console.WriteLine("EthLiquidity: 10ETH");
            Console.WriteLine("TokenLiquidity: 100TOKEN");

            Console.WriteLine("Asks:");
            orderBook.Asks.Reverse();
            orderBook.Asks.ForEach(ask => Console.WriteLine($"{ask.PriceEth}\t\t{ask.TokenAmount}TOKEN"));

            Console.WriteLine("Bids:");
            orderBook.Bids.ForEach(bid => Console.WriteLine($"{bid.PriceEth}\t\t{bid.EthAmount}ETH"));

            // Assert
        }
    }
}
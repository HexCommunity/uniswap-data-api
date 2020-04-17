using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Nethereum.Web3;
using UniswapDataApi.Interfaces;
using UniswapDataApi.Services;

[assembly: FunctionsStartup(typeof(UniswapDataApi.Startup))]

namespace UniswapDataApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var infuraApiKey = Environment.GetEnvironmentVariable("InfuraApiKey");
            if (string.IsNullOrEmpty(infuraApiKey))
                throw new ApplicationException("'InfuraApiKey' must be defined in your configuration");

            builder.Services.AddTransient<IOrderBookFactory>(_ => new OrderBookFactory(200));
            builder.Services.AddTransient<IWeb3>(_ => new Web3($"https://mainnet.infura.io/v3/{infuraApiKey}"));
            builder.Services.AddHttpClient();
        }
    }
}
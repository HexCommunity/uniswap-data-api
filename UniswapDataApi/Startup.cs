using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UniswapDataApi.Interfaces;
using UniswapDataApi.Services;

[assembly: FunctionsStartup(typeof(UniswapDataApi.Startup))]

namespace UniswapDataApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IOrderBookFactory>(_ => new OrderBookFactory(200));
            builder.Services.AddHttpClient();
        }
    }
}
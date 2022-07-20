using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeterPedia.Data.Implementations;

[assembly: FunctionsStartup(typeof(Reader.Api.Startup))]

namespace Reader.Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        IConfiguration configuration = builder.GetContext().Configuration;

        builder.Services.AddOptions<CosmosOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration.GetSection("Cosmos").Bind(settings));

        builder.Services.AddSingleton<IRepository, CosmosRepository>();
    }
}

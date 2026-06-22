using Alco.Functions.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration(configuration =>
    {
        configuration.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<ContractIdParser>();
        services.AddSingleton<BlobFileNameResolver>();
        services.AddSingleton<ContractSkillProcessor>();
        services.AddSingleton<SearchClientFactory>();
        services.AddSingleton<BlobServiceClientFactory>();
    })
    .Build();

host.Run();

using Application.Ports;
using Data.CosmosDb.Adapters;
using Microsoft.Azure.Cosmos;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection s) => s
        .AddSingleton<IDataPersister, DataPersister>()
        .AddSingleton(sp => new CosmosClient(
            "https://localhost:8081/", 
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));
}

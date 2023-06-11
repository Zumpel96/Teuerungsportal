using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Unimarkt;

using System;

public static class UnimarktDataImportCategory1
{
    [FunctionName("UnimarktDataImportCategory1")]
    public static async Task Run(
        [TimerTrigger("0 15 6/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var dataExtractor = new UnimarktDataExtractor("obst-gemuese", dbProducts, dbPrices);
        try
        {
            await dataExtractor.Run();
        }
        catch (Exception e)
        {
            log.LogInformation($"{e}");
        }
    }
}
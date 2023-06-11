using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Unimarkt;

using System;

public static class UnimarktDataImportCategory5
{
    [FunctionName("UnimarktDataImportCategory5")]
    public static async Task Run(
        [TimerTrigger("0 55 6/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var dataExtractor = new UnimarktDataExtractor("getraenke", dbProducts, dbPrices);
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
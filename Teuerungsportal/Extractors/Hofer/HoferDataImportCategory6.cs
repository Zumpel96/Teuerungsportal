using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Hofer;

using System;

public static class HoferDataImportCategory6
{
    [FunctionName("HoferDataImportCategory6")]
    public static async Task Run(
        [TimerTrigger("5 0 6/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");

        var dataExtractor = new HoferDataExtractor("sußes-salziges", dbProducts, dbPrices);
        try
        {
            await dataExtractor.Run(log);
        }
        catch (Exception e)
        {
            log.LogInformation($"{e}");
        }
    }
}
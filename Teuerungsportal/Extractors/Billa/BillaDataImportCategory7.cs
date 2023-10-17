using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Billa;

using System;
using global::Extractors.General;

public static class BillaDataImportCategory7
{
    [FunctionName("BillaDataImportCategory7")]
    public static async Task Run(
        [TimerTrigger("0 20 5/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<ProductDto> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<PriceDto> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");

        for (var i = 0; i < 10; i++)
        {
            log.LogInformation($"Starting page {i}");
            var dataExtractor = new BillaDataExtractor("suesses-und-salziges-14057", i, dbProducts, dbPrices);
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
}
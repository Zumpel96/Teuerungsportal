using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Spar;

using System;
using global::Extractors.General;

public static class SparDataImportCategory14
{
    [FunctionName("SparDataImportCategory14")]
    public static async Task Run(
        [TimerTrigger("0 30 7/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<ProductDto> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<PriceDto> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var dataExtractor = new SparDataExtractor("F14", dbProducts, dbPrices);
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
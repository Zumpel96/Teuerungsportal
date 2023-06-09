using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Econtrol;

using System;
using global::Extractors.General;

public static class EcontrolDataImport6900
{
    [FunctionName("EcontrolDataImport6900")]
    public static async Task Run(
        [TimerTrigger("0 50 0/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<ProductDto> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<PriceDto> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var dataExtractor = new EcontrolDataExtractor("6900", dbProducts, dbPrices);
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
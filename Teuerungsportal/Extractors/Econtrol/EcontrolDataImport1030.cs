using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Econtrol;

using System;

public static class EcontrolDataImport1030
{
    [FunctionName("EcontrolDataImport1030")]
    public static async Task Run(
        [TimerTrigger("0 0 6/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var dataExtractor = new EcontrolDataExtractor("1030", dbProducts, dbPrices);
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
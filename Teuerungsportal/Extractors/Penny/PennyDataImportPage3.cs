using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Penny;

using System;

public static class PennyDataImportPage3
{
    [FunctionName("PennyDataImportPage3")]
    public static async Task Run(
        [TimerTrigger("0 10 1/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var url = $"https://www.penny.at/api/products?page=3&pageSize=500";
        log.LogInformation("url: {Url}", url);

        var dataExtractor = new PennyDataExtractor(url, dbProducts, dbPrices);
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
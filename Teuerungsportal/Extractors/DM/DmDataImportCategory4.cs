using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Dm;

using System;
using Api.Extractors.DM;

public static class DmDataImportCategory4
{
    [FunctionName("DmDataImportCategory4")]
    public static async Task Run(
        [TimerTrigger("0 10 7/12 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var url = $"https://product-search.services.dmtech.com/at/search/crawl?pageSize=10000&allCategories.id=040000";
        log.LogInformation("url: {Url}", url);

        var dataExtractor = new DmDataExtractor(url, dbProducts, dbPrices);
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
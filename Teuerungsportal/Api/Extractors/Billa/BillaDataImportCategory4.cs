using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Api.Extractors.Billa;

public static class BillaDataImportCategory4
{
    [FunctionName("BillaDataImportCategory4")]
    public static async Task Run(
        [TimerTrigger("0 10 */2 * * *")] TimerInfo myTimer,
        [Sql(commandText: "dbo.product", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Product> dbProducts,
        [Sql(commandText: "dbo.price", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Price> dbPrices,
        ILogger log)
    {
        log.LogInformation("Request received - Starting");
        var url = $"https://shop.billa.at/api/search/full?category=B2-4&includeSort[]=rank&sort=rank&pageSize=1000000";
        log.LogInformation("url: {Url}", url);

        var dataExtractor = new BillaDataExtractor(url, dbProducts, dbPrices);
        await dataExtractor.Run();
    }
}
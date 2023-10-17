using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api.v2;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetProductUrl
{
    [FunctionName("GetProductUrlV2")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/product/url/{productId}")] HttpRequest req,
        [Sql(
                commandText: @"
                            SELECT 
							  CONCAT([s].[baseUrl], [p].[url]) AS [url]
                            FROM 
                              [dbo].[product] [p]
							  JOIN [dbo].[store] [s] ON [s].[id] = [p].[storeId]
                            WHERE [s].[hidden] = 0 
                              AND [p].[id] = @productId;",
                parameters: "@productId={productId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> urls)
    {
        var urlList = urls.ToList();
        if (!urlList.Any())
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(urlList.First().Url);
    }
}
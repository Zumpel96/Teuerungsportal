using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetStore
{
    [FunctionName("GetStore")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeName}")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [id], 
                                  [name], 
                                  [baseUrl] 
                                FROM 
                                  [dbo].[store] 
                                WHERE 
                                  LOWER([name]) = LOWER(@storeName)
                                AND
                                  [hidden] = 0;",
                parameters: "@storeName={storeName}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<StoreDbo> stores)
    {
        var storeList = stores.ToList();
        if (!storeList.Any())
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(new Store(storeList.First()));
    }
}
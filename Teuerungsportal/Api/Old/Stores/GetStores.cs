using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetStores
{
    [FunctionName("GetStores")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [id], 
                                  [name], 
                                  [baseUrl]
                                FROM 
                                  [dbo].[store]
                                WHERE
                                  [hidden] = 0;",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<StoreDbo> stores)
    {
        return new OkObjectResult(stores.Select(dbo => new Store(dbo)));
    }
}
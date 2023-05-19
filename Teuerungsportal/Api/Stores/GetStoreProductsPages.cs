using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetStoreProductsPages
{
    [FunctionName("GetStoreProductsPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/products")] HttpRequest req,
        [Sql(
              commandText: @"
                                SELECT 
                                  COUNT([p].[id]) AS [count]
                                FROM 
                                  [dbo].[product] [p]
                                WHERE 
                                  LOWER([p].[storeId]) = LOWER(@storeId);",
                parameters: "@storeId={storeId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
      var countList = count.ToList();
      return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}
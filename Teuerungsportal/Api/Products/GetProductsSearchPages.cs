using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;

public static class GetProductsSearchPages
{
    [FunctionName("GetProductsSearchPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/search/{searchString}")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  COUNT([p].[id]) AS [count]
                                FROM 
                                  [dbo].[product] [p] 
                                WHERE 
                                  LOWER([p] .[name]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37))) OR LOWER([p] .[brand]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37))) OR LOWER([p].[articleNumber]) LIKE LOWER(CONCAT(CHAR(37), @searchString, CHAR(37)));",
                parameters: "@searchString={searchString}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CountDbo> count)
    {
      var countList = count.ToList();
      return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}
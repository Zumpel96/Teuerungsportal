using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetProductsWithoutCategoryPages
{
    public class ProductCountDbo
    {
        public int Count { get; set; }
    }

    [FunctionName("GetProductsWithoutCategoryPages")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/noCategory")] HttpRequest req,
        [Sql(
                commandText: @"SELECT COUNT(*) AS [count] FROM [dbo].[product] WHERE categoryId IS NULL;",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductCountDbo> count)
    {
        var countList = count.ToList();
        return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult((int)Math.Ceiling((double)countList.First().Count / 25));
    }
}
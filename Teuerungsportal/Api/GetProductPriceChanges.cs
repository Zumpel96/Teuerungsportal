using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;

public static class GetProductPriceChanges
{
    public class Price
    {
        public double Value { get; set; }

        public DateTime TimeStamp { get; set; }
    }

    [FunctionName("GetPriceChangesForProduct")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "prices/product/{productId}")] HttpRequest req,
        [Sql(
                commandText: @"SELECT [value], [timeStamp]
				 				 FROM [dbo].[price]
				 				 WHERE LOWER(productId) = LOWER(@productId);",
                parameters: "@productId={productId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<Price> prices)
    {
        return new OkObjectResult(prices);
    }
}
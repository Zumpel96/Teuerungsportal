using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using Shared.DatabaseObjects;

public static class UpdateProductCategory
{
    [FunctionName("UpdateProductCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products/{productId}/category/{categoryId}")] HttpRequest req,
        [Sql(
                commandText: @"
                                UPDATE 
                                  [dbo].[product] 
                                SET 
                                  [categoryId] = LOWER(@categoryId) 
                                WHERE 
                                  LOWER([id]) = LOWER(@productId) 
                                  AND [categoryId] IS NULL;",
                parameters: "@productId={productId},@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkResult();
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;

public static class UpdateProductCategory
{
    public class Product
    {
        public Guid Id { get; set; }
    }
    
    [FunctionName("UpdateProductCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "product/categories/update/{productId}/{categoryId}")] HttpRequest req,
        [Sql(
                commandText: @"UPDATE [dbo].[product]
                                SET categoryId = LOWER(@categoryId)
                                WHERE LOWER(id) = LOWER(@productId) AND categoryId IS NULL;",
                parameters: "@productId={productId},@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<Product> products)
    {
        return new OkResult();
    }
}
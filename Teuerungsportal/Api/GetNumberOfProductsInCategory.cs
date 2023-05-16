using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetNumberOfProductsInCategory
{
    public class ProductDbo
    {
        public int TotalNumberOfProducts { get; set; }
    }

    [FunctionName("GetNumberOfProductsInCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "category/{categoryId}/products/number")] HttpRequest req,
        [Sql(
                commandText: @"WITH category_tree AS (
                                  SELECT id, parentId
                                  FROM category
                                  WHERE id = LOWER(@categoryId)
                                  
                                  UNION ALL
                                  
                                  SELECT c.id, c.parentId
                                  FROM category c
                                  INNER JOIN category_tree ct ON ct.id = c.parentId
                                )
                                SELECT SUM(product_counts.numberOfProducts) AS totalNumberOfProducts
                                FROM (
                                  SELECT COUNT(p.id) AS numberOfProducts
                                  FROM category_tree ct
                                  INNER JOIN category c ON ct.id = c.id
                                  LEFT JOIN product p ON c.id = p.categoryId
                                  GROUP BY c.id, c.name
                                ) AS product_counts;",
                parameters: "@categoryId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> count)
    {
        var countList = count.ToList();
        return !countList.Any() ? new OkObjectResult(0) : new OkObjectResult(countList.First().TotalNumberOfProducts);
    }
}
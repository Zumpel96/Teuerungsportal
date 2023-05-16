using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetCategorySubCategory
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    [FunctionName("GetCategorySubCategories")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "category/{categoryId}/categories")] HttpRequest req,
        [Sql(
                commandText: @"SELECT id, name FROM [dbo].[category] WHERE LOWER(parentId) = LOWER(@parentId)",
                parameters: "@parentId={categoryId}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<Category> categories)
    {
        return new OkObjectResult(categories);
    }
}
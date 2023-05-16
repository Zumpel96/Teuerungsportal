using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetCategory
{
    public class CategoryDbo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public string ParentCategoryName { get; set; }
    }

    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<Category> ParentCategories { get; set; }
    }

    [FunctionName("GetSpecificCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "category/{categoryName}")] HttpRequest req,
        [Sql(
                    commandText: @"SELECT c.id, c.name, p.id AS 'parentCategoryId', p.name AS 'parentCategoryName'
				 				     FROM [dbo].[category] c 
				 				     LEFT JOIN [dbo].[category] p ON c.parentId = p.Id 
				 				     WHERE LOWER(c.name) = LOWER(@categoryName)",
                parameters: "@categoryName={categoryName}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<CategoryDbo> categories)
    {
        var categoryList = categories.ToList();
        if (!categoryList.Any())
        {
            return new NotFoundResult();
        }

        var foundCategory = categoryList.First();
        return new OkObjectResult(
                                  new Category()
                                  {
                                      Id = foundCategory.Id,
                                      Name = foundCategory.Name,
                                      ParentCategories =
                                      foundCategory.ParentCategoryId == null
                                      ? new List<Category>()
                                      : new List<Category>()
                                        {
                                            new ()
                                            {
                                                Id = (Guid)foundCategory.ParentCategoryId,
                                                Name = foundCategory.ParentCategoryName,
                                            }
                                        },
                                  });
    }
}
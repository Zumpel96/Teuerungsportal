using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using Shared.DatabaseObjects;

public static class AddCategory
{
    [FunctionName("AddCategory")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "category/new/{categoryName}")] HttpRequest req,
        [Sql(
                commandText: @"
                                BEGIN
                                   IF NOT EXISTS (SELECT * FROM [dbo].[category] WHERE LOWER([name]) = LOWER(@categoryName))
                                   BEGIN
                                       INSERT INTO [dbo].[category] ([name], [parentId])
                                       VALUES (@categoryName, '23b3d57b-7d2f-4544-b4d3-3b7fdbdd22f8')
                                   END
                                END",
                parameters: "@categoryName={categoryName}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<ProductDbo> products)
    {
        return new OkResult();
    }
}
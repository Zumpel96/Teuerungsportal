using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Api;

using System.Collections.Generic;

public static class GetAllCategories
{
	public class Category
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string RecursionId { get; set; }
	}

	
    [FunctionName("GetAllCategories")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories")] HttpRequest req,
        [Sql(
                commandText: @"WITH recursion AS 
								(
									SELECT  c.*, CAST(ROW_NUMBER() OVER (ORDER BY c.id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN AS rc
									FROM    [dbo].[category] c
									WHERE   parentId IS NULL
									UNION ALL
									SELECT  c.*,  recursion.rc + '.' + CAST(ROW_NUMBER() OVER (PARTITION BY c.parentId ORDER BY c.Id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN
									FROM    [dbo].[category] c		
									JOIN    recursion
									ON      c.parentID = recursion.id
								)
								SELECT [id], [name], [rc] AS [RecursionId]
								FROM recursion
								ORDER BY rc",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<Category> categories)
    {
		return new OkObjectResult(categories);
    }
}
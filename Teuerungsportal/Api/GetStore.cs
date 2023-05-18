using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class GetStore
{
    public class StoreDbo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string BaseUrl { get; set; }
    }

    [FunctionName("GetSpecificStore")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeName}")] HttpRequest req,
        [Sql(
                commandText: @"SELECT id, name, baseUrl
				 				 FROM [dbo].[store]
				 				 WHERE LOWER(name) = LOWER(@storeName);",
                parameters: "@storeName={storeName}",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<StoreDbo> stores)
    {
        var storeList = stores.ToList();
        if (!storeList.Any())
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(storeList.First());
    }
}
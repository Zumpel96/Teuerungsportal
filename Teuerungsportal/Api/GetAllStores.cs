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

public class Store
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string BaseUrl { get; set; }
}

public static class GetAllStores
{
    [FunctionName("GetAllStores")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores")] HttpRequest req,
        [Sql(
                commandText: "SELECT [id], [name], [baseUrl] from [dbo].[store]",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<Store> stores)
    {
        return new OkObjectResult(stores);
    }
}
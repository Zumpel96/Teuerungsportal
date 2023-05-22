using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetDonators
{
    [FunctionName("GetDonators")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "donators")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [name]
                                FROM 
                                  [dbo].[donator] 
                                ORDER BY 
                                  [name];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<DonatorDbo> donators)
    {
        return new OkObjectResult(donators.Select(dbo => new Donator(dbo)));
    }
}
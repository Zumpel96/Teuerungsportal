using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Api;

using System.Collections.Generic;
using System.Linq;
using Shared.DatabaseObjects;
using Teuerungsportal.Models;

public static class GetAnnouncement
{
    [FunctionName("GetAnnouncement")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "announcement")] HttpRequest req,
        [Sql(
                commandText: @"
                                SELECT 
                                  [contentDe],
                                  [contentEn]
                                FROM 
                                  [dbo].[announcement];",
                commandType: System.Data.CommandType.Text,
                connectionStringSetting: "SqlConnectionString")]
        IEnumerable<AnnouncementDbo> announcements)
    {
        return new OkObjectResult(announcements.Select(dbo => new Announcement(dbo)).First());
    }
}
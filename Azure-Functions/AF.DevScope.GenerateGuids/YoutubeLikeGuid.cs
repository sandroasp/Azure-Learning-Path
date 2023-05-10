using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class YoutubeLikeGuid
{
    [FunctionName("YoutubeLikeGuid")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return new OkObjectResult(base64Guid);
    }
}

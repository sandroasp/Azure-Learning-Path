using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace AF.DevScope.GenerateGuids
{
    public static class TicketsShortGuid
    {
        [FunctionName("TicketsShortGuid")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get the year from the query parameter or route parameter
            string yearParam = req.GetQueryParameterDictionary().FirstOrDefault(q => string.Compare(q.Key, "year", true) == 0).Value;

            int year = string.IsNullOrEmpty(yearParam) ? 1978 : Convert.ToInt32(yearParam);

            // Calculate the uniqueId using the provided year
            var ticks = new DateTime(year, 1, 1).Ticks;
            var ans = DateTime.Now.Ticks - ticks;
            var uniqueId = ans.ToString("x");

            return new OkObjectResult(uniqueId);
        }
    }
}

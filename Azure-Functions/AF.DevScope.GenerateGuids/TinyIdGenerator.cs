using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;

namespace AF.DevScope.GenerateGuids
{
    public static class TinyIdGenerator
    {
        [FunctionName("TinyIdGenerator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            Guid guid = Guid.NewGuid();
            string modifiedBase64 = Convert.ToBase64String(guid.ToByteArray())
                .Replace('+', '-').Replace('/', '_') // avoid invalid URL characters
                .Substring(0, 22);

            return new OkObjectResult(modifiedBase64);
        }
    }
}

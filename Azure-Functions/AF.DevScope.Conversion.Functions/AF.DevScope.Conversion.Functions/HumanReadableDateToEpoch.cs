using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AF.DevScope.Conversion.Functions
{
    public static class HumanReadableDateToEpoch
    {
        [FunctionName("HumanReadableDateToEpoch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            HumanObj data = JsonConvert.DeserializeObject<HumanObj>(requestBody);

            try
            {
                DateTime myDate = DateTime.ParseExact(data.datetime, data.format, null);
                return new OkObjectResult((myDate.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
            }
            catch (Exception ex)
            {
                var result = new ObjectResult(ex.Message);
                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }

    internal class HumanObj
    {
        public string datetime { get; set; }
        public string format { get; set; }
    }
}

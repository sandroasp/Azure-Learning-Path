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
    public static class EpochToHumanReadableDate
    {
        [FunctionName("EpochToHumanReadableDate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            EpochObj data = JsonConvert.DeserializeObject<EpochObj>(requestBody);

            try
            {
                return new OkObjectResult(new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(data.unixtime)));
            }
            catch (Exception ex)
            {
                var result = new ObjectResult(ex.Message);
                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }

    internal class EpochObj
    {
        public long unixtime { get; set; }
    }
}

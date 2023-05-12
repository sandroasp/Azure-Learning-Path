using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AF.DevScope.Conversion.Functions
{
    public static class DateTimeConversion
    {
        [FunctionName("DateTimeConversion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            DateTimeObj data = JsonConvert.DeserializeObject<DateTimeObj>(requestBody);

            try
            {
                System.DateTime dateTime;
                if (System.DateTime.TryParseExact(data.inputDate, data.inputFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out dateTime))
                {
                    return new OkObjectResult(dateTime.ToString(data.outputFormat));
                }
                return new OkObjectResult(data.inputDate);
            }
            catch (Exception ex)
            {
                var result = new ObjectResult(ex.Message);
                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }

    internal class DateTimeObj
    {
        public string inputDate { get; set; }
        public string inputFormat { get; set; }
        public string outputFormat { get; set; }
    }
}

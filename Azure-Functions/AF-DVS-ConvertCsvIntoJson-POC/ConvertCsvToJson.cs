using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CsvHelper;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CsvHelper.Configuration;

namespace AF_DVS_ConvertCsvIntoJson_POC
{
    public static class ConvertCsvToJsonFunction
    {
        [FunctionName("ConvertCsvWithHeadersToJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new BadRequestObjectResult("Please provide CSV data in the request body.");
            }

            try
            {
                string json = ConvertCsvStringToJson(requestBody);

                var jsonObject = JsonConvert.DeserializeObject(json);
                return new JsonResult(jsonObject)
                {
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing CSV data");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static string ConvertCsvStringToJson(string csvString)
        {
            // Define CSV configuration to use ';' as the delimiter
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"

            };
            // Use a StringReader to simulate a file-like object for CsvHelper
            using (var reader = new StringReader(csvString))
            using (var csv = new CsvReader(reader, config))
            {
                // Read the records from the CSV string
                var records = csv.GetRecords<dynamic>();

                // Convert the list of records to JSON
                string json = JsonConvert.SerializeObject(records, Formatting.Indented);

                return json;
            }
        }
    }
}

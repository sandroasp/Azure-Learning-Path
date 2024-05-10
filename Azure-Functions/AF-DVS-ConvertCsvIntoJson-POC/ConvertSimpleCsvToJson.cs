using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;

namespace AF_DVS_ConvertCsvIntoJson_POC
{
    public static class ConvertSimpleCsvToJson
    {
        [FunctionName("ConvertSimpleCsvToJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
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
            // Define CSV configuration to use ';' as the delimiter and no headers
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false
            };

            // List to hold records with generated headers
            var recordsWithHeaders = new List<Dictionary<string, object>>();

            // Use a StringReader to simulate a file-like object for CsvHelper
            using (var reader = new StringReader(csvString))
            using (var csv = new CsvReader(reader, config))
            {
                bool firstRecord = true;
                int fieldCount = 0;

                while (csv.Read())
                {
                    if (firstRecord)
                    {
                        fieldCount = csv.Parser.Count;
                        firstRecord = false;
                    }

                    var record = new Dictionary<string, object>();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        string header = $"Column{i + 1}";
                        record[header] = csv.GetField(i);
                    }
                    recordsWithHeaders.Add(record);
                }
            }

            // Convert the list of records to JSON
            string json = JsonConvert.SerializeObject(recordsWithHeaders, Formatting.Indented);

            return json;
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SASP.FunctionApps.Samples.NetCore
{
    public static class GetAzureAppConfigurationValue
    {
        [FunctionName("GetAzureAppConfigurationValue")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");
            //Read appkey Query parameter from the GET operation
            string appKey = req.Query["appKey"];

            if(string.IsNullOrEmpty(appKey))
                return new BadRequestObjectResult("Mandatory parameter 'appKey' not found or empty.");

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("AppConfigConnectionString");

                var builder = new ConfigurationBuilder();
                builder.AddAzureAppConfiguration(connectionString);
                var build = builder.Build();

                string keyValue = build[appKey.ToString()];

                if (string.IsNullOrEmpty(keyValue))
                {
                    var result = new ObjectResult("Azure Configuration Key not found - " + appKey);
                    result.StatusCode = StatusCodes.Status404NotFound;
                    return result;
                }
                else return new OkObjectResult(keyValue);
            }
            catch(Exception ex)
            {
                var result = new ObjectResult(ex.Message);
                result.StatusCode = StatusCodes.Status500InternalServerError;
                return result;
            }
        }
    }
}

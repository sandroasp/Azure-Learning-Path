 using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SASP.FunctionApps.LoopbackAPI
{
    public static class Loopback
    {
        [FunctionName("Loopback")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);

            string contentType = req.Headers["Content-Type"];

            return new ContentResult { Content = requestBody, ContentType = req.Headers["Content-Type"] };



            //if (contentType == "application/json")
            //{
            //    return new JsonResult(JsonConvert.DeserializeObject(requestBody)) { StatusCode = 200, ContentType = "application/json" };
            //}
            //if(contentType == "text/xml")
            //{
            //    var xmlDoc = new System.Xml.XmlDocument();
            //    xmlDoc.LoadXml(requestBody); 
            //    req.HttpContext.Response.Headers.Add("Content-Type", contentType);
            //    return new ContentResult { Content = requestBody, ContentType = "application/xml" };
            //}

            //return new OkObjectResult(requestBody);
        }
    }
}

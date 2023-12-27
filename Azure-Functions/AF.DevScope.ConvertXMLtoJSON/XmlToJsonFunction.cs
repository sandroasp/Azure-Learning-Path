using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class XMLtoJSONFunction
{
    [FunctionName("ConvertXMLtoJSON")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        //log.LogInformation("C# HTTP trigger function processed a request.");
        
        string requestBody;
        using (StreamReader streamReader = new StreamReader(req.Body))
        {
            requestBody = await streamReader.ReadToEndAsync();
        }

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(requestBody);

            string jsonResult = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

            // Set the content type to application/json
            var result = new ContentResult
            {
                Content = jsonResult,
                ContentType = "application/json",
                StatusCode = 200
            };

            return result;
        }
        catch (Exception ex)
        {
            log.LogError($"Error converting XML to JSON: {ex.Message}");
            return new BadRequestObjectResult("Error converting XML to JSON:" + ex.Message);
        }
    }
}
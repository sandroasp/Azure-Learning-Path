using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml;
using Microsoft.VisualBasic;
using System.Text;
using System.Xml.Linq;
using System.IO.Pipes;
using System.Xml.Serialization;

namespace AF.DevScope.XmlDTDValidation
{
    public static class ValidateXMLAgainstDTD
    {
        [FunctionName("ValidateXMLAgainstDTD")]
        public static async Task<IActionResult> Run(
         [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
         ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string connectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");

            // Read containerName from local.settings.json
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            // Read dtdFileName from request headers
            string dtdFileName = req.Headers["DTDFileName"];

            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(dtdFileName))
            {
                return new BadRequestObjectResult("Bad Request. ContainerName and DTDFileName headers are required.");
            }

            try
            {
                // Download DTD from Blob Storage
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(dtdFileName);
                string dtdContent = await blob.DownloadTextAsync();

                string path = Directory.GetCurrentDirectory();
                if(!File.Exists(Path.Combine(path, dtdFileName)))
                {
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, dtdFileName)))
                    {
                        outputFile.WriteLine(dtdContent);
                    }
                }

                using (StringReader xmlReader = new StringReader(requestBody.ToString()))
                {
                    XmlReaderSettings xmlSettings = new XmlReaderSettings()
                    {
                        DtdProcessing = DtdProcessing.Parse,
                        ValidationType = ValidationType.DTD,
                        XmlResolver = new XmlUrlResolver()
                    };
                    List<string> validationErrors = new List<string>();
                    xmlSettings.ValidationEventHandler += (sender, e) =>
                    {
                        if (e.Severity == XmlSeverityType.Error)
                        {
                            validationErrors.Add(e.Message);
                        }
                    };

                    XmlReader reader = XmlReader.Create(xmlReader, xmlSettings);
                    while (reader.Read()) ;

                    if (validationErrors.Count > 0)
                    {
                        string errorMessage = string.Join(", ", validationErrors);
                        log.LogError($"Validation Errors: {errorMessage}");

                        // Include detailed validation error messages in the response
                        return new BadRequestObjectResult($"Bad Request. XML validation failed. Validation Errors: {errorMessage}");
                    }
                }

                return new OkObjectResult("Validation successful.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error: {ex.Message}");
                return new BadRequestObjectResult($"Bad Request. XML validation failed. Error: {ex.Message}");
            }
        }
    }
}
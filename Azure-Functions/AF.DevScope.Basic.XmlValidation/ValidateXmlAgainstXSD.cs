using System.IO;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.Xml.Schema;
using System;
using System.Collections.Generic;

public static class ValidateXmlAgainstXSD
{
    [FunctionName("Basic-XMLSchemaValidation")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        string connectionString = Environment.GetEnvironmentVariable("StorageAccountConnectionString");

        // Read containerName from local.settings.json
        string containerName = Environment.GetEnvironmentVariable("ContainerName");

        // Read schemaFileName from request headers
        string schemaFileName = req.Headers["SchemaFileName"];

        if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(schemaFileName))
        {
            return new BadRequestObjectResult("Bad Request. ContainerName and SchemaFileName are required.");
        }

        try
        {
            // Download XSD from Blob Storage
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(schemaFileName);
            string xsdContent = await blob.DownloadTextAsync();

            // Validate XML against XSD
            using (StringReader xsdReader = new StringReader(xsdContent))
            using (StringReader xmlReader = new StringReader(requestBody))
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(null, XmlReader.Create(xsdReader));

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemaSet;

                // Add validation flags
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                List<string> validationErrors = new List<string>();
                settings.ValidationEventHandler += (sender, e) =>
                {
                    if (e.Severity == XmlSeverityType.Error)
                    {
                        validationErrors.Add(e.Message);
                    }
                    else if (e.Severity == XmlSeverityType.Warning)
                    {
                        // Include warnings in validationErrors if needed
                        validationErrors.Add(e.Message);
                    }
                };

                using (XmlReader reader = XmlReader.Create(xmlReader, settings))
                {
                    while (reader.Read()) ;
                }

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
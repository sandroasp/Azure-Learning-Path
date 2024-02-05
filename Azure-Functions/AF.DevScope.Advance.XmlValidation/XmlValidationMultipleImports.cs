using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public static class XmlValidationMultipleImports
{
    [FunctionName("Advsnce-XMLSchemaValidation")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            // Read the XML content from the HTTP request
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Get the main schema file name from the HTTP request header
            string mainSchemaFileName = req.Headers["X-Main-Schema-Name"];

            if (string.IsNullOrEmpty(mainSchemaFileName))
            {
                log.LogError("Main schema file name not provided in the header.");
                return new BadRequestObjectResult("Main schema file name not provided in the header.");
            }

            // Download schemas from the storage account container
            XmlSchemaSet schemaSet = await DownloadSchemasAsync(mainSchemaFileName, log);

            // Validate the XML against the schemas
            ValidationResults validationResults = ValidateXml(requestBody, schemaSet, log);

            if (validationResults.IsValid)
            {
                log.LogInformation("XML validation succeeded.");
                return new OkObjectResult("XML validation succeeded.");
            }
            else
            {
                log.LogError($"XML validation failed: {string.Join(", ", validationResults.ErrorMessages)}");
                return new BadRequestObjectResult($"XML validation failed: {string.Join(", ", validationResults.ErrorMessages)}");
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Exception during XML validation: {ex.Message}");
            return new BadRequestObjectResult($"Exception during XML validation. Errors: {ex.Message}");
        }
    }

    private static async Task<XmlSchemaSet> DownloadSchemasAsync(string mainSchemaFileName, ILogger log)
    {
        XmlSchemaSet schemaSet = new XmlSchemaSet();

        try
        {
            // Read values from environment variables
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Download the main schema
            CloudBlockBlob mainSchemaBlob = container.GetBlockBlobReference(mainSchemaFileName);

            using (var memoryStream = new MemoryStream())
            {
                await mainSchemaBlob.DownloadToStreamAsync(memoryStream);
                memoryStream.Position = 0;

                XmlSchema mainSchema = XmlSchema.Read(memoryStream, ValidationCallback);
                schemaSet.Add(mainSchema);

                // Process imports recursively
                ProcessImports(mainSchema, schemaSet, container, log);
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Exception during downloading schemas: {ex.Message}");
            throw;
        }

        return schemaSet;
    }

    private static void ProcessImports(XmlSchema schema, XmlSchemaSet schemaSet, CloudBlobContainer container, ILogger log)
    {
        foreach (XmlSchemaExternal external in schema.Includes)
        {
            if (external is XmlSchemaImport import)
            {
                string importSchemaName = import.SchemaLocation;
                CloudBlockBlob importBlob = container.GetBlockBlobReference(importSchemaName);

                using (var importMemoryStream = new MemoryStream())
                {
                    importBlob.DownloadToStreamAsync(importMemoryStream).Wait();
                    importMemoryStream.Position = 0;

                    XmlSchema importedSchema = XmlSchema.Read(importMemoryStream, ValidationCallback);
                    schemaSet.Add(importedSchema);

                    // Recursively process imports
                    ProcessImports(importedSchema, schemaSet, container, log);
                }
            }
        }
    }

    private static void ValidationCallback(object sender, ValidationEventArgs args)
    {
        // Handle validation warnings or errors
        // Log or process the validation messages as needed
    }

    private static ValidationResults ValidateXml(string xmlContent, XmlSchemaSet schemaSet, ILogger log)
    {
        try
        {
            XmlReaderSettings settings = new XmlReaderSettings
            {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

            List<string> errorMessages = new List<string>();

            settings.ValidationEventHandler += (sender, args) =>
            {
                if (args.Severity == XmlSeverityType.Error)
                {
                    errorMessages.Add(args.Message);
                }
                if (args.Severity == XmlSeverityType.Warning)
                {
                    if (args.Message.ToString().Contains("Could not find schema information for the"))
                        errorMessages.Add(args.Message);
                }
            };

            using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlContent)))
            {
                using (XmlReader reader = XmlReader.Create(ms, settings))
                {
                    while (reader.Read()) { } // Read the entire XML to trigger validation
                }
            }

            return new ValidationResults(errorMessages.Count == 0, errorMessages);
        }
        catch (XmlException ex)
        {
            return new ValidationResults(false, new List<string> { ex.Message }); // Validation failed
        }
    }

    private class ValidationResults
    {
        public bool IsValid { get; }
        public List<string> ErrorMessages { get; }

        public ValidationResults(bool isValid = true, List<string> errorMessages = null)
        {
            IsValid = isValid;
            ErrorMessages = errorMessages ?? new List<string>();
        }
    }
}

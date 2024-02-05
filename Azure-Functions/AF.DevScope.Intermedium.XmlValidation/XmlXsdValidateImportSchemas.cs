using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Linq;

public static class XmlXsdValidateImportSchemas
{
    [FunctionName("Intermedium-XMLSchemaValidation")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        // Retrieve values from local.settings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        string mainSchemaFileName = req.Headers["MainSchemaFileName"];
        string containerName = config["ContainerName"];
        string storageConnectionString = config["StorageConnectionString"];

        List<string> errorMessages = new List<string>();

        if (string.IsNullOrEmpty(mainSchemaFileName) || string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(storageConnectionString))
        {
            errorMessages.Add("MainSchemaFileName, ContainerName, and StorageConnectionString in local.settings.json are required.");
            return new BadRequestObjectResult($"Internal Error. Errors: {string.Join(", ", errorMessages)}");
        }

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        CloudStorageAccount storageAccount;
        try
        {
            if (!CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                throw new InvalidOperationException("Invalid storage connection string");
            }
        }
        catch (Exception ex)
        {
            errorMessages.Add($"{ex.Message}");
            log.LogError($"Internal Error: {ex.Message}");
            return new BadRequestObjectResult($"Internal Error: {string.Join(", ", errorMessages)}");
        }

        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);

        XmlReaderSettings settings = new XmlReaderSettings();

        // Assuming you have one XSD file in the container for simplicity
        CloudBlockBlob blob = container.GetBlockBlobReference(mainSchemaFileName);
        XmlReader schemaReader = XmlReader.Create(await blob.OpenReadAsync());
        XmlSchema schema = XmlSchema.Read(schemaReader, ValidationCallback);
        settings.Schemas.Add(schema);

        foreach (XmlSchemaImport import in schema.Includes)
        {
            string importSchemaName = import.SchemaLocation.ToString();
            CloudBlockBlob importBlob = container.GetBlockBlobReference(importSchemaName);
            XmlReader importSchemaReader = XmlReader.Create(await importBlob.OpenReadAsync());
            XmlSchema importedSchema = XmlSchema.Read(importSchemaReader, ValidationCallback);
            settings.Schemas.Add(importedSchema);
        }

        settings.ValidationType = ValidationType.Schema;

        // Set up event handler for validation errors
        settings.ValidationEventHandler += (sender, args) =>
        {
            errorMessages.Add(args.Message);
        };

        try
        {
            XmlReader reader = XmlReader.Create(new StringReader(requestBody), settings);
            XmlDocument document = new XmlDocument();
            document.Load(reader);

            if (errorMessages.Any())
            {
                log.LogError($"XML validation failed against the schemas. Errors: {string.Join(", ", errorMessages)}");
                return new BadRequestObjectResult($"XML validation failed against the schemas. Errors: {string.Join(", ", errorMessages)}");
            }

            return new OkObjectResult("XML is valid against the schemas");
        }
        catch (Exception ex)
        {
            errorMessages.Add($"Exception during XML validation: {ex.Message}");
            log.LogError($"Exception during XML validation: {ex.Message}");
            return new BadRequestObjectResult($"Exception during XML validation. Errors: {string.Join(", ", errorMessages)}");
        }
    }

    static void ValidationCallback(object sender, ValidationEventArgs args)
    {
        // Your existing validation callback code
    }
}

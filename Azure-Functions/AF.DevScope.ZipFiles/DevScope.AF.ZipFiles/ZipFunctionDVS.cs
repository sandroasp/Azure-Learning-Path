using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class ZipFunction
{
    [FunctionName("ZipData")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        var contentType = req.ContentType;
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return new BadRequestObjectResult("Please provide input data in the request body.");
        }

        byte[] zippedBytes;

        // Determine the file extension based on content type
        string fileExtension = DetermineFileExtension(contentType);

        if (!string.IsNullOrEmpty(fileExtension))
        {
            // Compress the data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry($"data{fileExtension}");
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write(requestBody);
                    }
                }
                zippedBytes = memoryStream.ToArray();
            }

            return new FileContentResult(zippedBytes, "application/zip")
            {
                FileDownloadName = "output.zip"
            };
        }
        else
        {
            return new BadRequestObjectResult("Unsupported content type.");
        }
    }

    private static string DetermineFileExtension(string contentType)
    {
        switch (contentType)
        {
            case "application/json":
                return ".json";
            case "text/xml":
                return ".xml";
            case "text/plain":
                return ".txt";
            // Add more cases for other supported content types
            default:
                return string.Empty; // Unsupported content type
        }
    }
}

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PdfSharp.Drawing;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class MergeFilesFunction
{
    private static IConfigurationRoot _config;

    static MergeFilesFunction()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _config = builder.Build();
    }

    [FunctionName("MergeFilesToPdf")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        string[] fileNames;

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            // If the request body is empty, list all blobs in the specified container
            fileNames = await GetAllBlobNamesAsync(log);
            if (fileNames.Length == 0)
            {
                return new NotFoundResult();
            }
        }
        else
        {
            try
            {
                // Try to deserialize the JSON data
                var requestData = JsonConvert.DeserializeObject<RequestData>(requestBody);
                fileNames = requestData.FileNames;
            }
            catch (JsonException)
            {
                // If deserialization fails, return BadRequest
                return new BadRequestResult();
            }
        }

        using (PdfDocument pdfDocument = new PdfDocument())
        {
            foreach (string fileName in fileNames)
            {
                byte[] fileBytes = await GetBlobContentAsync(fileName);

                if (fileBytes != null)
                {
                    // Process each file based on its extension
                    if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        using (MemoryStream stream = new MemoryStream(fileBytes))
                        {
                            PdfDocument inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
                            foreach (PdfPage page in inputDocument.Pages)
                            {
                                pdfDocument.AddPage(page);
                            }
                        }
                    }
                    else if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        using (MemoryStream stream = new MemoryStream(fileBytes))
                        {
                            using (Image image = Image.Load(stream))
                            {
                                PdfPage page = pdfDocument.AddPage();
                                int targetWidth = (int)page.Width.Point;
                                image.Mutate(x => x.Resize(new ResizeOptions
                                {
                                    Size = new Size(targetWidth, 0),
                                    Mode = ResizeMode.Max
                                }));

                                double pageWidth = page.Width.Point;
                                double pageHeight = page.Height.Point;

                                double centerX = (pageWidth - image.Width) / 2;
                                double centerY = (pageHeight - image.Height) / 2;

                                XGraphics gfx = XGraphics.FromPdfPage(page);
                                using (MemoryStream imageStream = new MemoryStream())
                                {
                                    image.Save(imageStream, new PngEncoder());
                                    imageStream.Position = 0;
                                    XImage xImage = XImage.FromStream(imageStream);
                                    gfx.DrawImage(xImage, centerX, centerY, image.Width, image.Height);
                                }
                            }
                        }
                    }
                }
            }

            // Generate a unique file name for the merged PDF
            string dateTimeFormat = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string mergedPdfFileName = $"MergedFile_{dateTimeFormat}.pdf";

            byte[] mergedPdfBytes;
            using (MemoryStream outputStream = new MemoryStream())
            {
                // Save the merged PDF document to byte array
                pdfDocument.Save(outputStream, false);
                mergedPdfBytes = outputStream.ToArray();
            }

            // Save the merged PDF to the "generatedpdfs" container
            await SavePdfToGeneratedPdfsContainerAsync(mergedPdfBytes, mergedPdfFileName);

            // Set the file name in the response headers
            var contentDispositionHeader = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = mergedPdfFileName
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(mergedPdfBytes)
            };
            response.Content.Headers.ContentDisposition = contentDispositionHeader;
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            // Set the file name in the response headers
            var result = new FileContentResult(mergedPdfBytes, "application/pdf")
            {
                FileDownloadName = mergedPdfFileName
            };

            // Add the file name to the response headers
            req.HttpContext.Response.Headers.Add("Filename", mergedPdfFileName);

            // Return the merged PDF as response
            return new FileContentResult(mergedPdfBytes, "application/pdf")
            {
                FileDownloadName = mergedPdfFileName
            };
        }
    }

    private static async Task SavePdfToGeneratedPdfsContainerAsync(byte[] pdfBytes, string fileName)
    {
        // Get reference to storage account and container
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config["ConnectionString"]);
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference(_config["GeneratedPdfsContainerName"]);

        // Create the container if it doesn't exist
        await container.CreateIfNotExistsAsync();

        // Upload the merged PDF to the container
        CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
        using (MemoryStream stream = new MemoryStream(pdfBytes))
        {
            await blob.UploadFromStreamAsync(stream);
        }
    }

    private static async Task<byte[]> GetBlobContentAsync(string fileName)
    {
        // Get reference to storage account and container
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config["ConnectionString"]);
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference(_config["BlobContainerName"]);

        // Get the blob reference
        CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

        if (await blob.ExistsAsync())
        {
            // Download blob content to memory stream
            using (MemoryStream stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);
                return stream.ToArray();
            }
        }

        return null;
    }

    private static async Task<string[]> GetAllBlobNamesAsync(ILogger log)
    {
        // Get reference to storage account and container
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_config["ConnectionString"]);
        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference(_config["BlobContainerName"]);

        // List all blobs in the container
        BlobContinuationToken continuationToken = null;
        List<string> fileNames = new List<string>();

        do
        {
            var resultSegment = await container.ListBlobsSegmentedAsync(null, continuationToken);
            foreach (var item in resultSegment.Results)
            {
                if (item is CloudBlob blob)
                {
                    fileNames.Add(blob.Name);
                }
            }
            continuationToken = resultSegment.ContinuationToken;
        } while (continuationToken != null);

        return fileNames.ToArray();
    }
    public class RequestData
    {
        public string[] FileNames { get; set; }
    }
}

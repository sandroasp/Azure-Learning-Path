

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using DotLiquid;
using System.Net;
using Microsoft.Extensions.Options;

namespace AF.DevScope.ApplyLiquidTransformation
{
    public static class LiquidTransformation
    {
        [FunctionName("LiquidTransformation")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                #region Read function configurations

                string stgAccConnectionString = System.Environment.GetEnvironmentVariable("StorageAccountConnectionString", EnvironmentVariableTarget.Process);
                string containerName = System.Environment.GetEnvironmentVariable("ContainerName", EnvironmentVariableTarget.Process);

                if (string.IsNullOrEmpty(stgAccConnectionString))
                    throw new Exception("Storage Account Connection String configuration is missing.");
                if (string.IsNullOrEmpty(containerName))
                    throw new Exception("Container name configuration is missing.");

                #endregion

                // Retrieve headers from HTTP request and check if the header is present and not null or empty
                #region Read mandatory and optional HTTP Headers

                if (!req.Headers.TryGetValue("Content-Type", out var contentTypeHeaderValues) ||
                  String.IsNullOrEmpty(contentTypeHeaderValues.FirstOrDefault()))
                {
                    // Handle the missing or empty header (e.g., return a bad request response)
                    throw new Exception("Header 'Content-Type' is missing or empty.");
                }
                string requestContentType = contentTypeHeaderValues.FirstOrDefault();
                
                if (!req.Headers.TryGetValue("LiquidFileName", out var headerValues) ||
                   String.IsNullOrEmpty(headerValues.FirstOrDefault()))
                {
                    throw new Exception("Header 'LiquidFileName' is missing or empty.");
                }
                string liquidFileName = headerValues.FirstOrDefault();

                string outputContentType = string.Empty;
                if (!req.Headers.TryGetValue("Output-Content-Type", out var outHeaderValues) ||
                   String.IsNullOrEmpty(outHeaderValues.FirstOrDefault()))
                    outputContentType = "application/json";
                else outputContentType = outHeaderValues.FirstOrDefault();

                string delimiterChar = string.Empty;
                if (!req.Headers.TryGetValue("CSVDelimiterChar", out var delimiterHeaderValues) ||
                   String.IsNullOrEmpty(delimiterHeaderValues.FirstOrDefault()))
                {
                    if(requestContentType.ToLower() == "text/csv")
                        throw new Exception("Header 'CSVDelimiterChar' is missing or empty.");
                    delimiterChar = "";
                }
                else delimiterChar = delimiterHeaderValues.FirstOrDefault();

                #endregion

                #region Retrieve liquid file from storage account

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(stgAccConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(liquidFileName);

                string liquidMap = null;
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memoryStream))
                    {
                        liquidMap = await reader.ReadToEndAsync();
                    }
                }

                #endregion

                if(requestContentType.ToLower() == "application/json")
                {
                    var transformer = Transformer.SetLiquidTransformerMap(liquidMap);
                    var transformedString = transformer.RenderFromString(requestBody, "content");

                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(transformedString, Encoding.Default, outputContentType),
                    };
                }
                else
                {
                    var contentReader = ContentTypeFactory.GetContentReader(requestContentType);
                    var contentWriter = ContentTypeFactory.GetContentWriter(outputContentType);
                    Hash inputHash;
                    inputHash = await contentReader.ParseRequestAsync(requestBody, delimiterChar);

                    Template.RegisterFilter(typeof(CustomFilters));
                    Template template;
                    template = Template.Parse(liquidMap);

                    string output = string.Empty;
                    output = template.Render(inputHash);

                    if (template.Errors != null && template.Errors.Count > 0)
                        throw new Exception($"Error rendering Liquid template: {template.Errors[0].Message}");

                    var content = contentWriter.CreateResponse(output);

                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent(output, Encoding.Default, outputContentType),
                    };
                }

            }
            catch (Exception ex)
            {
                var errorResponse = new JArray();
                var item = new JObject();
                item.Add(new JProperty("name", "LiquidTransformation function"));
                item.Add(new JProperty("type", "Internal Error"));
                item.Add(new JProperty("status", "Failed"));
                item.Add(new JProperty("code", "500"));
                item.Add(new JProperty("startTime", DateTime.Now.ToString()));
                item.Add(new JProperty("endTime", DateTime.Now.ToString()));
                item.Add(new JProperty("errorMessage", ex.Message));
                errorResponse.Add(item);
                //return new OkObjectResult(errorResponse);

                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(errorResponse.ToString(), Encoding.Default, @"application/json"),
                };
            }
        }
    }
}

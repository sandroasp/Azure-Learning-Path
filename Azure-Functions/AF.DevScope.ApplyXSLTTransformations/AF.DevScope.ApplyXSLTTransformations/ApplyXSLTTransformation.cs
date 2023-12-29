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
using System.Xml.Xsl;
using System.Xml;
using Microsoft.Net.Http.Headers;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace AF.DevScope.ApplyXSLTTransformations
{
    public static class ApplyXSLTTransformation
    {
        [FunctionName("ApplyXSLTTransformation")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");


            try
            {
                // Read function configurations
                string stgAccConnectionString = System.Environment.GetEnvironmentVariable("StorageAccountConnectionString", EnvironmentVariableTarget.Process);
                string containerName = System.Environment.GetEnvironmentVariable("ContainerName", EnvironmentVariableTarget.Process);

                if (string.IsNullOrEmpty(stgAccConnectionString))
                    throw new Exception("Storage Account Connection String configuration is missing.");
                if (string.IsNullOrEmpty(containerName))
                    throw new Exception("Container name configuration is missing.");

                // Retrieve headers from HTTP request and check if the header is present and not null or empty
                if (!req.Headers.TryGetValue("XsltFileName", out var headerValues) ||
                   String.IsNullOrEmpty(headerValues.FirstOrDefault()))
                {
                    // Handle the missing or empty header (e.g., return a bad request response)
                    throw new Exception("Header 'XsltFileName' is missing or empty.");
                }
                // Extract the header value
                string xsltFileName = headerValues.FirstOrDefault();

                string outputContentType = string.Empty;
                if (!req.Headers.TryGetValue("Output-Content-Type", out var outHeaderValues) ||
                   String.IsNullOrEmpty(outHeaderValues.FirstOrDefault()))
                    outputContentType = "text/xml";
                else outputContentType = outHeaderValues.FirstOrDefault();


                // Parse input XML
                string xmlInput = await new StreamReader(req.Body).ReadToEndAsync();
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlInput);

                // Retrieve XSLT file from storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(stgAccConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(xsltFileName);

                string xsltContent = null;
                using (var memoryStream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memoryStream))
                    {
                        xsltContent = await reader.ReadToEndAsync();
                    }
                }

                // Apply XSLT transformation
                XslCompiledTransform xslt = new XslCompiledTransform();
                using (var reader = XmlReader.Create(new StringReader(xsltContent)))
                {
                    xslt.Load(reader);
                }

                StringWriter stringWriter = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
                {
                    xslt.Transform(xmlDocument, xmlWriter);
                }
                string outputXml = stringWriter.ToString();

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(outputXml, Encoding.Default, outputContentType),
                };
            }
            catch (Exception ex)
            {
                var errorResponse = new JArray();
                var item = new JObject();
                item.Add(new JProperty("name", "ApplyXSLTTransformation function"));
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

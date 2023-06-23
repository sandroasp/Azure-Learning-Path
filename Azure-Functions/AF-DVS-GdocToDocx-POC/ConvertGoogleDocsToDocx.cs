using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class ConvertGoogleDocsToDocx
{
    [FunctionName("ConvertGoogleDocsToDocx")]
    public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string googleDocsFileId = req.Query["fileId"];
        string googleCredentialsJson = req.Headers["X-Secret-Google-Credentials"];

        if (string.IsNullOrEmpty(googleDocsFileId))
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Please provide the 'fileId' query parameter.")
            };
        }

        if (string.IsNullOrEmpty(googleCredentialsJson))
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Please provide the 'X-Secret-Google-Credentials' header.")
            };
        }

        try
        {
            // Load the service account credentials from the JSON file
            var credential = GoogleCredential.FromJson(googleCredentialsJson)
                .CreateScoped(DocsService.ScopeConstants.DocumentsReadonly, DriveService.ScopeConstants.DriveReadonly);

            // Create the Drive and Docs services using the service account credentials
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Docs to DOCX Converter"
            });

            // Export the Google Doc as DOCX
            var exportRequest = service.Files.Export(googleDocsFileId, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            var streamResult = await exportRequest.ExecuteAsStreamAsync();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(streamResult);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "converted.docx"
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            return response;
        }
        catch (Exception ex)
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent($"An error occurred: {ex.Message}")
            };
        }
    }
}

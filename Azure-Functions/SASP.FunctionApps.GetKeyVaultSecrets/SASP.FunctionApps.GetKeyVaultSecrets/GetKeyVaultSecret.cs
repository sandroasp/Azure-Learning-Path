using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.Net.Http;
using System.Text;
using System.Net;

namespace SASP.FunctionApps.GetKeyVaultSecrets
{
    public static class GetKeyVaultSecret
    {
        [FunctionName("GetKeyVaultSecret")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            SecretRequest data = JsonConvert.DeserializeObject<SecretRequest>(requestBody);


            // Create a new secret client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
            var client = new SecretClient(vaultUri: new Uri(data.KeyVaultUrl), credential: new DefaultAzureCredential());

            // Retrieve a secret using the secret client.
            KeyVaultSecret secret = client.GetSecret(data.SecretName);

            var secretResponse = new SecretResponse { Secret = "sgskvslajwtclientid", Value = secret.Value };

            //return new HttpResponseMessage(HttpStatusCode.OK)
            //{
            //    Content = new StringContent(JsonConvert.SerializeObject(secretResponse), Encoding.UTF8, "application/json")
            //};


            return new OkObjectResult(JsonConvert.SerializeObject(secretResponse));



            //SecretRequest secretRequest = await req.Content.ReadAsAsync();

            //if (string.IsNullOrEmpty(secretRequest.Secret))
            //    return req.CreateResponse(HttpStatusCode.BadRequest, “Request does not contain a valid Secret.”);

            //log.Info($”GetKeyVaultSecret request received for secret { secretRequest.Secret}”);

            //var serviceTokenProvider = new AzureServiceTokenProvider();

            //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));

            //var secretUri = SecretUri(secretRequest.Secret);
            //log.Info($”Key Vault URI { secretUri}
            //generated”);
            //SecretBundle secretValue;
            //try
            //{
            //    secretValue = await keyVaultClient.GetSecretAsync(secretUri);
            //}
            //catch (KeyVaultErrorException kex)
            //{
            //    return req.CreateResponse(HttpStatusCode.NotFound, $”{ kex.Message}”);
            //}
            //log.Info(“Secret Value retrieved from KeyVault.”);

            //var secretResponse = new SecretResponse { Secret = secretRequest.Secret, Value = secretValue.Value };

            //return new HttpResponseMessage(HttpStatusCode.OK)
            //{
            //    Content = new StringContent(JsonConvert.SerializeObject(secretResponse), Encoding.UTF8, “application / json”)
            //};
        }
    }

    internal class SecretResponse
    {
        public string Secret { get; set; }
        public string Value { get; set; }
    }

    internal class SecretRequest
    {
        public string KeyVaultUrl { get; set; }
        public string SecretName { get; set; }
    }
}

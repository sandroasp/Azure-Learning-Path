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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SASP.FunctionApps.GetKeyVaultSecrets
{
    public static class GetKeyVaultSecrets
    {
        [FunctionName("GetKeyVaultSecrets")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            SecretsRequest data = JsonConvert.DeserializeObject<SecretsRequest>(requestBody);


            // Create a new secret client using the default credential from Azure.Identity using environment variables previously set,
            // including AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, and AZURE_TENANT_ID.
            var client = new SecretClient(vaultUri: new Uri(data.KeyVaultUrl), credential: new DefaultAzureCredential());

            KeyVaultSecret secret;
            var response = new JObject();

            foreach (SecretNamesRequest kvSecret in data.SecretNames)
            {
                // Retrieve a secret using the secret client.
                secret = client.GetSecret(kvSecret.SecretName);
                //var item = new JProperty(kvSecret.SecretName, secret.Value);
                //item.Add(new JProperty(kvSecret.SecretName, secret.Value));
                response.Add(new JProperty(kvSecret.SecretName, secret.Value));
            }

            return new JsonResult(response);
        }
    }

    internal class SecretsRequest
    {
        public string KeyVaultUrl { get; set; }
        public List<SecretNamesRequest> SecretNames { get; set; }
    }

    internal class SecretNamesRequest
    {
        public string SecretName { get; set; }
    }
}

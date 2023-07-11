using System;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.RabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AF_DVS_RabbitMQ_TRIGGER_dotNET6
{
    public class RabbitMQTrigger
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("RabbitMQTrigger")]
        public async Task RunAsync([RabbitMQTrigger("rabbit_mq_v3", ConnectionStringSetting = "RabbitMQConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            // Prepare the request payload
            var requestPayload = new { Message = myQueueItem };
            var jsonPayload = JsonConvert.SerializeObject(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Set the Logic App endpoint URL
            var logicAppUrl = "<add-url>";

            // Send the HTTP request to the Logic App
            var response = await httpClient.PostAsync(logicAppUrl, httpContent);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                log.LogInformation("Request sent to Logic App successfully.");
            }
            else
            {
                log.LogError($"Failed to send request to Logic App. Status code: {response.StatusCode}");
            }
        }
    }
}

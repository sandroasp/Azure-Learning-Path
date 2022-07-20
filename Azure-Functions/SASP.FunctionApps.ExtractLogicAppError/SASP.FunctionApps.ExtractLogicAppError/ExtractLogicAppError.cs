using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using System.Web;

namespace SASP.FunctionApps.ExtractLogicAppError
{
    public static class ExtractLogicAppError
    {
        //static readonly string logicAppHistory = @"https://management.azure.com/subscriptions/{subId}/resourceGroups/{rgName}/providers/Microsoft.Logic/workflows/{laName}/runs/{runId}/actions?api-version=2016-06-01"
        private static System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();

        [FunctionName("ExtractLogicAppError")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                LAContext data = JsonConvert.DeserializeObject<LAContext>(requestBody);

                #region Configurations

                string logicAppHistory = System.Environment.GetEnvironmentVariable("logicAppHistory", EnvironmentVariableTarget.Process);
                string defaultSubscriptionId = System.Environment.GetEnvironmentVariable("defaultSubscriptionId", EnvironmentVariableTarget.Process);
                string defaultResourceGroup = System.Environment.GetEnvironmentVariable("defaultResourceGroup", EnvironmentVariableTarget.Process);

                #region Set Management Azure Logic App Historic URL 

                if (string.IsNullOrEmpty(data.SubscriptionId))
                    logicAppHistory = logicAppHistory.Replace("{subId}", defaultSubscriptionId);
                logicAppHistory = logicAppHistory.Replace("{subId}", data.SubscriptionId);

                if (string.IsNullOrEmpty(data.ResourceGroup))
                    logicAppHistory = logicAppHistory.Replace("{rgName}", defaultResourceGroup);
                logicAppHistory = logicAppHistory.Replace("{rgName}", data.ResourceGroup);

                logicAppHistory = logicAppHistory.Replace("{laName}", data.LogicAppName);
                logicAppHistory = logicAppHistory.Replace("{runId}", data.Runid);

                #endregion

                #endregion

                var target = "https://management.azure.com";
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(target);

                var logicAppRunRequest = new HttpRequestMessage(HttpMethod.Get, logicAppHistory);
                logicAppRunRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var result = await _client.SendAsync(logicAppRunRequest);

                var azureResponseBody = await result.Content.ReadAsStringAsync();
                var azureResponse = JObject.Parse(azureResponseBody);

                var response = new JArray();
                foreach (var action in azureResponse["value"])
                {
                    if (action["properties"] != null)
                    {
                        if (action["properties"]["status"].ToString() == "Failed")
                        {
                            if (action["properties"]["error"] == null)
                            {
                                var item = new JObject();
                                item.Add(new JProperty("name", action["name"]));
                                item.Add(new JProperty("type", action["type"]));
                                item.Add(new JProperty("status", action["properties"]["status"]));
                                item.Add(new JProperty("code", action["properties"]["code"]));
                                item.Add(new JProperty("startTime", action["properties"]["startTime"]));
                                item.Add(new JProperty("endTime", action["properties"]["endTime"]));

                                log.LogInformation(action["properties"]["outputsLink"]["uri"].ToString());

                                using (var client = new HttpClient())
                                {
                                    string urlAction = action["properties"]["outputsLink"]["uri"].ToString();
                                    log.LogInformation(urlAction);
                                    var actionResponse = await client.GetAsync(new Uri(action["properties"]["outputsLink"]["uri"].ToString(), UriKind.Absolute));

                                    var actionResult = await actionResponse.Content.ReadAsStringAsync();
                                    item.Add(new JProperty("errorMessage", actionResult.ToString()));
                                }

                                response.Add(item);
                            }
                            else
                            {
                                if (action["properties"]["error"]["message"].ToString() != "An action failed. No dependent actions succeeded.")
                                {
                                    var item = new JObject();
                                    item.Add(new JProperty("name", action["name"]));
                                    item.Add(new JProperty("type", action["type"]));
                                    item.Add(new JProperty("status", action["properties"]["status"]));
                                    item.Add(new JProperty("code", action["properties"]["code"]));
                                    item.Add(new JProperty("startTime", action["properties"]["startTime"]));
                                    item.Add(new JProperty("endTime", action["properties"]["endTime"]));
                                    item.Add(new JProperty("errorMessage", action["properties"]["error"]["message"]));
                                    response.Add(item);
                                }
                            }
                        }
                    }
                }

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new JArray();
                var item = new JObject();
                item.Add(new JProperty("name", "ExtractLogicAppError function"));
                item.Add(new JProperty("type", "Internal Error"));
                item.Add(new JProperty("status", "Failed"));
                item.Add(new JProperty("code", "500"));
                item.Add(new JProperty("startTime", DateTime.Now.ToString()));
                item.Add(new JProperty("endTime", DateTime.Now.ToString()));
                item.Add(new JProperty("errorMessage", ex.Message));
                errorResponse.Add(item);
                return new OkObjectResult(errorResponse);
            }
        }
    }

    public class LAContext
    {
        public string Runid { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string LogicAppName { get; set; }
    }
}

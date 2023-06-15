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
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.Linq;

namespace AF.DevScope.JSONValidation
{
    public static class JSONSchemaValidation
    {
        [FunctionName("JSONSchemaValidation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger("post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Read the JSON and schema strings from the input data
            string jsonString = data.json.ToString();
            string jsonSchemaString = data.jsonSchema.ToString();

            // Parse the JSON and schema
            JToken json = JToken.Parse(jsonString);
            JSchema schema = JSchema.Parse(jsonSchemaString);

            // Validate the JSON against the schema
            IList<object> errorsList = new List<object>();
            ValidateProperties(json, schema, errorsList);

            if (errorsList.Any())
            {
                return new BadRequestObjectResult(errorsList);
            }
            else
            {
                return new OkResult();
            }
        }

        private static void ValidateProperties(JToken json, JSchema schema, IList<object> errorsList)
        {
            if (json.Type != JTokenType.Object)
            {
                return;
            }

            JObject jsonObject = (JObject)json;

            foreach (JProperty property in jsonObject.Properties())
            {
                if (schema.Properties.TryGetValue(property.Name, out JSchema propertySchema))
                {
                    if (!property.Value.IsValid(propertySchema, out IList<string> errors))
                    {
                        foreach (string error in errors)
                        {
                            errorsList.Add(new Dictionary<string, string>
                            {
                                { "error", error }
                            });
                        }
                    }
                }
            }

            if (schema.If != null && schema.Then != null && schema.Else != null)
            {
                bool isIfValid = json.IsValid(schema.If, out IList<string> ifErrors);

                if (isIfValid)
                {
                    ValidateProperties(json, schema.Then, errorsList);
                }
                else
                {
                    ValidateProperties(json, schema.Else, errorsList);
                }
            }
        }
    }
}

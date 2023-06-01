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
using System.Collections.Generic;
using Newtonsoft.Json.Schema;
using System.Linq;

namespace AF.DevScope.JSONValidation
{
    public static class JSONSchemaValidation
    {
        [FunctionName("JSONSchemaValidation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Lendo o corpo da solicitação
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Lendo o esquema JSON a partir dos dados de entrada
            string jsonString = data.json.ToString();
            string jsonSchemaString = data.jsonSchema.ToString();

            // Parse do objeto JSON e do esquema JSON
            JToken json = JToken.Parse(jsonString);
            JSchema schema = JSchema.Parse(jsonSchemaString);

            // Validando as propriedades do objeto JSON em relação ao esquema
            IList<object> errorsList = new List<object>();

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
                    string expectedType = schema.Type.ToString();
                    string actualType = json.Type.ToString();
                    if (json.Type != JTokenType.Object)
                    {
                        errorsList.Add(new Dictionary<string, string>
                    {
                        { "typeerror", $"Invalid type. Expected: {expectedType}. Found: {actualType}." }
                    });
                    }
                }
            }

            if (errorsList.Any())
            {
                return new BadRequestObjectResult(errorsList);
            }
            else
            {
                return new OkResult();
            }
        }
    }
}
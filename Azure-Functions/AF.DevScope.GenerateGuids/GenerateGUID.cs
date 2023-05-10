using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class GenerateGUID
{
    [FunctionName("GenerateGUID")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string hyphen = req.GetQueryParameterDictionary().FirstOrDefault(q => string.Compare(q.Key, "useHyphen", true) == 0).Value;
        string uppercase = req.GetQueryParameterDictionary().FirstOrDefault(q => string.Compare(q.Key, "useUppercase", true) == 0).Value;
        string braces = req.GetQueryParameterDictionary().FirstOrDefault(q => string.Compare(q.Key, "useBraces", true) == 0).Value;

        bool useHyphen = string.IsNullOrEmpty(hyphen) ? true : Convert.ToBoolean(hyphen);
        bool useUppercase = string.IsNullOrEmpty(uppercase) ? false : Convert.ToBoolean(uppercase); ;
        bool useBraces = string.IsNullOrEmpty(braces) ? false : Convert.ToBoolean(braces); ;

        if (useHyphen)
        {
            Guid hyphenGuid = Guid.NewGuid();
            string finalGuid = "";

            if (useBraces)
                finalGuid = hyphenGuid.ToString("B");
            else finalGuid = hyphenGuid.ToString();

            if (useUppercase)
                return new OkObjectResult(finalGuid.ToUpper());
            return new OkObjectResult(finalGuid);
        }
        else
        {
            Guid withoutHyphenGuid = Guid.NewGuid();
            string finalGuid = withoutHyphenGuid.ToString("N");

            if (useBraces)
                finalGuid = "{" + finalGuid + "}";

            if (useUppercase)
                return new OkObjectResult(finalGuid.ToUpper());
            return new OkObjectResult(finalGuid);
        }
    }
}
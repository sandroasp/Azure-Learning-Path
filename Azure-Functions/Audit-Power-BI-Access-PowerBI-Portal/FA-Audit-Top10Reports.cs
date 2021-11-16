using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SASP.FunctionApps.Samples.NetCore
{
    public static class FA_Monitoring_Top10Reports
    {
        [FunctionName("FA_Monitoring_Top10Reports")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            JArray data = (JArray)JsonConvert.DeserializeObject(requestBody);

            var apiReport = new JArray();

            var groups = data
                .GroupBy(s => s["name"])
                .Select(s => new
                {
                    Dashboard = s.Key,
                    Count = s.Count()
                })
                .OrderByDescending(s=> s.Count).Take(10);

            string htmlOutput = "";

            Random rnd = new Random();
            int rowType = 1;


            foreach (var user in groups)
            {
                log.LogInformation("foreach");
                string apiReportRow = @"
                <tr>
                    <td style='margin-left:10px; margin-top:5px; margin-bottom:5px;'>" + user.Dashboard + @"</td>
                    <td style='margin-left:10px; margin-top:5px; margin-bottom:5px;'>" + user.Count + @"</td>";

                rowType = rnd.Next(1, 3);
                switch (rowType)
                {
                    case 1:
                        apiReportRow = apiReportRow + @"<td align='center'><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAtCAMAAAANxBKoAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAABaUExURfLy8unq79rd6pWf0tDU5z9RtVloveDi7FhovdTX6MfM5LvA37O53MzQ5q6126iw2JOd0ZSd0ejp79/h68bK4298xc/T5qiw2err78nN5bG427vB39PW51hovBt6nZ4AAAAJcEhZcwAADsIAAA7CARUoSoAAAAClSURBVEhL7ZTJDsIwDEQDdGDYy77//2/SJoNQUMG1xAXRd6mneqoix27o+Ad6erajP/D4BYYjlS0oAI5V21Q2OFEwqW1wqmQRbXCmaDCPNrhQfrAsm2CywdVaXqLU+3dwIzFi2XknTRvcSq2wbez2cr/+bde5mU3XQR3Oefb7KO8TJ8mvd9mM5uSsaJBm8KJk4Z5vXlXbFLg59tK3877/ScdPEcIdUOALUii6seYAAAAASUVORK5CYII="" style=""height: 20px; ""></td></tr>";
                        break;
                    case 2:
                        apiReportRow = apiReportRow + @"<td align='center'><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAtCAYAAAA6GuKaAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAI8SURBVGhD7Zi/axRBFIC/MYWxCFooqNiFiAoBG+EKtTGCiaIYbbSxEWwFqyuEgHbiP2CXRkVi/IFJClOZIip2ikFFJCjxMFZnLneXZt23M6t7t7t3cW+zuyf7Ne/dzO3x3WN4j1lVLpctuoxNJnYVuXRS5NJJkUsnxX8i/atskuzSKL34BW7dgJknZiGbNEp/XIDlEjydyLR4o/TxERi9oPMMi/vPdBeI+6WFjIsHSwsZFg+XFqKK//wB43dg6pFZiJeeYrE4ZvJg+gdgcy8svIMP7+0nemBgn9kMQISvX4M3r2DpGygFe/ebzXhoLy2sV9wVLn3HVoWVlQ0R/7fr1vNpmLyn89PnYfiMzgWPMDt3OXuqUsF6Mav3T43CybM675D1VdolrOLNwpeuwLETMHgQerfY33+rB1dMFY92sfVWvHAEXs41ChcO6z2X2Rl4eFfnMVQ8+m3ciCtb1vmBMGGXGMU7e4XgrXgrYZeYxDt+76Hm57C2boMDg2alDTGIdywdiVbi9TrcH4ftO0L/UOuJuFEMDcO5izp/Nvl3corw1cvw+IHdkZb1WgDpSAvN4jL2RdgdTC1IT1qwxdXRIS0q1TXC7c5rutLCocIfUW90en+tJpmPdKXtM2zdvukTdqJUXSZtAOlKS5fwHInmyNqaE5pJT/r1PHz+FC5sT1irr08++kinTxtUaQm+LsoxsazViqJWtahWlarXsHbv0R0mgFSlo5J+94hALp0UuXRS5NJJkUsnRRdKw28IgvpksZtClwAAAABJRU5ErkJggg=="" style=""height: 20px; ""></td></tr>";
                        break;
                    case 3:
                        apiReportRow = apiReportRow + @"<td align='center'><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAtCAYAAAA6GuKaAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAI8SURBVGhD7Zi/axRBFIC/MYWxCFooqNiFiAoBG+EKtTGCiaIYbbSxEWwFqyuEgHbiP2CXRkVi/IFJClOZIip2ikFFJCjxMFZnLneXZt23M6t7t7t3cW+zuyf7Ne/dzO3x3WN4j1lVLpctuoxNJnYVuXRS5NJJkUsnxX8i/atskuzSKL34BW7dgJknZiGbNEp/XIDlEjydyLR4o/TxERi9oPMMi/vPdBeI+6WFjIsHSwsZFg+XFqKK//wB43dg6pFZiJeeYrE4ZvJg+gdgcy8svIMP7+0nemBgn9kMQISvX4M3r2DpGygFe/ebzXhoLy2sV9wVLn3HVoWVlQ0R/7fr1vNpmLyn89PnYfiMzgWPMDt3OXuqUsF6Mav3T43CybM675D1VdolrOLNwpeuwLETMHgQerfY33+rB1dMFY92sfVWvHAEXs41ChcO6z2X2Rl4eFfnMVQ8+m3ciCtb1vmBMGGXGMU7e4XgrXgrYZeYxDt+76Hm57C2boMDg2alDTGIdywdiVbi9TrcH4ftO0L/UOuJuFEMDcO5izp/Nvl3corw1cvw+IHdkZb1WgDpSAvN4jL2RdgdTC1IT1qwxdXRIS0q1TXC7c5rutLCocIfUW90en+tJpmPdKXtM2zdvukTdqJUXSZtAOlKS5fwHInmyNqaE5pJT/r1PHz+FC5sT1irr08++kinTxtUaQm+LsoxsazViqJWtahWlarXsHbv0R0mgFSlo5J+94hALp0UuXRS5NJJkUsnRRdKw28IgvpksZtClwAAAABJRU5ErkJggg=="" style=""height: 20px; ""></td></tr>";
                        break;
                    default:
                        apiReportRow = apiReportRow + @"<td align='center'><img src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAC0AAAAtCAMAAAANxBKoAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAABaUExURfLy8unq79rd6pWf0tDU5z9RtVloveDi7FhovdTX6MfM5LvA37O53MzQ5q6126iw2JOd0ZSd0ejp79/h68bK4298xc/T5qiw2err78nN5bG427vB39PW51hovBt6nZ4AAAAJcEhZcwAADsIAAA7CARUoSoAAAAClSURBVEhL7ZTJDsIwDEQDdGDYy77//2/SJoNQUMG1xAXRd6mneqoix27o+Ad6erajP/D4BYYjlS0oAI5V21Q2OFEwqW1wqmQRbXCmaDCPNrhQfrAsm2CywdVaXqLU+3dwIzFi2XknTRvcSq2wbez2cr/+bde5mU3XQR3Oefb7KO8TJ8mvd9mM5uSsaJBm8KJk4Z5vXlXbFLg59tK3877/ScdPEcIdUOALUii6seYAAAAASUVORK5CYII="" style=""height: 20px; ""></td></tr>";
                        break;
                }

                htmlOutput = (htmlOutput + apiReportRow);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(htmlOutput));
            //response.Content.Headers.ContentType = "application/json";
            return response;
        }
    }
}


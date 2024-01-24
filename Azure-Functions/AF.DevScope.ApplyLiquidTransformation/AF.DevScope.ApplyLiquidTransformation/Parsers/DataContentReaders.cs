using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DotLiquid;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace AF.DevScope.ApplyLiquidTransformation.Readers
{
    public class XMLDataReader : IDataContentReader
    {
        public XMLDataReader()
        {
        }

        public async Task<Hash> ParseRequestAsync(string requestBody, string delimiterChar)
        {
            var transformInput = new Dictionary<string, object>();

            var xDoc = XDocument.Parse(requestBody);
            var json = JsonConvert.SerializeXNode(xDoc);

            // Convert the XML converted JSON to an object tree of primitive types
            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(json, new DictionaryConverter());

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            transformInput.Add("content", requestJson);

            return Hash.FromDictionary(transformInput);
        }
    }

    public class CSVDataReader : IDataContentReader
    {
        public async Task<Hash> ParseRequestAsync(string requestBody, string delimiterChar)
        {
            var transformInput = new Dictionary<string, object>();

            List<object[]> csv = new List<object[]>();

            foreach(string line in requestBody.Split("\r\n"))
                csv.Add(line.Split(delimiterChar));

            transformInput.Add("content", csv.ToArray<object>());

            return Hash.FromDictionary(transformInput);
        }
    }
}

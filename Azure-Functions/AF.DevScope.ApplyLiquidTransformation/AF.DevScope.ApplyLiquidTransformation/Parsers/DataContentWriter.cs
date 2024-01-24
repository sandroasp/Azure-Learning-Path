using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AF.DevScope.ApplyLiquidTransformation.Readers
{
    public class GenericDataWriter : IDataContentWriter
    {
        string _contentType;

        public GenericDataWriter(string contentType)
        {
            _contentType = contentType;
        }

        public StringContent CreateResponse(string output)
        {
            return new StringContent(output, Encoding.UTF8, _contentType);
        }
    }
}
using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AF.DevScope.ApplyLiquidTransformation.Readers
{
    public interface IDataContentReader
    {
        Task<Hash> ParseRequestAsync(string requestBody, string delimiterChar);
    }

    public interface IDataContentWriter
    {
        StringContent CreateResponse(string output);
    }
}

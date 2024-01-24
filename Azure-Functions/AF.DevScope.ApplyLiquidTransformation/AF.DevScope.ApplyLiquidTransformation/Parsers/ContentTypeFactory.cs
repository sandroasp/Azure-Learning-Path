using AF.DevScope.ApplyLiquidTransformation.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AF.DevScope.ApplyLiquidTransformation
{
    public static class ContentTypeFactory
    {
        public static IDataContentReader GetContentReader(string contentType)
        {
            switch (contentType)
            {
                case "application/xml":
                case "text/xml":
                    return new XMLDataReader();
                case "text/csv":
                    return new CSVDataReader();
                default:
                    return new XMLDataReader();
            }
        }

        public static IDataContentWriter GetContentWriter(string contentType)
        {
            return new GenericDataWriter(contentType);

            #region old
            //switch (contentType)
            //{
            //    case "application/xml":
            //    case "text/xml":
            //        return new GenericDataWriter(contentType);
            //    default:
            //        return new GenericDataWriter(contentType);
            //}
            #endregion
        }
    }
}
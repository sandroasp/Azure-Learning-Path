﻿using DotLiquid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AF.DevScope.ApplyLiquidTransformation
{
    public static class CustomFilters
    {
        public static string Padleft(Context context, string input, int totalWidth, string padChar = " ")
        {
            return input.PadLeft(totalWidth, padChar[0]);
        }

        public static string Padright(Context context, string input, int totalWidth, string padChar = " ")
        {
            return input.PadRight(totalWidth, padChar[0]);
        }

        public static string Nullifnull(Context context, string input)
        {
            return string.IsNullOrEmpty(input) ? "null" : input;
        }

        public static double Parsedouble(Context context, string input)
        {
            return Double.Parse(input);
        }

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.Default
        };

        private static JsonSerializerSettings jsonNoHtmlSettings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml
        };

        public static string Json(Context context, dynamic input)
        {
            return JsonConvert.SerializeObject(input, jsonSettings);
        }

        public static string Json_nohtml(Context context, dynamic input)
        {
            return JsonConvert.SerializeObject(input, jsonNoHtmlSettings);
        }
    }
}

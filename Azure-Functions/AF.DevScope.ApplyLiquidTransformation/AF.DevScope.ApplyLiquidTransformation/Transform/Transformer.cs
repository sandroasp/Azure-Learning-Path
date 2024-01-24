using AF.DevScope.ApplyLiquidTransformation.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AF.DevScope.ApplyLiquidTransformation
{
    public class Transformer
    {
        public readonly string Template;
        public readonly bool UseRubyNamingConvention;
        public readonly DotLiquid.Template LiquidTemplate;

        public static Transformer SetLiquidTransformerMap(string template, bool useRubyNamingConvention = false)
        {
            return new Transformer(template, useRubyNamingConvention);
        }

        public Transformer(string template, bool useRubyNamingConvention = false)
        {
            Template = template;
            UseRubyNamingConvention = useRubyNamingConvention;
            if (!useRubyNamingConvention)
            {
                DotLiquid.Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            }
            LiquidTemplate = DotLiquid.Template.Parse(template);

        }

        public string RenderFromString(string content, string rootElement = null)
        {
            Dictionary<string, object> dicContent;
            Dictionary<string, object> finalContent;
            JsonSerializerSettings sets = new JsonSerializerSettings
            {
                CheckAdditionalContent = true,
                MaxDepth = 10
            };
            var jo = JObject.Parse(content);
            var dic = jo.ToDictionary();

            //var dic = jo.ToObject<Dictionary<string, object>>();   

            if (rootElement is null)
            {
                finalContent = (Dictionary<string, object>)dic;
            }
            else
            {
                finalContent = new Dictionary<string, object>
            {
                { rootElement, dic }
            };
            }
            var obj = DotLiquid.Hash.FromDictionary(finalContent);
            return LiquidTemplate.Render(obj);
        }
    }
}
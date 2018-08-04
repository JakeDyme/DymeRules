using DymeInferenceEngine.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonEasyRule.Models
{
    public class JsonPathWorldAnalyser : IWorldAnalyser
    {
        //public bool AttributeExists<WorldType>(string key, WorldType world)
        //{
        //    throw new NotImplementedException();
        //}

        public IEnumerable<KeyValuePair<string, string>> GetAttributesAndValues<WorldType>(WorldType world)
        {
            JObject actualWorld = (world.GetType() == typeof(JObject) ? world as JObject : JObject.Parse(world as string));
            IEnumerable<JToken> jTokens = actualWorld.Descendants().Where(p => p.Count() == 0);
            return jTokens.Select(jt => new KeyValuePair<string, string>(jt.Path, jt.ToString())).ToList();
        }
    }
}

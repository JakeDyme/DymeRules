using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonToDymeWorldParser
{
    public class JsonDymeWorldParser
    {
		public Dictionary<string,string> ParseJson(string json)
		{
			JObject jsonObject = JObject.Parse(json);
			IEnumerable<JToken> jTokens = jsonObject.Descendants().Where(p => p.Count() == 0);
			Dictionary<string, string> results = jTokens.Aggregate(new Dictionary<string, string>(), (properties, jToken) =>
			{
				properties.Add(jToken.Path, jToken.ToString());
				return properties;
			});
			return results;
		}
    }
}

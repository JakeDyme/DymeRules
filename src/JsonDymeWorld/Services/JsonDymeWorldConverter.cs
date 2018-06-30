using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace JsonDymeWorld.Services
{
    public class JsonDymeWorldConverter
    {
		public Dictionary<string,string> ConvertJsonToDymeWorld(string json)
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

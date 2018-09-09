using DymeRuleEngine.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace JsonEasyRule.Models
{
    public class JsonPathWorldReader : IWorldReader
    {
        public IEnumerable<string> GetValueFromWorld<WorldType>(string queryString, WorldType world)
        {
            JObject worldAsType = null;

            if (world.GetType() == typeof(JObject))
                worldAsType = world as JObject;
            else if (world.GetType() == typeof(string))
                worldAsType = JObject.Parse(world as string);
            else
                throw new Exception("Unknown world type, must be string or JObject");
            var resultToken = worldAsType.SelectToken(queryString);
            
            if (resultToken.Type == JTokenType.Array)
                return resultToken.Values<string>();

            return new[] { resultToken.ToString() };
        }
    }
}

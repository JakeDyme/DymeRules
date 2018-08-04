using DymeRuleEngine.Contracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonEasyRule.Models
{
    public class JsonPathWorldReader : IWorldReader
    {
        public string GetValueFromWorld<WorldType>(string queryString, WorldType world)
        {
            JObject worldAsType = null;

            if (world.GetType() == typeof(JObject))
                worldAsType = world as JObject;
            else if (world.GetType() == typeof(string))
                worldAsType = JObject.Parse(world as string);
            else
                throw new Exception("Unknown world type, must be string or JObject");
            return worldAsType.SelectToken(queryString).ToString();
        }
    }
}

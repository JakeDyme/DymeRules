using DymeInferenceEngine.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace DymeInferenceEngine.Models
{
    public class DictionaryWorldAnalyser : IWorldAnalyser
    {
        //public bool AttributeExists<WorldType>(string key, WorldType world)
        //{
        //    return WorldAsDictionary(world)
        //        .ContainsKey(key);
        //}

        public IEnumerable<KeyValuePair<string, string>> GetAttributesAndValues<WorldType>(WorldType world)
        {
            return WorldAsDictionary(world)
                .Select(a => new KeyValuePair<string, string>(a.Key, a.Value));
        }

        private Dictionary<string, string> WorldAsDictionary<WorldType>(WorldType world)
        {
            return world as Dictionary<string, string>;
        }
    }
}
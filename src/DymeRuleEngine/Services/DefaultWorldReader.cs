using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeRuleEngine.Services
{
    public class DictionaryWorldReader : IWorldReader
    {
        public string GetValueFromWorld<WorldType>(string queryString, WorldType world)
        {
            return WorldAsDictionary(world)[queryString];
        }

        private Dictionary<string, string> WorldAsDictionary<WorldType>(WorldType world)
        {
            return world as Dictionary<string,string>;
        }
    }
}

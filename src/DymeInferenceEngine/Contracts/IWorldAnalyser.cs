using DymeRuleEngine.Contracts;
using System.Collections.Generic;

namespace DymeInferenceEngine.Contracts
{
    public interface IWorldAnalyser
    {
        IEnumerable<KeyValuePair<string, string>> GetAttributesAndValues<WorldType>(WorldType world);
    }
}
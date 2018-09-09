using System;
using System.Collections.Generic;

namespace DymeRuleEngine.Contracts
{
    public interface IWorldReader
    {
        IEnumerable<string> GetValueFromWorld<WorldType>(string queryString, WorldType world);
    }
}
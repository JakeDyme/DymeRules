using System;
using System.Collections.Generic;

namespace DymeRuleEngine.Contracts
{
    public interface IEvaluatable
    {
        //bool Evaluate(Dictionary<string, string> stateOfTheWorld);
        bool RelationallyEquivalentTo(IEvaluatable evaluatable);
        string ToFormattedString(Func<IEvaluatable, string> formatFunction);
        string ToString();
        bool Equals(object obj);
        int GetHashCode();
    }

}

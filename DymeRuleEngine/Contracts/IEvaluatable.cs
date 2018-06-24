using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeRuleEngine.Contracts
{
    public interface IEvaluatable
    {
        bool Evaluate(Dictionary<string, string> stateOfTheWorld);
        bool RelationallyEquivalentTo(IEvaluatable evaluatable);
        string ToFormattedString(Func<IEvaluatable, string> formatFunction);
    }

}

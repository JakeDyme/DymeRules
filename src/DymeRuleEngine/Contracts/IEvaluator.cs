using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeRuleEngine.Contracts
{
    public interface IEvaluator
    {
        bool IsTrueIn(IEvaluatable rule, Dictionary<string, string> world);
    }
}

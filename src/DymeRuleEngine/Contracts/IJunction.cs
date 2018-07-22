using System.Collections.Generic;

namespace DymeRuleEngine.Contracts
{
    public interface IJunction
    {
        IEnumerable<IEvaluatable> Arguments { get; set; }
    }
}

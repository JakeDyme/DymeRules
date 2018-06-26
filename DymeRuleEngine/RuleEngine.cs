using DymeRuleEngine.Contracts;
using System.Collections.Generic;

namespace Dyme.RuleEngine
{
	public class RuleEngine
	{
		public bool ValidateRuleAgainstWorld(IEvaluatable rule, Dictionary<string, string> stateOfTheWorld)
		{
			return rule.Evaluate(stateOfTheWorld);
		}
	}
}

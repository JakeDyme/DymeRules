using DymeRuleEngine.Contracts;
using System.Collections.Generic;

namespace DymeRuleEngine.Services
{
	public class DymeRuleEvaluator
	{
		public bool ValidateRuleAgainstWorld(IEvaluatable rule, Dictionary<string, string> stateOfTheWorld)
		{
			return rule.Evaluate(stateOfTheWorld);
		}
	}
}

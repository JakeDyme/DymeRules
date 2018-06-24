using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

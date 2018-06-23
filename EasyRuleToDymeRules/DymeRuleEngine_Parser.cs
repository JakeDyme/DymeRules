using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dyme.RuleEngine;

namespace EasyRule.Dyme
{
	public class EasyRuleDymeParser
	{
		const string _implicationRegex = @"^\s*IF\s+(.+)\s+THEN\s+(.+)\s*$";
		const string _scenarioRegex = @"^\s*(.+)\s+(AND|OR)\s+(.+)\s*$";
		const string _termRegex = @"^\s*\((.+)\)\s+(IS|NOT|GREATER THAN|LESS THAN|CONTAINS|IN)\s+(Setting)*\((.+)\)\s*$";

		public IEvaluatable ParseEvaluatable(string ruleString)
		{
			return GetEvaluatable(ruleString);
		}

		private IEvaluatable GetEvaluatable(string inputString)
		{
			if (IsImplication(inputString))
				return GetImplication(inputString);
			if (IsScenario(inputString))
				return GetScenario(inputString);
			if (IsTerm(inputString))
				return GetTerm(inputString);
			throw new Exception("Syntax error. Cannot determine logic structure");
		}

		private bool IsImplication(string inputString)
		{
			var pattern = new Regex(_implicationRegex);
			return pattern.IsMatch(inputString);
		}

		private bool IsScenario(string inputString)
		{
			var pattern = new Regex(_scenarioRegex);
			return pattern.IsMatch(inputString);
		}

		private bool IsTerm(string inputString)
		{
			var pattern = new Regex(_termRegex);
			return pattern.IsMatch(inputString);
		}

		private IEvaluatable GetImplication(string inputString)
		{
			var pattern = new Regex(_implicationRegex);
			var matches = pattern.Matches(inputString);
            var antecedent = GetEvaluatable(matches[0].Groups[1].Value);
            var consequent = GetEvaluatable(matches[0].Groups[2].Value);
            return new Imply(antecedent, consequent);
		}

		private IEvaluatable GetScenario(string inputString)
		{
			var pattern = new Regex(_scenarioRegex);
			var matches = pattern.Matches(inputString);
			var arguments = new List<IEvaluatable>();
			arguments.Add(GetEvaluatable(matches[0].Groups[1].Value));
			Junction junction;
			Enum.TryParse<Junction>(matches[0].Groups[2].Value, out junction);
			foreach (Match match in matches)
			{
				arguments.Add(GetEvaluatable(match.Groups[3].Value));
			}
            return new Scenario(arguments, junction);
		}
		private IEvaluatable GetTerm(string inputString)
		{
			var pattern = new Regex(_termRegex);
			var matches = pattern.Matches(inputString);
			Predicate relationalOperator;
			Enum.TryParse<Predicate>(matches[0].Groups[2].Value.Replace(' ', '_'), out relationalOperator);
            var attributeName = matches[0].Groups[1].Value;
            var reflective = !string.IsNullOrEmpty(matches[0].Groups[3].Value);
            var attributeValue = matches[0].Groups[4].Value;
            return new Fact(attributeName, relationalOperator, attributeValue, reflective);
		}
	}
}

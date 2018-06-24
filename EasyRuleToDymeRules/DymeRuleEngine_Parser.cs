using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dyme.RuleEngine;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Constructs;
using System.Linq;

namespace EasyRule.Dyme
{
    public class EasyRuleDymeParser
    {
        const string _implyRegex = @"^\s*IF\s+(.+)\s+THEN\s+(.+)\s*$";
        const string _scenarioRegex = @"^\s*(.+)\s+(AND|OR)\s+(.+)\s*$";
        const string _factRegex = @"^\s*\((.+)\)\s+(IS|NOT|GREATER THAN|LESS THAN|CONTAINS|IN)\s+(setting)*\((.+)\)\s*$";

        public IEvaluatable ConvertEasyRuleToDymeRule(string ruleString)
        {
            return GetEvaluatable(ruleString);
        }

        public string ConvertDymeRuleToEasyRule(IEvaluatable dymeRule)
        {
            return dymeRule.ToFormattedString(DymeConstructToFormattedString);
        }

        public string DymeConstructToFormattedString(IEvaluatable construct)
        {
            var constructType = construct.GetType().ToString();
            switch (constructType)
            {
                case nameof(Fact):
                    return FactToEasyRuleFormat(construct);
                case nameof(Scenario):
                    return ScenarioToEasyRuleFormat(construct);
                case nameof(Imply):
                    return ImplyToEasyRuleFormat(construct);
            }
            throw new ArgumentOutOfRangeException();
        }

        private string ImplyToEasyRuleFormat(IEvaluatable construct)
        {
            var implication = construct as Imply;
            return $"IF {implication.Antecedent} THEN {implication.Consequent}";
        }

        private string ScenarioToEasyRuleFormat(IEvaluatable construct)
        {
            var scenario = construct as Scenario;
            return scenario.Arguments
                .Select(x => x.ToFormattedString(DymeConstructToFormattedString))
                .Aggregate((a, b) => $"{a} + {scenario.Junction} + {b}");
        }

        private string FactToEasyRuleFormat(IEvaluatable construct)
        {
            var fact = construct as Fact;
            return $"({fact.AttributeName}) {fact.Operator} {(fact.BinaryArgument?"(setting)":"")} ({fact.AttributeValue})";
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
            var pattern = new Regex(_implyRegex);
            return pattern.IsMatch(inputString);
        }

        private bool IsScenario(string inputString)
        {
            var pattern = new Regex(_scenarioRegex);
            return pattern.IsMatch(inputString);
        }

        private bool IsTerm(string inputString)
        {
            var pattern = new Regex(_factRegex);
            return pattern.IsMatch(inputString);
        }

        private IEvaluatable GetImplication(string inputString)
        {
            var pattern = new Regex(_implyRegex);
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
            var pattern = new Regex(_factRegex);
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

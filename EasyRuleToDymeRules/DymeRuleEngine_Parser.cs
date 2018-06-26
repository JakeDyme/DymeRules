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
        const string _factRegexTemplate = @"^\s*\((.+)\)\s+(#OperatorMapKeys#)\s+(setting)*\((.+)\)\s*$";
        readonly string _implyRegex = @"^\s*IF\s+(.+)\s+THEN\s+(.+)\s*$";
        readonly string _scenarioRegex = @"^\s*(.+)\s+(AND|OR)\s+(.+)\s*$";
        readonly string _factRegex;
        readonly Dictionary<string, Predicate> _operatorMap = new Dictionary<string, Predicate>();

        public EasyRuleDymeParser()
        {
            _operatorMap.Add("IS", Predicate.IS);
            _operatorMap.Add("MUST BE", Predicate.IS);
            _operatorMap.Add("SHOULD BE", Predicate.IS);
            _operatorMap.Add("EQUALS", Predicate.IS);
            _operatorMap.Add("GREATER THAN", Predicate.GREATER_THAN);
            _operatorMap.Add("IS GREATER THAN", Predicate.GREATER_THAN);
            _operatorMap.Add("LESS THAN", Predicate.LESS_THAN);
            _operatorMap.Add("NOT", Predicate.NOT);
            _operatorMap.Add("CONTAINS", Predicate.CONTAINS);
            _operatorMap.Add("IN", Predicate.IN);

            _factRegex = _factRegexTemplate.Replace("#OperatorMapKeys#", _operatorMap.Select(m => m.Key).Aggregate((a, b) => $"{a}|{b}"));
        }
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
            var constructType = construct.GetType().Name.ToString();
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

        private string GetDataType(Type type)
        {
            return type.GetType().ToString();
        }

        private string ImplyToEasyRuleFormat(IEvaluatable construct)
        {
            var implication = construct as Imply;
            return $"IF {implication.Antecedent.ToFormattedString(DymeConstructToFormattedString)} THEN {implication.Consequent.ToFormattedString(DymeConstructToFormattedString)}";
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
            return $"({fact.AttributeName}) {fact.Operator} {(fact.BinaryArgument?"(setting)":"")}({fact.AttributeValue})";
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
            Predicate relationalOperator = _operatorMap[matches[0].Groups[2].Value];
            var attributeName = matches[0].Groups[1].Value;
            var reflective = !string.IsNullOrEmpty(matches[0].Groups[3].Value);
            var attributeValue = matches[0].Groups[4].Value;
            return new Fact(attributeName, relationalOperator, attributeValue, reflective);
        }

    }
}

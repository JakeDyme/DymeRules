using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System.Linq;

namespace EasyRuleDymeRule.Services
{
    public class EasyRuleDymeRuleConverter
    {
        const string _factRegexTemplate = @"^\s*\((.+)\)\s+(#OperatorMapKeys#)\s+(setting)*\((.+)\)\s*$";
        readonly string _implyRegex = @"^\s*(IF|if)\s+(.+)\s+(THEN|then)\s+(.+)\s*$";
        readonly string _conjunctionRegex = @"^\s*(.+)\s+(AND|and)\s+(.+)\s*$";
        readonly string _disjunctionRegex = @"^\s*(.+)\s+(OR|or)\s+(.+)\s*$";
        readonly string _factRegex;
        readonly Dictionary<string, Predicate> _operatorMap = new Dictionary<string, Predicate>();

        public EasyRuleDymeRuleConverter()
        {
            _operatorMap.Add("IS", Predicate.IS);
            _operatorMap.Add("is", Predicate.IS);
            _operatorMap.Add("ARE", Predicate.IS);
            _operatorMap.Add("are", Predicate.IS);
            _operatorMap.Add("MUST BE", Predicate.IS);
            _operatorMap.Add("must be", Predicate.IS);
            _operatorMap.Add("SHOULD BE", Predicate.IS);
            _operatorMap.Add("should be", Predicate.IS);
            _operatorMap.Add("EQUALS", Predicate.IS);
            _operatorMap.Add("equals", Predicate.IS);
            _operatorMap.Add("GREATER THAN", Predicate.GREATER_THAN);
            _operatorMap.Add("greater than", Predicate.GREATER_THAN);
            _operatorMap.Add("IS GREATER THAN", Predicate.GREATER_THAN);
            _operatorMap.Add("is greater than", Predicate.GREATER_THAN);
            _operatorMap.Add("LESS THAN", Predicate.LESS_THAN);
            _operatorMap.Add("less than", Predicate.LESS_THAN);
            _operatorMap.Add("IS LESS THAN", Predicate.LESS_THAN);
            _operatorMap.Add("is less than", Predicate.LESS_THAN);
            _operatorMap.Add("NOT", Predicate.NOT);
            _operatorMap.Add("not", Predicate.NOT);
            _operatorMap.Add("CONTAINS", Predicate.CONTAINS);
            _operatorMap.Add("contains", Predicate.CONTAINS);
            _operatorMap.Add("IN", Predicate.IN);
            _operatorMap.Add("in", Predicate.IN);

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
                case nameof(Proposition):
                    return FactToEasyRuleFormat(construct);
                case nameof(Conjunction):
                    return ConjunctionToEasyRuleFormat(construct);
                case nameof(Disjunction):
                    return DisjunctionToEasyRuleFormat(construct);
                case nameof(Implication):
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
            var implication = construct as Implication;
            return $"IF {implication.Antecedent.ToFormattedString(DymeConstructToFormattedString)} THEN {implication.Consequent.ToFormattedString(DymeConstructToFormattedString)}";
        }

        private string ConjunctionToEasyRuleFormat(IEvaluatable construct)
        {
            var scenario = construct as Conjunction;
            return scenario.Arguments
                .Select(x => x.ToFormattedString(DymeConstructToFormattedString))
                .Aggregate((a, b) => $"{a} AND {b}");
        }

        private string DisjunctionToEasyRuleFormat(IEvaluatable construct)
        {
            var scenario = construct as Disjunction;
            return scenario.Arguments
                .Select(x => x.ToFormattedString(DymeConstructToFormattedString))
                .Aggregate((a, b) => $"{a} OR {b}");
        }

        private string FactToEasyRuleFormat(IEvaluatable construct)
        {
            var fact = construct as Proposition;
            return $"({fact.AttributeName}) {fact.Operator} {(fact.BinaryArgument?"(setting)":"")}({fact.AttributeValue})";
        }

        private IEvaluatable GetEvaluatable(string inputString)
        {
            if (IsImplication(inputString))
                return GetImplication(inputString);
            if (IsConjunction(inputString))
                return GetConjunction(inputString);
            if (IsDisjunction(inputString))
                return GetDisjunction(inputString);
            if (IsTerm(inputString))
                return GetTerm(inputString);
            throw new Exception("Syntax error. Cannot determine logic structure");
        }

        private bool IsImplication(string inputString)
        {
            var pattern = new Regex(_implyRegex);
            return pattern.IsMatch(inputString);
        }

        private bool IsConjunction(string inputString)
        {
            var pattern = new Regex(_conjunctionRegex);
            return pattern.IsMatch(inputString);
        }
        private bool IsDisjunction(string inputString)
        {
            var pattern = new Regex(_disjunctionRegex);
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
            var antecedent = GetEvaluatable(matches[0].Groups[2].Value);
            var consequent = GetEvaluatable(matches[0].Groups[4].Value);
            return new Implication(antecedent, consequent);
        }

        private IEvaluatable GetConjunction(string inputString)
        {
            var pattern = new Regex(_conjunctionRegex);
            var matches = pattern.Matches(inputString);
            var arguments = new List<IEvaluatable>();
            arguments.Add(GetEvaluatable(matches[0].Groups[1].Value));

            foreach (Match match in matches)
            {
                arguments.Add(GetEvaluatable(match.Groups[3].Value));
            }
            arguments = distributeArguments<Conjunction>(arguments).ToList();
            return new Conjunction(arguments);
        }

        private IEvaluatable GetDisjunction(string inputString)
        {
            var pattern = new Regex(_disjunctionRegex);
            var matches = pattern.Matches(inputString);
            var arguments = new List<IEvaluatable>();
            arguments.Add(GetEvaluatable(matches[0].Groups[1].Value));
            foreach (Match match in matches)
            {
                arguments.Add(GetEvaluatable(match.Groups[3].Value));
            }
            arguments = distributeArguments<Disjunction>(arguments).ToList();
            return new Disjunction(arguments);
        }

        private IEnumerable<IEvaluatable> distributeArguments<T>(IEnumerable<IEvaluatable> arguments)
        {
            var nonJunctionArguments = arguments.Where(a => !a.GetType().IsAssignableFrom(typeof(T)));
            var junctionArguments = arguments.Where(a => a.GetType().IsAssignableFrom(typeof(T)));
            return junctionArguments
                .SelectMany(a => (a as IJunction).Arguments)
                .Union(nonJunctionArguments);
        }

        private IEvaluatable GetTerm(string inputString)
        {
            var pattern = new Regex(_factRegex);
            var matches = pattern.Matches(inputString);
            Predicate relationalOperator = _operatorMap[matches[0].Groups[2].Value];
            var attributeName = matches[0].Groups[1].Value;
            var reflective = !string.IsNullOrEmpty(matches[0].Groups[3].Value);
            var attributeValue = matches[0].Groups[4].Value;
            return new Proposition(attributeName, relationalOperator, attributeValue, reflective);
        }

    }
}

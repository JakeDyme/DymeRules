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
        const string _regexMatchInnerMostBrackets = @"\(((?:\(??[^\(]*?))\)";
        const string _s = @"(\r\n|\r|\n|\f|\t|\s)";
        readonly string _templateRegexForFact = $@"^{_s}*\((.+)\){_s}+(#OperatorMapKeys#){_s}+(setting)*\((.+)\){_s}*$";
        readonly string _regexMatchImplication = $@"^{_s}*(IF|if){_s}+(.+){_s}+(THEN|then){_s}+(.+){_s}*$";
        readonly string _regexMatchConjunction = $@"^{_s}*(.+){_s}+(AND|and){_s}+(.+){_s}*$";
        readonly string _regexMatchDisjunction = $@"^{_s}*(.+){_s}+(OR|or){_s}+(.+){_s}*$";
        const string _mapPrefix = "#MAP#";
        const string _mapSuffix = "#";
        readonly string _regexCompressionMapSurrogate = $@"{_mapPrefix}(\d+){_mapSuffix}";
        readonly string _regexMatchFact;
        
        Dictionary<string, string> _argumentCompressionMap = new Dictionary<string, string>();

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
            _operatorMap.Add("IS NOT", Predicate.NOT);
            _operatorMap.Add("is not", Predicate.NOT);
            _operatorMap.Add("CONTAINS", Predicate.CONTAINS);
            _operatorMap.Add("contains", Predicate.CONTAINS);
            _operatorMap.Add("IN", Predicate.IN);
            _operatorMap.Add("in", Predicate.IN);

            _regexMatchFact = _templateRegexForFact.Replace("#OperatorMapKeys#", _operatorMap.Select(m => m.Key).Aggregate((a, b) => $"{a}|{b}"));
        }
        public IEvaluatable ConvertEasyRuleToDymeRule(string ruleString)
        {
            var compressedString = CompressString(ruleString);
            return GetEvaluatable(compressedString);
        }

        private bool HasParenthesis(string inputString)
        {
            var pattern = new Regex(_regexMatchInnerMostBrackets);
            return pattern.IsMatch(inputString);
        }

        private string CompressString(string inputString)
        {
            var compressedString = inputString;
            var pattern = new Regex(_regexMatchInnerMostBrackets);
            var matches = pattern.Matches(inputString);
            while (HasParenthesis(compressedString))
            {
                foreach (Match match in matches)
                {
                    var capture = match.Groups[0].Value;
                    compressedString = MapAndReplaceStringWithSurrogate(compressedString, capture);
                }
                matches = pattern.Matches(compressedString);
            }
            return MapAndReplaceStringWithSurrogate(compressedString, compressedString);
        }

        private string MapAndReplaceStringWithSurrogate(string inputString, string stringToReplace)
        {
            var nextMappingSurrogate = $"{_mapPrefix}{_argumentCompressionMap.Count()}{_mapSuffix}";
            _argumentCompressionMap.Add(nextMappingSurrogate, stringToReplace);
            var augmentedString = inputString.Replace(stringToReplace, nextMappingSurrogate);
            return augmentedString;
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
            //inputString = UncompressString(inputString);
            if (IsImplication(inputString))
                return GetImplication(inputString);
            if (IsConjunction(inputString))
                return GetConjunction(inputString);
            if (IsDisjunction(inputString))
                return GetDisjunction(inputString);
            if (IsTerm(inputString))
                return GetTerm(inputString);
            if (HasCompression(inputString))
                return GetEvaluatable(DecompressStringLevel(inputString));
            throw new Exception("Syntax error. Cannot determine logic structure");
        }

        private bool HasCompression(string inputString)
        {
            var pattern = new Regex(_regexCompressionMapSurrogate);
            return pattern.IsMatch(inputString);
        }

        private bool IsImplication(string inputString)
        {
            var pattern = new Regex(_regexMatchImplication);
            return pattern.IsMatch(inputString);
        }

        private bool IsConjunction(string inputString)
        {
            var pattern = new Regex(_regexMatchConjunction);
            return pattern.IsMatch(inputString);
        }
        private bool IsDisjunction(string inputString)
        {
            var pattern = new Regex(_regexMatchDisjunction);
            return pattern.IsMatch(inputString);
        }
        private bool IsTerm(string inputString)
        {
            var pattern = new Regex(_regexMatchFact);
            return pattern.IsMatch(inputString);
        }

        private IEvaluatable GetImplication(string inputString)
        {
            var pattern = new Regex(_regexMatchImplication);
            var matches = pattern.Matches(inputString);
            var antecedent = GetEvaluatable(matches[0].Groups[4].Value);
            var consequent = GetEvaluatable(matches[0].Groups[8].Value);
            return new Implication(antecedent, consequent);
        }

        private IEvaluatable GetConjunction(string inputString)
        {
            var pattern = new Regex(_regexMatchConjunction);
            var matches = pattern.Matches(inputString);
            var arguments = new List<IEvaluatable>();
            arguments.Add(GetEvaluatable(matches[0].Groups[2].Value));

            foreach (Match match in matches)
            {
                arguments.Add(GetEvaluatable(match.Groups[6].Value));
            }
            arguments = distributeArguments<Conjunction>(arguments).ToList();
            return new Conjunction(arguments);
        }

        private IEvaluatable GetDisjunction(string inputString)
        {
            var pattern = new Regex(_regexMatchDisjunction);
            var matches = pattern.Matches(inputString);
            var arguments = new List<IEvaluatable>();
            arguments.Add(GetEvaluatable(matches[0].Groups[2].Value));
            foreach (Match match in matches)
            {
                arguments.Add(GetEvaluatable(match.Groups[6].Value));
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
            inputString = DecompressStringFully(inputString);
            var pattern = new Regex(_regexMatchFact);
            var matches = pattern.Matches(inputString);
            var attributeName = matches[0].Groups[2].Value;
            Predicate relationalOperator = _operatorMap[matches[0].Groups[4].Value];
            var reflective = !string.IsNullOrEmpty(matches[0].Groups[6].Value);
            var attributeValue = matches[0].Groups[7].Value;
            return new Proposition(attributeName, relationalOperator, attributeValue, reflective);
        }

        private string DecompressStringFully(string inputString)
        {
            while (HasCompression(inputString)) inputString = DecompressStringLevel(inputString);
            return inputString;
        }

        private string DecompressStringLevel(string inputString)
        {
            var pattern = new Regex(_regexCompressionMapSurrogate);
            var matches = pattern.Matches(inputString);
            var arguments = new List<IEvaluatable>();
            foreach (Match match in matches)
            {
                var capture = match.Groups[0].Value;
                inputString = inputString.Replace(capture, _argumentCompressionMap[capture]);
            }
            return inputString;
        }


    }
}

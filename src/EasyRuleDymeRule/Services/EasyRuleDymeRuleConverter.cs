﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System.Linq;

namespace EasyRuleDymeRule.Services
{
    public class EasyRuleDymeRuleConverter
    {
        /// Any kind of empty space
        const string _s = @"(\r\n|\r|\n|\f|\t|\s)";

        /// Proposition
        const string _attOpen = @"(";
        const string _attClose = @")";
        const string _valOpen = @"(";
        const string _valClose = @")";
        const string _binaryArgOpen = @"(setting)";
        const string _binaryArgClose = @"";

        readonly Dictionary<string, Quantifier> _quantifierSynonyms = new Dictionary<string, Quantifier>();
        readonly string _attributeSentence = $@"(#Quantifier#)*{Regex.Escape(_attOpen)}(.+){Regex.Escape(_attClose)}";
        readonly string _valueSentence = $@"(#Quantifier#)*({Regex.Escape(_binaryArgOpen)})*{Regex.Escape(_valOpen)}(.+){Regex.Escape(_valClose)}{Regex.Escape(_binaryArgClose)}";
        readonly Dictionary<string, Predicate> _operatorSynonyms = new Dictionary<string, Predicate>();
        readonly string _regexMatchFact_Template = $@"^{_s}*#AttributeSentence#{_s}+(#OperatorMapKeys#){_s}+#ValueSentence#{_s}*$";
        readonly string _regexMatchFact;

        /// Implication, Conjunction, Disjunction
        readonly string _regexMatchImplication = $@"^{_s}*(IF){_s}+(.+){_s}+(THEN){_s}+(.+){_s}*$";
        readonly string _regexMatchConjunction = $@"^{_s}*(.+){_s}+(AND){_s}+(.+){_s}*$";
        readonly string _regexMatchDisjunction = $@"^{_s}*(.+){_s}+(OR){_s}+(.+){_s}*$";

        /// Compression
        const string _regexMatchInnerMostBrackets = @"\(([^\(]*?)\)";
        const string _mapPrefix = "#MAP#";
        const string _mapSuffix = "#";
        readonly string _regexCompressionMapSurrogate = $@"{_mapPrefix}(\d+){_mapSuffix}";
        Dictionary<string, string> _argumentCompressionMap = new Dictionary<string, string>();

        /// Encapsulation
        const string _openEncapsulation = "(";
        const string _closeEncapsulation = ")";
        readonly string _regexMatchEncapsulated = $@"^{_s}*{Regex.Escape(_openEncapsulation)}(.*){Regex.Escape(_closeEncapsulation)}{_s}*$";

        public EasyRuleDymeRuleConverter()
        {
            _operatorSynonyms.Add("IS", Predicate.IS);
            _operatorSynonyms.Add("ARE", Predicate.IS);
            _operatorSynonyms.Add("MUST BE", Predicate.IS);
            _operatorSynonyms.Add("SHOULD BE", Predicate.IS);
            _operatorSynonyms.Add("EQUALS", Predicate.IS);
            _operatorSynonyms.Add("IS EQUAL TO", Predicate.IS);

            _operatorSynonyms.Add("GREATER THAN", Predicate.GREATER_THAN);
            _operatorSynonyms.Add("IS GREATER THAN", Predicate.GREATER_THAN);

            _operatorSynonyms.Add("LESS THAN", Predicate.LESS_THAN);
            _operatorSynonyms.Add("IS LESS THAN", Predicate.LESS_THAN);

            _operatorSynonyms.Add("NOT", Predicate.NOT);
            _operatorSynonyms.Add("IS NOT", Predicate.NOT);

            _operatorSynonyms.Add("CONTAINS", Predicate.CONTAINS);
            _operatorSynonyms.Add("CONTAIN", Predicate.CONTAINS);

            _operatorSynonyms.Add("PART OF", Predicate.PARTOF);
            _operatorSynonyms.Add("IS PART OF", Predicate.PARTOF);

            _quantifierSynonyms.Add("FOR ALL", Quantifier.ALL);
            _quantifierSynonyms.Add("ALL", Quantifier.ALL);
            _quantifierSynonyms.Add("ALL OF", Quantifier.ALL);
            _quantifierSynonyms.Add("EACH", Quantifier.ALL);
            _quantifierSynonyms.Add("EACH ONE OF", Quantifier.ALL);
            _quantifierSynonyms.Add("EVERY", Quantifier.ALL);
            _quantifierSynonyms.Add("EVERY ONE OF", Quantifier.ALL);

            _quantifierSynonyms.Add("ANY", Quantifier.ANY);
            _quantifierSynonyms.Add("ANY OF", Quantifier.ANY);
            _quantifierSynonyms.Add("ANY ONE OF", Quantifier.ANY);
            _quantifierSynonyms.Add("SOME", Quantifier.ANY);
            _quantifierSynonyms.Add("SOME OF", Quantifier.ANY);
            _quantifierSynonyms.Add("ONE OF", Quantifier.ANY);
            _quantifierSynonyms.Add("THERE EXISTS", Quantifier.ANY);
            _quantifierSynonyms.Add("WITHIN", Quantifier.ANY);
            _quantifierSynonyms.Add("IN", Quantifier.ANY);

            _quantifierSynonyms.Add("SINGLE", Quantifier.SINGLE);
            _quantifierSynonyms.Add("ONLY ONE", Quantifier.SINGLE);
            _quantifierSynonyms.Add("ONLY ONE OF", Quantifier.SINGLE);

            _regexMatchFact = _regexMatchFact_Template
                .Replace("#OperatorMapKeys#", _operatorSynonyms.Select(m => m.Key).Aggregate((a, b) => $"{a}|{b}"))
                .Replace("#AttributeSentence#", _attributeSentence)
                .Replace("#ValueSentence#", _valueSentence)
                .Replace("#Quantifier#", _quantifierSynonyms.Select(m => m.Key).Aggregate((a, b) => $"{a}|{b}"));
        }
        public IEvaluatable ConvertEasyRuleToDymeRule(string ruleString)
        {
            var compressedString = CompressString(ruleString);
            return GetEvaluatable(compressedString);
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
                .Select(x => Parenthesize(x, x.ToFormattedString(DymeConstructToFormattedString)))
                .Aggregate((a, b) => $"{a} AND {b}");
        }

        private string DisjunctionToEasyRuleFormat(IEvaluatable construct)
        {
            var scenario = construct as Disjunction;
            return scenario.Arguments
                .Select(x => Parenthesize(x, x.ToFormattedString(DymeConstructToFormattedString)))
                .Aggregate((a, b) => $"{a} OR {b}");
        }

        private string FactToEasyRuleFormat(IEvaluatable construct)
        {
            var fact = construct as Proposition;
            var friendlyOperator = _operatorSynonyms.First(m => m.Value == fact.Operator).Key;
            return $"{_attOpen}{fact.AttributeName}{_attClose} {friendlyOperator} {(fact.BinaryArgument? _binaryArgOpen:"")}{_valOpen}{fact.AttributeValue}{_valClose}{(fact.BinaryArgument ? _binaryArgClose : "")}";
        }

        private string Parenthesize(IEvaluatable construct, string argument)
        {
            if (construct.GetType() != typeof(Proposition))
                return $"{_openEncapsulation}{argument}{_closeEncapsulation}";
            return argument;
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
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchImplication, RegexOptions.IgnoreCase);
            return pattern.IsMatch(inputString);
        }

        private bool IsConjunction(string inputString)
        {
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchConjunction, RegexOptions.IgnoreCase);
            return pattern.IsMatch(inputString);
        }
        private bool IsDisjunction(string inputString)
        {
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchDisjunction, RegexOptions.IgnoreCase);
            return pattern.IsMatch(inputString);
        }
        private bool IsTerm(string inputString)
        {
            var pattern = new Regex(_regexMatchFact, RegexOptions.IgnoreCase);
            return pattern.IsMatch(inputString);
        }

        private IEvaluatable GetImplication(string inputString)
        {
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchImplication, RegexOptions.IgnoreCase);
            var matches = pattern.Matches(inputString);
            var antecedent = GetEvaluatable(matches[0].Groups[4].Value);
            var consequent = GetEvaluatable(matches[0].Groups[8].Value);
            return new Implication(antecedent, consequent);
        }

        private IEvaluatable GetConjunction(string inputString)
        {
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchConjunction, RegexOptions.IgnoreCase);
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
            inputString = RemoveOuterBrackets(inputString);
            var pattern = new Regex(_regexMatchDisjunction, RegexOptions.IgnoreCase);
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


        private IEvaluatable GetTerm(string inputString)
        {
            var pattern = new Regex(_regexMatchFact, RegexOptions.IgnoreCase);
            var matches = pattern.Matches(inputString);
            Quantifier quantifier1 = string.IsNullOrEmpty(matches[0].Groups[2].Value) ? Quantifier.ALL: _quantifierSynonyms[matches[0].Groups[2].Value.ToUpperInvariant()];
            var attributeName = DecompressStringFully(matches[0].Groups[3].Value);
            Predicate relationalOperator = _operatorSynonyms[matches[0].Groups[5].Value.ToUpperInvariant()];
            Quantifier quantifier2 = string.IsNullOrEmpty(matches[0].Groups[7].Value) ? quantifier1 : _quantifierSynonyms[matches[0].Groups[7].Value.ToUpperInvariant()];
            var reflective = !string.IsNullOrEmpty(matches[0].Groups[8].Value);
            var attributeValue = DecompressStringFully(matches[0].Groups[9].Value);
            return new Proposition(attributeName, relationalOperator, attributeValue, reflective, quantifier1, quantifier2);
        }

        private bool IsEncapsulated(string inputString)
        {
            var pattern = new Regex(_regexMatchEncapsulated);
            return pattern.IsMatch(inputString);
        }

        private string RemoveOuterBrackets(string inputString)
        {
            if (!IsEncapsulated(inputString)) return inputString;
            var pattern = new Regex(_regexMatchEncapsulated);
            var matches = pattern.Matches(inputString);
            foreach (Match match in matches)
            {
                return match.Groups[2].Value;
            }
            throw new Exception("Regex irregularity");
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

        private string DecompressStringFully(string inputString)
        {
            while (HasCompression(inputString)) inputString = DecompressStringLevel(inputString);
            return inputString;
        }

        private IEnumerable<IEvaluatable> distributeArguments<T>(IEnumerable<IEvaluatable> arguments)
        {
            var nonJunctionArguments = arguments.Where(a => !a.GetType().IsAssignableFrom(typeof(T)));
            var junctionArguments = arguments.Where(a => a.GetType().IsAssignableFrom(typeof(T)));
            return junctionArguments
                .SelectMany(a => (a as IJunction).Arguments)
                .Union(nonJunctionArguments);
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

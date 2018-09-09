using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeRuleEngine.Services
{
	public class DymeRuleEvaluator: IEvaluator
    {
        IMetricService _metricService;
        IWorldReader _worldInterrogater;

        public DymeRuleEvaluator(IWorldReader worldInterrogator, IMetricService metricService = null) {
            _worldInterrogater = worldInterrogator;
            _metricService = metricService ?? new DefaultMetricService();
        }

        public bool IsTrueIn<World>(IEvaluatable rule, World world)
		{
            return EvaluateAgainst(rule, world);
        }

        private bool EvaluateAgainst<World>(IEvaluatable evaluatable, World world)
        {
            switch (evaluatable.GetType().Name)
            {
                case nameof(Conjunction):
                    return EvaluateConjunction(evaluatable as Conjunction, world);
                case nameof(Implication):
                    return EvaluateImplication(evaluatable as Implication, world);
                case nameof(Disjunction):
                    return EvaluateDisjunction(evaluatable as Disjunction, world);
                case nameof(Proposition):
                    return EvaluateProposition(evaluatable as Proposition, world);
            }
            throw new Exception("Unknown construct");
        }

        public bool EvaluateConjunction<World>(Conjunction conjunction, World world)
        {
            _metricService.IncrementMetric("EvaluateConjunction");
            return !conjunction.Arguments.Any(arg => FoundABadOne(EvaluateAgainst(arg, world)));
        }

        public bool EvaluateImplication<World>(Implication implication, World world)
        {
            _metricService.IncrementMetric("EvaluateImplication");
            if (NotApplicable(implication, world))
                return true;
            return EvaluateAgainst(implication.Consequent, world);
        }

        public bool EvaluateDisjunction<World>(Disjunction disjunction, World world)
        {
            _metricService.IncrementMetric("EvaluateDisjunction");
            return disjunction.Arguments.Any(arg => FoundAWinner(EvaluateAgainst(arg, world)));
        }

        public bool EvaluateProposition<World>(Proposition proposition, World world)
        {
            _metricService.IncrementMetric("EvaluateProposition");
            var actualValues = GetValueFromWorld(world, proposition.AttributeName);
            IEnumerable<string> expectedValues = proposition.BinaryArgument ? GetValueFromWorld(world, proposition.AttributeValue) : new[] { proposition.AttributeValue };
            ValidateDataTypeForOperation(actualValues, proposition.Operator, proposition.AttributeName);
            ValidateDataTypeForOperation(expectedValues, proposition.Operator, proposition.AttributeName);

            return EvaluateQuantifiedOperation(proposition.Argument1Quantifier, actualValues, proposition.Operator, proposition.Argument2Quantifier, expectedValues);
        }

        private bool EvaluateQuantifiedOperation(Quantifier quantifier1, IEnumerable<string> actualValues, Predicate op, Quantifier quantifier2, IEnumerable<string> expectedValues)
        {
            if (quantifier1 == Quantifier.ALL && quantifier2 == Quantifier.ALL)
                return actualValues.All(actualValue => expectedValues.All(expectedValue => EvaluateOperation(actualValue, op, expectedValue)));

            if (quantifier1 == Quantifier.ALL && quantifier2 == Quantifier.ANY)
                return actualValues.All(actualValue => expectedValues.Any(expectedValue => EvaluateOperation(actualValue, op, expectedValue)));

            if (quantifier1 == Quantifier.ALL && quantifier2 == Quantifier.SINGLE)
                return actualValues.All(actualValue => expectedValues.Where(expectedValue => EvaluateOperation(actualValue, op, expectedValue)).Count() == 1);

            if (quantifier1 == Quantifier.ANY && quantifier2 == Quantifier.ALL)
                return actualValues.Any(actualValue => expectedValues.All(expectedValue => EvaluateOperation(actualValue, op, expectedValue)));

            if (quantifier1 == Quantifier.ANY && quantifier2 == Quantifier.ANY)
                return actualValues.Any(actualValue => expectedValues.Any(expectedValue => EvaluateOperation(actualValue, op, expectedValue)));

            if (quantifier1 == Quantifier.ANY && quantifier2 == Quantifier.SINGLE)
                return actualValues.Any(actualValue => expectedValues.Where(expectedValue => EvaluateOperation(actualValue, op, expectedValue)).Count() == 1);

            if (quantifier1 == Quantifier.SINGLE && quantifier2 == Quantifier.ALL)
                return actualValues.Where(actualValue => expectedValues.All(expectedValue => EvaluateOperation(actualValue, op, expectedValue))).Count() == 1;

            if (quantifier1 == Quantifier.SINGLE && quantifier2 == Quantifier.ANY)
                return actualValues.Where(actualValue => expectedValues.Any(expectedValue => EvaluateOperation(actualValue, op, expectedValue))).Count() == 1;

            if (quantifier1 == Quantifier.SINGLE && quantifier2 == Quantifier.SINGLE)
                return actualValues.Where(actualValue => expectedValues.Where(expectedValue => EvaluateOperation(actualValue, op, expectedValue)).Count() == 1).Count() == 1;

            throw new Exception("Invalid proposition quantification");
        }

        private bool EvaluateOperation(string actualValue, Predicate op, string expectedValue)
        {
            if (op == Predicate.IS)
                return (expectedValue == actualValue);
            if (op == Predicate.NOT)
                return (expectedValue != actualValue);
            if (op == Predicate.GREATER_THAN)
                return (Convert.ToDouble(actualValue) > Convert.ToDouble(expectedValue));
            if (op == Predicate.LESS_THAN)
                return (Convert.ToDouble(actualValue) < Convert.ToDouble(expectedValue));
            if (op == Predicate.CONTAINS)
                return (expectedValue.IndexOf(actualValue) > -1);
            if (op == Predicate.PARTOF)
                return (actualValue.IndexOf(expectedValue) > -1);
            throw new Exception("Unexpected relational operator");
        }

        private void ValidateDataTypeForOperation(IEnumerable<string> data, Predicate operation, string query) {

            foreach (var element in data )
            if (MustBeNumberAccordingTo(operation) && !IsNumber(element))
                throw new Exception($"The value must be a number. Query: ({query}) Problem value: ({element})");
        }

        private bool MustBeNumberAccordingTo(Predicate operation) {
            return operation == Predicate.GREATER_THAN
                || operation == Predicate.LESS_THAN;
        }

        private bool IsNumber(string input) {
            double n;
            return double.TryParse(input, out n);
        }

        private IEnumerable<string> GetValueFromWorld<WorldType>(WorldType world, string attributeName)
        {
            _metricService.IncrementMetric("GetValueFromWorld");
            return _worldInterrogater.GetValueFromWorld(attributeName, world);
        }

        private bool NotApplicable<World>(Implication implication, World world)
        {
            return EvaluateAgainst(implication.Antecedent, world) == false;
        }

        private bool FoundABadOne(bool result)
        {
            return result == false;
        }

        private bool FoundAWinner(bool result)
        {
            return result == true;
        }







    }
}

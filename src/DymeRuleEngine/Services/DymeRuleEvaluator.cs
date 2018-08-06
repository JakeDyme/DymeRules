using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
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
            var actualValue= GetValueFromWorld(world, proposition.AttributeName);
            string expectedValue = proposition.BinaryArgument ? GetValueFromWorld(world, proposition.AttributeValue) : proposition.AttributeValue;
            ValidateDataTypeForOperation(actualValue, proposition.Operator, proposition.AttributeName);
            ValidateDataTypeForOperation(expectedValue, proposition.Operator, proposition.AttributeName);

            if (proposition.Operator == Predicate.IS)
                return (expectedValue == actualValue);
            if (proposition.Operator == Predicate.NOT)
                return (expectedValue != actualValue);
            if (proposition.Operator == Predicate.GREATER_THAN)
                return (Convert.ToDouble(actualValue) > Convert.ToDouble(expectedValue));
            if (proposition.Operator == Predicate.LESS_THAN)
                return (Convert.ToDouble(actualValue) < Convert.ToDouble(expectedValue));
            if (proposition.Operator == Predicate.CONTAINS)
                return (expectedValue.IndexOf(actualValue) > -1);
            if (proposition.Operator == Predicate.IN)
                return (actualValue.IndexOf(expectedValue) > -1);
            throw new Exception("Unexpected relational operator");
        }

        private void ValidateDataTypeForOperation(string data, Predicate operation, string query) {

            if (MustBeNumberAccordingTo(operation) && !IsNumber(data))
                throw new Exception($"The value must be a number. Query: ({query}) Problem value: ({data})");
        }
        private bool MustBeNumberAccordingTo(Predicate operation) {
            return operation == Predicate.GREATER_THAN
                || operation == Predicate.LESS_THAN;
        }

        private bool IsNumber(string input) {
            double n;
            return double.TryParse(input, out n);
        }

        private string GetValueFromWorld<WorldType>(WorldType world, string attributeName)
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

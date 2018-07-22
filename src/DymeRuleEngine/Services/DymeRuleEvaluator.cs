using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeRuleEngine.Services
{
	public class DymeRuleEvaluator: IEvaluator
	{
		public bool IsTrueIn(IEvaluatable rule, Dictionary<string, string> world)
		{
            return EvaluateAgainst(rule, world);
        }

        private bool EvaluateAgainst(IEvaluatable evaluatable, Dictionary<string, string> world)
        {
            switch (evaluatable.GetType().Name)
            {
                case nameof(Conjunction):
                    return EvaluateConjunction(evaluatable as Conjunction, world);
                    break;
                case nameof(Implication):
                    return EvaluateImplication(evaluatable as Implication, world);
                    break;
                case nameof(Disjunction):
                    return EvaluateDisjunction(evaluatable as Disjunction, world);
                    break;
                case nameof(Proposition):
                    return EvaluateProposition(evaluatable as Proposition, world);
                    break;
            }
            throw new Exception("Unkonwn construct");
        }

        public bool EvaluateConjunction(Conjunction conjunction, Dictionary<string,string> world)
        {
            return !conjunction.Arguments.Any(arg => FoundABadOne(EvaluateAgainst(arg, world)));
        }

        public bool EvaluateImplication(Implication implication, Dictionary<string, string> world)
        {
            if (NotApplicable(implication, world))
                return true;
            return EvaluateAgainst(implication.Consequent, world);
        }

        public bool EvaluateDisjunction(Disjunction disjunction, Dictionary<string, string> world)
        {
            return disjunction.Arguments.Any(arg => FoundAWinner(EvaluateAgainst(arg, world)));
        }

        public bool EvaluateProposition(Proposition proposition, Dictionary<string, string> world)
        {
            var actualValue= GetValueFromWorld(world, proposition.AttributeName);
            string expectedValue = proposition.BinaryArgument ? GetValueFromWorld(world, proposition.AttributeValue) : proposition.AttributeValue;

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

        private bool NotApplicable(Implication implication, Dictionary<string, string> world)
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

        private bool AttributeExistsInWorld(Dictionary<string, string> world, string attributeName)
        {
            return world.ContainsKey(attributeName);
        }

        private string GetValueFromWorld(Dictionary<string, string> world, string attributeName)
        {
            return world[attributeName];
        }




    }
}

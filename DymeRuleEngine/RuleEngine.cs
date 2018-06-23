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

    [DebuggerDisplay("{ToString()}")]
    public class Fact : IEvaluatable
	{
        //private IMetricsService _metricsService;
        public IAttributeProperties attributeProperties { get; set; }
        public string AttributeName { get; set; }
		public Predicate Operator { get; set; }
		public string AttributeValue { get; set; }
        /// <summary>
        /// A binary argument means that the attribute points to the value of another attribute rather than a static value.
        /// If this property is set to true, then the attribute will automatically use the AttributeValue field as the name of the second attribute, and fetch that attribute's value from the world for comparison.
        /// </summary>
        public bool BinaryArgument { get; set; } = false;

        public Fact() { }
        public Fact(string attributeName, Predicate operation, string attributeValue, bool BinaryArgument = false)
        {
            AttributeName = attributeName;
            Operator = operation;
            AttributeValue = attributeValue;
            this.BinaryArgument = BinaryArgument;
        }

        public static Fact That(string attributeName)
        {
            var newFact = new Fact();
            newFact.AttributeName = attributeName;
            return newFact;
        }

        public Fact Is(string value)
        {
            this.AttributeValue = value;
            return this;
        }
        public Fact IsGreaterThan(string value)
        {
            this.AttributeValue = value;
            return this;
        }
        public Fact IsLessThan(string value)
        {
            this.AttributeValue = value;
            return this;
        }
        public Fact IsNot(string value)
        {
            this.AttributeValue = value;
            return this;
        }
        public Fact Contains(string value)
        {
            this.AttributeValue = value;
            return this;
        }
        public Fact IsIn(string value)
        {
            this.AttributeValue = value;
            return this;
        }

        public bool Evaluate(Dictionary<string, string> world)
        {
            //if (!AttributeExistsInWorld(world, Key)) return true;
            var expectedValue = GetValueFromWorld(world, AttributeName);
            string actualValue = BinaryArgument ? GetValueFromWorld(world, AttributeValue): AttributeValue;

            //_metricsService.IncrementMetric("Rule.Fact.Evaluate", String.Concat("world", AttributeName));
            if (Operator == Predicate.IS)
                return (expectedValue == actualValue);
            if (Operator == Predicate.NOT)
                return (expectedValue != actualValue);
            if (Operator == Predicate.GREATER_THAN)
                return (Convert.ToDouble(expectedValue) > Convert.ToDouble(actualValue));
            if (Operator == Predicate.LESS_THAN)
                return (Convert.ToDouble(expectedValue) < Convert.ToDouble(actualValue));
            if (Operator == Predicate.CONTAINS)
                return (expectedValue.IndexOf(actualValue) > -1);
            if (Operator == Predicate.IN)
                return (actualValue.IndexOf(expectedValue) > -1);
            throw new Exception("Unexpected relational operator");
        }

        private bool AttributeExistsInWorld(Dictionary<string, string> world, string attributeName)
        {
            //_metricsService.IncrementMetric("Rule.Fact.WorldLookup", attributeName);
            return world.ContainsKey(attributeName);
        }

        private string GetValueFromWorld(Dictionary<string, string> world, string attributeName)
        {
            //_metricsService.IncrementMetric("Rule.Fact.WorldLookup", attributeName);
            return world[attributeName];
        }

        public override bool Equals(object obj)
        {
            var inputObject = obj as Fact;
            return inputObject.AttributeName.Equals(AttributeName)
                && inputObject.Operator.Equals(Operator)
                && inputObject.AttributeValue.Equals(AttributeValue)
                && inputObject.BinaryArgument.Equals(BinaryArgument);
        }
        public override string ToString()
        {
            return $"{AttributeName} {Operator} {AttributeValue}";
        }
        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() != typeof(Fact)) return false;
            var fact = evaluatable as Fact;
            return fact.AttributeName == AttributeName;
        }

    }

    [DebuggerDisplay("{ToString()}")]
    public class Imply : IEvaluatable
	{
		public IEvaluatable Antecedent { get; set; }
		public IEvaluatable Consequent { get; set; }

        public static Imply That()
        {
            return new Imply();
        }

        public Imply If (IEvaluatable antecedant) {
            Antecedent = antecedant;
            return this;
        }
        public Imply Then(IEvaluatable consequent)
        {
            Consequent = consequent;
            return this;
        }

        public static Imply Create()
        {
            return new Imply();
        }
        private Imply() { }

        public Imply(IEvaluatable antecedent, IEvaluatable consequent)
        {
            Antecedent = antecedent;
            Consequent = consequent;
        }
        public override bool Equals(object obj)
        {
            var inputObject = obj as Imply;
            return inputObject.Antecedent.Equals(Antecedent)
                && inputObject.Consequent.Equals(Consequent);
        }

        public bool Evaluate(Dictionary<string, string> stateOfTheWorld)
		{
			if (NotApplicapable(stateOfTheWorld))
				return true;
			return Consequent.Evaluate(stateOfTheWorld);
		}

		private bool NotApplicapable(Dictionary<string, string> stateOfTheWorld)
		{
			return Antecedent.Evaluate(stateOfTheWorld) == false;
		}

        public override string ToString()
        {
            return $"IF {Antecedent.ToString()} Then {Consequent.ToString()}";
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() == typeof(Imply))
            {
                var implication = evaluatable as Imply;
                return Antecedent.RelationallyEquivalentTo(implication.Antecedent) && Consequent.RelationallyEquivalentTo(implication.Consequent);
            }
            return false;
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class Scenario : IEvaluatable
	{
		public IEnumerable<IEvaluatable> Arguments { get; set; }
		public Junction Junction { get; set; }
        public Scenario(IEnumerable<IEvaluatable> arguments, Junction junction)
        {
            Arguments = arguments;
            Junction = junction;
        }
		public bool Evaluate(Dictionary<string, string> stateOfTheWorld)
		{
			foreach (var argument in Arguments)
			{
				var result = argument.Evaluate(stateOfTheWorld);
				if (FoundABadOne(result))
					return false;
				if (FoundAWinner(result))
					return true;
			}
			if (Junction == Junction.AND)
				return true;
			if (Junction == Junction.OR)
				return false;
			throw new Exception("Unexpected resolution");
		}

		private bool FoundAWinner(bool result)
		{
			return Junction == Junction.OR && result == true;
		}

		private bool FoundABadOne(bool result)
		{
			return Junction == Junction.AND && result == false;
		}

        public override bool Equals(object obj)
        {
            var inputObject = obj as Scenario;
            return Compare(inputObject.Arguments, Arguments) 
                && inputObject.Junction.Equals(Junction);
        }

        private bool Compare(IEnumerable<IEvaluatable> argumentSet1, IEnumerable<IEvaluatable> argumentSet2)
        {
            return (!argumentSet1.Except(argumentSet2).Any());
        }

        public override string ToString()
        {
            return Arguments.Select(x=>x.ToString()).Aggregate((a, b) => a + $" {Junction} " + b);
        }

        public bool RelationallyEquivalentTo(IEvaluatable evaluatable)
        {
            if (evaluatable.GetType() != typeof(Scenario)) return false;
            var scenario = evaluatable as Scenario;
            foreach (var arg in Arguments)
            {
                if (!ScenarioContainsArgumentRelation(scenario.Arguments, arg)) return false;
            }

            foreach (var arg in scenario.Arguments)
            {
                if (!ScenarioContainsArgumentRelation(Arguments, arg)) return false;
            }

            return true;
        }

        private bool ScenarioContainsArgumentRelation(IEnumerable<IEvaluatable> arguments, IEvaluatable argument)
        {
            foreach (var arg in arguments)
            {
                if (argument.RelationallyEquivalentTo(arg))
                {
                    return true;
                }
            }
            return false;
        }
    }

	public interface IEvaluatable
	{
		bool Evaluate(Dictionary<string, string> stateOfTheWorld);
        bool RelationallyEquivalentTo(IEvaluatable evaluatable);
    }


    public interface IMetricsService
    {
        void IncrementMetric(string metricIdentifier, string data = "");
    }

    public interface IAttributeProperties
    {
        bool BinaryArgument { get; set; }
        string IsCollectionOf { get; set; }
    }

	public enum Predicate
	{
		IS, NOT, GREATER_THAN, LESS_THAN, CONTAINS, IN
	}

	public enum Junction
	{
		AND, OR
	}
}

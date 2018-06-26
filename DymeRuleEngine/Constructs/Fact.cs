using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DymeRuleEngine.Constructs
{

    [DebuggerDisplay("{ToString()}")]
    public class Proposition : IEvaluatable
    {
        //private IMetricsService _metricsService;
        //public IAttributeProperties attributeProperties { get; set; }
        public string AttributeName { get; set; }
        public Predicate Operator { get; set; }
        public string AttributeValue { get; set; }
        /// <summary>
        /// A binary argument means that the attribute points to the value of another attribute rather than a static value.
        /// If this property is set to true, then the attribute will automatically use the AttributeValue field as the name of the second attribute, and fetch that attribute's value from the world for comparison.
        /// </summary>
        public bool BinaryArgument { get; set; } = false;

        public Proposition() { }
        public Proposition(string attributeName, Predicate operation, string attributeValue, bool BinaryArgument = false)
        {
            AttributeName = attributeName;
            Operator = operation;
            AttributeValue = attributeValue;
            this.BinaryArgument = BinaryArgument;
        }



        public bool Evaluate(Dictionary<string, string> world)
        {
            //if (!AttributeExistsInWorld(world, Key)) return true;
            var expectedValue = GetValueFromWorld(world, AttributeName);
            string actualValue = BinaryArgument ? GetValueFromWorld(world, AttributeValue) : AttributeValue;

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
            var inputObject = obj as Proposition;
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
            if (evaluatable.GetType() != typeof(Proposition)) return false;
            var fact = evaluatable as Proposition;
            return fact.AttributeName == AttributeName;
        }

        public string ToFormattedString(Func<IEvaluatable, string> formatFunction)
        {
            return formatFunction(this);
        }
    }
}

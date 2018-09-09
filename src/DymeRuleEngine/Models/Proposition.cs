using DymeRuleEngine.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DymeRuleEngine.Models
{

    [DebuggerDisplay("{ToString()}")]
    public class Proposition : IEvaluatable
    {
        public string AttributeName { get; set; }
        public Predicate Operator { get; set; }
        public string AttributeValue { get; set; }
        /// <summary>
        /// A binary argument means that the attribute points to the value of another attribute rather than a static value.
        /// If this property is set to true, then the attribute will automatically use the AttributeValue field as the name of the second attribute, and fetch that attribute's value from the world for comparison.
        /// </summary>
        public bool BinaryArgument { get; set; } = false;
        public Quantifier Argument1Quantifier { get; set; } = Quantifier.ALL;
        public Quantifier Argument2Quantifier { get; set; } = Quantifier.ALL;

        public Proposition() { }
        public Proposition(
            string attributeName, 
            Predicate operation, 
            string attributeValue, 
            bool binaryArgument = false, 
            Quantifier argument1Quantifier = Quantifier.ALL,
            Quantifier argument2Quantifier = Quantifier.ALL)
        {
            AttributeName = attributeName;
            Operator = operation;
            AttributeValue = attributeValue;
            BinaryArgument = binaryArgument;
            Argument1Quantifier = argument1Quantifier;
            Argument2Quantifier = argument2Quantifier;
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

        public override int GetHashCode()
        {
            return (nameof(Proposition) + ":" + ToString()).GetHashCode();
        }
    }

}

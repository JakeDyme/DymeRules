using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeInferenceEngine.Services
{

    public enum AttributeContinuity { increasing, decreasing }
    public enum AttributeDataType { number, text, boolean }


    public class Attribute
    {
        public string Name { get; set; }
        public int OccurrenceCount { get; set; }
        public ICollection<Option> DiscreteOptions { get; set; }
    }

    public class AttributeProperties
    {
        AttributeContinuity Continuity { get; set; }
        AttributeDataType DataType { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
    }

    public class AttributeProperty
    {
        string PropertyName { get; set; }
        int probability { get; set; }
    }

    public class Option
    {
        public Option(string value)
        {
            Value = value;
            OccurrenceCount = 0;
        }
        public string Value;
        public int OccurrenceCount;
    }

    class AttributesHelper
    {

        private List<Attribute> _attributeDatabase = new List<Attribute>();

        private void UpdateAttributeMetrics(Attribute attribute, Dictionary<string, string> world1)
        {
            var attributeValueInWorld = world1[attribute.Name];
            attribute.OccurrenceCount++;
            var attributeOption = GetSetAttributeOption(attribute, attributeValueInWorld);
            attributeOption.OccurrenceCount++;
        }

        private Option GetSetAttributeOption(Attribute attribute, string attributeValueInWorld)
        {
            var attributeOption = attribute.DiscreteOptions.SingleOrDefault(a => a.Value == attributeValueInWorld);
            if (attributeOption == null)
            {
                attributeOption = new Option(attributeValueInWorld);
                attribute.DiscreteOptions.Add(attributeOption);
            }
            return attributeOption;
        }

        private Attribute GetSetAttribute(string attName)
        {
            var attribute = GetAttributeFromDatabase(attName);
            if (attribute == null)
            {
                attribute = CreateAttributeInDatabase(attName);
            }
            return attribute;
        }

        private Attribute CreateAttributeInDatabase(string attName)
        {
            Attribute newAttribute = CreateAttribute();
            _attributeDatabase.Add(newAttribute);
            return newAttribute;
        }

        private static Attribute CreateAttribute()
        {
            return new Attribute();
        }

        private Attribute GetAttributeFromDatabase(string attName)
        {
            return _attributeDatabase.SingleOrDefault(a => a.Name == attName);
        }

    }
}

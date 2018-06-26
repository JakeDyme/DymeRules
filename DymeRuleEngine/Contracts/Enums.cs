﻿
namespace DymeRuleEngine.Contracts
{
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

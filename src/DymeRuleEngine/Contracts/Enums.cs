
namespace DymeRuleEngine.Contracts
{
    public interface IMetricService
    {
        void IncrementMetric(string metricIdentifier);
    }

    public enum Predicate
    {
        IS, NOT, GREATER_THAN, LESS_THAN, CONTAINS, PARTOF
    }

    public enum Quantifier
    {
        ALL, ANY, SINGLE
    }

    public enum Junction
    {
        AND, OR
    }
}

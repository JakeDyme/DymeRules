using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeRuleEngine.Services
{
    public class DefaultMetricService : IMetricService
    {
        public Dictionary<string, int> metrics = new Dictionary<string, int>();

        public void IncrementMetric(string metricIdentifier)
        {
            if (metrics.ContainsKey(metricIdentifier))
                metrics[metricIdentifier]++;
            else
                metrics.Add(metricIdentifier, 1);
        }
    }
}

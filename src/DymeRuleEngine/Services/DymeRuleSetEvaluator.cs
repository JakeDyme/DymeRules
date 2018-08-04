using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeRuleEngine.Services
{
	public class DymeRuleSetEvaluator
    {
        private readonly IEvaluator _ruleEvaluator;
        IMetricService _metricService;
        public DymeRuleSetEvaluator(IEvaluator ruleEvaluator, IMetricService metricService = null)
        {
            _ruleEvaluator = ruleEvaluator;
            _metricService = metricService ?? new DefaultMetricService();
        }

        public bool IsTrueInAtLeastTwo<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInAtLeastTwo");
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount >= 2;
        }

        public bool IsTrueInAll<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInAll");
            return !worlds.Any(world => !_ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsNotTrueInAll<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsNotTrueInAll");
            return worlds.Any(world => !_ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsTrueInAtLeastOne<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInAtLeastOne");
            return worlds.Any(world => _ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsTrueInOnlyOne<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInOnlyOne");
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount == 1;
        }

        public bool IsTrueInMost<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInMost");
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount/worlds.Count() >= 0.68;
        }

        public bool IsTrueInOnlyFew<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInOnlyFew");
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount / worlds.Count() <= 0.34;
        }

        public IEnumerable<WorldType> IsTrueIn<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueIn");
            return worlds.Where(world => _ruleEvaluator.IsTrueIn(rule, world));
        }

        public IEnumerable<int> IsTrueInWorldsReturnWorldIndexes<WorldType>(IEvaluatable rule, IEnumerable<WorldType> worlds)
        {
            _metricService.IncrementMetric("IsTrueInWorldsReturnWorldIndexes");
            var indexedWorlds = worlds.ToArray();
            var indexes = new List<int>();
            for (var i = 0; i < indexedWorlds.Length; i++)
            {
                var world = indexedWorlds[i];
                if (_ruleEvaluator.IsTrueIn(rule, world))
                    indexes.Add(i);
            }
            return indexes;
        }

    }
}

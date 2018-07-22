using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeRuleEngine.Services
{
	public class DymeRuleSetEvaluator
	{
        private readonly DymeRuleEvaluator _ruleEvaluator;

        public DymeRuleSetEvaluator(DymeRuleEvaluator ruleEvaluator)
        {
            _ruleEvaluator = ruleEvaluator;
        }

        public bool IsTrueInAtLeastTwo(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount >= 2;
        }

        public bool IsTrueInAll(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            return !worlds.Any(world => !_ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsNotTrueInAll(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.Any(world => !_ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsTrueInAtLeastOne(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.Any(world => _ruleEvaluator.IsTrueIn(rule, world));
        }

        public bool IsTrueInOnlyOne(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount == 1;
        }

        public bool IsTrueInMost(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount/worlds.Count() >= 0.68;
        }

        public bool IsTrueInOnlyFew(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            var positiveCount = worlds.Count(world => _ruleEvaluator.IsTrueIn(rule, world));
            return positiveCount / worlds.Count() <= 0.34;
        }

        public IEnumerable<Dictionary<string, string>> IsTrueIn(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.Where(world => _ruleEvaluator.IsTrueIn(rule, world));
        }

        public IEnumerable<int> IsTrueInWorldsReturnWorldIndexes(IEvaluatable rule, IEnumerable<Dictionary<string, string>> worlds)
        {
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

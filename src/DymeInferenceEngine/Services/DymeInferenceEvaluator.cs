using DymeInferenceEngine.Contracts;
using DymeRuleEngine.Services;
using DymeRuleEngine.Models;
using DymeFluentSyntax.Models;
using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DymeInferenceEngine.Services
{


    public class DymeInferenceEvaluator
    {
        private IEvaluator _ruleEngineSvc;
        private IWorldAnalyser _worldAnalyser;
        private IWorldReader _worldReader;
        private DymeRuleSetEvaluator _setEvaluator;

        public DymeInferenceEvaluator(IWorldReader reader, IWorldAnalyser analyser, IMetricService metricService = null)
        {
            _worldAnalyser = analyser;
            _worldReader = reader;
            _ruleEngineSvc = new DymeRuleEvaluator(_worldReader, metricService);
            _setEvaluator = new DymeRuleSetEvaluator(_ruleEngineSvc);
        }

        public IEnumerable<IEvaluatable> GetRulesForWorlds<WorldType>(IEnumerable<WorldType> worlds, InferenceMethod methodType)
        {
            switch (methodType)
            {
                case InferenceMethod.Optimistic:
                    return GetRulesOptimisticallyFromWorlds(worlds);
                case InferenceMethod.Pessimistic:
                    return GetRulesPessimisticallyFromWorlds(worlds);
                default:
                    return GetRulesPessimisticallyFromWorlds(worlds);
            }
        }

        public IEnumerable<IEvaluatable> AsConjunctionGetAllFactsThatRepeatWhenOtherFactsRepeat<WorldType>(IEnumerable<WorldType> worlds, IEnumerable<Proposition> facts)
        {
            var repeatedFactGroupedByWorldIndexes = facts
                .Select(fact => new Tuple<Proposition, IEnumerable<int>>(fact, _setEvaluator.IsTrueInWorldsReturnWorldIndexes(fact, worlds)))
                .Where(factWorlds => factWorlds.Item2.Count() > 1);

            var WhenFactThenFacts_tuple =
                repeatedFactGroupedByWorldIndexes
                .Select((fact1Worlds, x) => new Tuple<Proposition, IEnumerable<Proposition>>(
                    fact1Worlds.Item1,
                    repeatedFactGroupedByWorldIndexes
                        .Where((fact2Worlds, y) => NotTheSameElementAs(y, x) && RepeatsWhenRepeating(fact1Worlds.Item2, fact2Worlds.Item2))
                        .Select(fact2Worlds => fact2Worlds.Item1)
                        ))
                .Where(propPropsTuple => propPropsTuple.Item2.Any());

            var conjunction =
                WhenFactThenFacts_tuple
                .Select(propPropsTuple =>
                    ImplicationHelper.ConvertArgumentsToJunction(propPropsTuple.Item2.Concat(new[] { propPropsTuple.Item1 }), Junction.AND))
                .Distinct();

            return conjunction.ToList();
        }

        private IEnumerable<IEvaluatable> GetRulesOptimisticallyFromWorlds<WorldType>(IEnumerable<WorldType> worlds)
        {
            var allImplicationsFromAllWorlds = worlds.SelectMany(world => GetCartesianImplicationsFromWorld(world));
            var validImplications = allImplicationsFromAllWorlds.Where(imp => RuleIsValidInAllWorlds(worlds, imp));
            return validImplications.ToList();
        }

        private IEnumerable<IEvaluatable> GetRulesPessimisticallyFromDistinctWorlds<WorldType>(IEnumerable<WorldType> worlds)
        {
            var factAndOccurrenceCounts = AllFactAndOccurrenceCounts(worlds);

            var repeatedFactsAndOcurrences = factAndOccurrenceCounts.Where(fc => fc.Item2 > 1);

            var nonConstantFacts = repeatedFactsAndOcurrences
                .Select(factAndOccurrence => factAndOccurrence.Item1)
                .Distinct()
                .GroupBy(fact => fact.AttributeName)
                .Where(factGroup => factGroup.Count() == 1)
                .Select(factGroup => factGroup.Single());

            var implications = AsSimpleImplicationsAllFactsThatRepeatWhenAnotherFactsRepeats(worlds, nonConstantFacts);

            return implications.ToList();
        }

        private IEnumerable<IEvaluatable> GetRulesPessimisticallyFromWorlds<WorldType>(IEnumerable<WorldType> worlds)
        {
            var allDistinctFactsFromAllWorlds = AllAttributesAsDistinctListOfFacts(worlds);

            var nonConstantFacts = AllFactsThatAreNotConstantInAllWorlds(worlds, allDistinctFactsFromAllWorlds);

            var repeatedFacts = AllFactsThatRepeatInMoreThanOneWorld(worlds, nonConstantFacts);

            var implications = AsSimpleImplicationsAllFactsThatRepeatWhenAnotherFactsRepeats(worlds, repeatedFacts);

            return implications.ToList();
        }

        private IEnumerable<IEvaluatable> GetCartesianImplicationsFromWorld<WorldType>(WorldType world)
        {
            var allFactsFromWorld = _worldAnalyser.GetAttributesAndValues(world).Select(attVal => ItsAFact.That(attVal.Key).Is(attVal.Value));
            var factPairs = CreateCartesianPairs(allFactsFromWorld);
            var implications = factPairs.Select(factPair => If.When(factPair.Item1).Then(factPair.Item2));
            return implications;
        }

        private IEnumerable<Tuple<itemType, itemType>> CreateCartesianPairs<itemType>(IEnumerable<itemType> itemList)
        {
            return itemList.SelectMany((item1, x) =>
                itemList
                .Where((item2, y) => NotTheSameElementAs(x, y))
                .Select(item2 => new Tuple<itemType, itemType>(item1, item2)));
        }
        private Implication CreateImplicationBetweenAttributes<WorldType>(string att1Name, string att2Name, WorldType world1)
        {
            var ifSide = CreateUnaryFactFromAttribute(world1, att1Name);
            var thenSide = CreateUnaryFactFromAttribute(world1, att2Name);
            var newRule = new Implication(ifSide, thenSide);
            return newRule;
        }

        private Proposition CreateUnaryFactFromAttribute<WorldType>(WorldType world, string attributeKey)
        {
            return new Proposition(attributeKey, Predicate.IS, _worldReader.GetValueFromWorld(attributeKey, world).Single());
        }

        private IEnumerable<Tuple<string, string>> CreateAttributeCombinations(IEnumerable<string> attNames)
        {
            return attNames.SelectMany(a1 => attNames.Where(a2 => a1 != a2).Select(a2 => new Tuple<string, string>(a1, a2)));
        }

        private IEnumerable<IEvaluatable> GetInvalidRules<WorldType>(IEnumerable<WorldType> worlds, IEnumerable<IEvaluatable> rules)
        {
            var invalidRules = new List<IEvaluatable>();
            var purgeList = new List<IEvaluatable>();
            var ruleList = rules.ToList();
            var index = -1;
            while (++index < ruleList.Count())
            {
                var rule = ruleList[index];
                if (!RuleIsValidInAllWorlds(worlds, rule))
                    invalidRules.Add(rule);
            }
            return invalidRules;
        }
        private IEnumerable<IEvaluatable> AsSimpleImplicationsAllFactsThatRepeatWhenAnotherFactsRepeats<WorldType>(IEnumerable<WorldType> worlds, IEnumerable<Proposition> facts)
        {
            var repeatedFactsGroupedByWorldIndexes = facts
                .Select(fact => new Tuple<Proposition, IEnumerable<int>>(fact, _setEvaluator.IsTrueInWorldsReturnWorldIndexes(fact, worlds)));

            var implications =
                repeatedFactsGroupedByWorldIndexes
                .SelectMany((fact1Worlds_tuple, x) =>
                    repeatedFactsGroupedByWorldIndexes
                    .Where((fact2Worlds_tuple, y) =>
                        NotTheSameElementAs(y, x)
                        && RepeatsWhenRepeating(fact1Worlds_tuple.Item2, fact2Worlds_tuple.Item2))
                    .Select(prop2Worlds => If.When(fact1Worlds_tuple.Item1).Then(prop2Worlds.Item1)));

            return implications;
        }

        private bool RuleIsValidInAllWorlds<WorldType>(IEnumerable<WorldType> worlds, IEvaluatable rule)
        {
            return worlds.All(world => _ruleEngineSvc.IsTrueIn(rule, world));
        }

        private bool RepeatsWhenRepeating(IEnumerable<int> xFactWorldIndexes, IEnumerable<int> yFactWorldIndexes)
        {
            return IsSubsetOf(xFactWorldIndexes, yFactWorldIndexes);
        }

        private bool IsSupersetOf(IEnumerable<int> apparentSuperset, IEnumerable<int> apparentSubset)
        {
            return IsSubsetOf(apparentSubset, apparentSuperset);
        }

        private bool IsSubsetOf(IEnumerable<int> apparentSubset, IEnumerable<int> apparentSuperset)
        {
            return apparentSuperset.Intersect(apparentSubset).Count() == apparentSubset.Count();
        }

        private bool IsSameSetAs(IEnumerable<int> apparentSubset, IEnumerable<int> apparentSuperset)
        {
            return apparentSuperset.Intersect(apparentSuperset).Count() == apparentSubset.Count() && apparentSubset.Count() == apparentSuperset.Count();
        }

        private bool NotTheSameElementAs(int index1, int index2)
        {
            return index1 != index2;
        }
        private IEnumerable<Proposition> AllFactsThatAreNotConstantInAllWorlds<WorldType>(IEnumerable<WorldType> worlds, IEnumerable<Proposition> commonFact)
        {
            return commonFact.Where(p => _setEvaluator.IsNotTrueInAll(p, worlds));
        }
        private IEnumerable<Proposition> AllAttributesAsDistinctListOfFacts<WorldType>(IEnumerable<WorldType> worlds)
        {
            return worlds.SelectMany(world => GetSimpleFactFromWorld(world)).Distinct();
        }
        private IEnumerable<Tuple<Proposition, int>> AllFactAndOccurrenceCounts<WorldType>(IEnumerable<WorldType> worlds)
        {
            var facts = worlds.SelectMany(world => GetSimpleFactFromWorld(world));
            return facts.GroupBy(fact => fact).Select(g => new Tuple<Proposition, int>(g.Key, g.Count()));
        }
        private IEnumerable<Proposition> GetSimpleFactFromWorld<WorldType>(WorldType world)
        {
            var attributes = _worldAnalyser.GetAttributesAndValues<WorldType>(world);
            return attributes.Select(att => new Proposition(att.Key, Predicate.IS, att.Value));
        }
        private IEnumerable<Proposition> AllFactsThatRepeatInMoreThanOneWorld<WorldType>(IEnumerable<WorldType> worlds, IEnumerable<Proposition> fact)
        {
            return fact.Where(proposition => _setEvaluator.IsTrueInAtLeastTwo(proposition, worlds));
        }

    }
}

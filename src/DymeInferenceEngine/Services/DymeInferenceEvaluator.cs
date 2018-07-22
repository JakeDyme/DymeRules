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
        private DymeRuleEvaluator _ruleEngineSvc;
        private DymeRuleSetEvaluator _setEvaluator;

        public DymeInferenceEvaluator()
        {
            _ruleEngineSvc = new DymeRuleEvaluator();
            _setEvaluator = new DymeRuleSetEvaluator(_ruleEngineSvc);
        }

        public IEnumerable<IEvaluatable> GetRulesForWorlds(IEnumerable<Dictionary<string, string>> worlds, PickAttriubutesBy methodType)
        {
            var relevantAttributes = GetRelevantAttributes(worlds, methodType);

            var allRules = new List<IEvaluatable>();
            var invalidRules = new List<IEvaluatable>();
            foreach (var dymeWorld in worlds)
            {
                var newRules = GetCartesianImplicationsFromWorld(relevantAttributes, dymeWorld);
                invalidRules.AddRange(GetInvalidRules(worlds, newRules));
                newRules = newRules.Where(newRule => !allRules.Contains(newRule)).ToList();
                allRules.AddRange(newRules.Except(invalidRules));
            }

            foreach (var invalidRule in invalidRules)
            {
                allRules = RemoveRulesWithEquivalentRelationships(allRules, invalidRule);
            }

            return allRules;
        }

        private IEnumerable<string> GetRelevantAttributes(IEnumerable<Dictionary<string,string>> worlds, PickAttriubutesBy methodType)
        {
            switch (methodType)
            {
                case PickAttriubutesBy.WorldDeltas:
                    return GetAttributesBaseOnSimilarity(worlds, false);
                case PickAttriubutesBy.WorldMatching:
                    return GetAttributesBaseOnSimilarity(worlds, true);
                case PickAttriubutesBy.World:
                    return GetAllAttributes(worlds);
            }
            throw new ArgumentOutOfRangeException();
        }

        private IEnumerable<IEvaluatable> GetInvalidRules(IEnumerable<Dictionary<string, string>> worlds, IEnumerable<IEvaluatable> rules)
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

        private List<IEvaluatable> RemoveRulesWithEquivalentRelationships(IEnumerable<IEvaluatable> ruleSet, IEvaluatable ruleWithRelation)
        {
            var satisfactoryRules = new List<IEvaluatable>();
            foreach (var ruleFromRuleSet in ruleSet)
            {
                if (!ruleFromRuleSet.RelationallyEquivalentTo(ruleWithRelation))
                    satisfactoryRules.Add(ruleFromRuleSet);
            }
            return satisfactoryRules;
        }

        private bool RuleIsValidInAllWorlds(IEnumerable<Dictionary<string, string>> worlds, IEvaluatable rule)
        {
            foreach (var world in worlds)
            {
                if (!_ruleEngineSvc.IsTrueIn(rule, world))
                    return false;
            }
            return true;
        }

        private static List<IEvaluatable> GetValidatedRulesForWorld(IEnumerable<IEvaluatable> newRules, Dictionary<string, string> world)
        {
            var ruleEngine = new DymeRuleEvaluator();
            var validRules = new List<IEvaluatable>();
            foreach (var rule in newRules)
            {
                if (ruleEngine.IsTrueIn(rule, world))
                    validRules.Add(rule);
            }
            return validRules;
        }

        private IEnumerable<IEvaluatable> CreateNewRulesFromWorld(Dictionary<string, string> world, IEnumerable<string> attributeNames, BuildRulesFromAttributesUsing methodType)
        {
            switch (methodType)
            {
                case BuildRulesFromAttributesUsing.CartesianImplication:
                    return GetCartesianImplicationsFromWorld(attributeNames, world);
                case BuildRulesFromAttributesUsing.ByWorldDeltas:
                    return GetCartesianImplicationsFromWorld(attributeNames, world);
                default:
                    return GetCartesianImplicationsFromWorld(attributeNames, world);
            }
        }

        private Implication CreateFullyInclusiveImplicationFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world1)
        {
            var orScenario = CreateOptionalScenarioFromWorld(attNames, world1);
            var andScenario = CreateCompositeScenarioFromWorld(attNames, world1);
            return new Implication(orScenario, andScenario);
        }

        private IEnumerable<string> GetSimilarAttributes(IEnumerable<string> commonAttributes, Dictionary<string, string> world1, Dictionary<string, string> world2)
        {
            return commonAttributes.Where(att => world1[att] == world2[att]).ToList();
        }

        private IEnumerable<string> GetDissimilarAttributes(IEnumerable<string> commonAttributes, Dictionary<string, string> world1, Dictionary<string, string> world2)
        {
            return commonAttributes.Where(att => world1[att] != world2[att]).ToList();
        }

        private IEnumerable<string> GetCommonAttributes(Dictionary<string, string> world1, Dictionary<string, string> world2)
        {
            return world1.Where(w => w.Value == world2[w.Key]).Select(w => w.Key).ToList();
        }

        private Disjunction CreateOptionalScenarioFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = CreateFactListFromWorld(attNames, world);
            return new Disjunction(facts);
        }

        private Conjunction CreateCompositeScenarioFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = CreateFactListFromWorld(attNames, world);
            return new Conjunction(facts);
        }

        private List<Proposition> CreateFactListFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = new List<Proposition>();
            foreach (var attName in attNames)
            {
                var fact = CreateUnaryFactFromAttribute(world, attName);
                facts.Add(fact);
            }

            return facts;
        }

        private IEnumerable<Implication> GetCartesianImplicationsFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var implications = new List<Implication>();
            var attributePairs = CreateAttributeCombinations(attNames);
            return attributePairs
                .Where(pair => world.ContainsKey(pair.Item1) && world.ContainsKey(pair.Item2))
                .Select(p => CreateImplicationBetweenAttributes(p.Item1, p.Item2, world));
        }

        private IEnumerable<Tuple<string,string>> CreateAttributeCombinations(IEnumerable<string> attNames)
        {
            return attNames.SelectMany(a1 => attNames.Where(a2 => a1 != a2).Select(a2 => new Tuple<string, string>(a1, a2)));
        }

        private IEnumerable<Tuple<string, string>> CreateAttributePairs(IEnumerable<string> attNames)
        {
            var array = attNames.ToArray();
            var pairs = new List<Tuple<string, string>>();
            for (int x = 0; x < array.Length; x++)
                for (int y = 0; y < array.Length; y++)
                    if (x != y && !pairs.Any(s => (s.Item1.Equals(array[x]) && s.Item2.Equals(array[y])) || (s.Item1.Equals(array[y]) && s.Item2.Equals(array[x]))))
                        pairs.Add(new Tuple<string,string>(array[x], array[y]));
            return pairs;
        }

        private IEnumerable<Tuple<Dictionary<string, string>, Dictionary<string, string>>> CreateWorldPairs(IEnumerable<Dictionary<string, string>> worlds)
        {
            var array = worlds.ToArray();
            var pairs = new List<Tuple<Dictionary<string, string>, Dictionary<string, string>>>();
            for (int x = 0; x < array.Length; x++)
                for (int y = 0; y < array.Length; y++)
                    if (x != y && !pairs.Any(s => (s.Item1.Equals(array[x]) && s.Item2.Equals(array[y])) || (s.Item1.Equals(array[y]) && s.Item2.Equals(array[x]))))
                        pairs.Add(new Tuple<Dictionary<string, string>, Dictionary<string, string>>(array[x], array[y]));
            return pairs;
        }

        private Implication CreateImplicationBetweenAttributes(string att1Name, string att2Name, Dictionary<string, string> world1)
        {
            var ifSide = CreateUnaryFactFromAttribute(world1, att1Name);
            var thenSide = CreateUnaryFactFromAttribute(world1, att2Name);
            var newRule = new Implication(ifSide, thenSide);
            return newRule;
        }

        private Proposition CreateUnaryFactFromAttribute(Dictionary<string, string> world1, string attName)
        {
            return new Proposition(attName, Predicate.IS, world1[attName]);
        }

        private IEnumerable<string> GetAllAttributes(IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.SelectMany(w => w.Select(att=> att.Key)).Distinct().ToList();
        }

        private IEnumerable<string> GetAllAttributes(Dictionary<string, string> world)
        {
            return world.Select(att => att.Key).Distinct().ToList();
        }

        private IEnumerable<string> GetAttributesBaseOnSimilarity(IEnumerable<Dictionary<string, string>> worlds, bool areSimilar)
        {
            var worldPairs = CreateWorldPairs(worlds);
            var commonAttributes = worldPairs.SelectMany(worldPair => GetCommonAttributes(worldPair.Item1, worldPair.Item2)).Distinct().ToList();
            var relevantAttributes = worldPairs.SelectMany(worldPair => (areSimilar)
                ? GetSimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2) 
                : GetDissimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2));
            var attributeCombinations = CreateAttributeCombinations(relevantAttributes);
            var validatedAttributePairs = attributeCombinations.Where(attPair => AttributesRelationAreConsistentAcrossWorldPairs(worldPairs, attPair)).ToList();
            
            var results = validatedAttributePairs.SelectMany(a => new List<string>() { a.Item1, a.Item2 }).Distinct();
            return results;
        }

        private bool AttributesRelationAreConsistentAcrossWorldPairs(IEnumerable<Tuple<Dictionary<string, string>, Dictionary<string, string>>> worldPairs, Tuple<string,string> attributeCombinations)
        {
            foreach (var worldPair in worldPairs)
            {
                var attribute1ValueInWorld1 = worldPair.Item1[attributeCombinations.Item1];
                var attribute1ValueInWorld2 = worldPair.Item2[attributeCombinations.Item1];
                var attribute2ValueInWorld1 = worldPair.Item1[attributeCombinations.Item2];
                var attribute2ValueInWorld2 = worldPair.Item2[attributeCombinations.Item2];
                if (attribute1ValueInWorld1 == attribute1ValueInWorld2 && attribute2ValueInWorld1 != attribute2ValueInWorld2) return false;
            }
            return true;
        }

        #region Pessimistic worlds

        public IEnumerable<IEvaluatable> GetRulesPessimisticallyFromWorlds(IEnumerable<Dictionary<string,string>> worlds)
        {
            var allDistinctFactsFromAllWorlds = AllAttributesAsDistinctListOfFact(worlds);

            var nonConstantFacts = AllFactsThatAreNotConstantInAllWorlds(worlds, allDistinctFactsFromAllWorlds);

            var repeatedFacts = AllFactsThatRepeatInMoreThanOneWorld(worlds, nonConstantFacts);

            var implications = AsSimpleImplicationsAllFactsThatRepeatWhenAnotherFactsRepeats(worlds, repeatedFacts);

            return implications;
        }

        private IEnumerable<IEvaluatable> AsSimpleImplicationsAllFactsThatRepeatWhenAnotherFactsRepeats(IEnumerable<Dictionary<string, string>> worlds, IEnumerable<Proposition> facts)
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

        public IEnumerable<IEvaluatable> AsConjunctionGetAllFactThatRepeatWhenOtherFactRepeat(IEnumerable<Dictionary<string, string>> worlds, IEnumerable<Proposition> facts)
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

            return conjunction;
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
        private IEnumerable<Proposition> AllFactsThatAreNotConstantInAllWorlds(IEnumerable<Dictionary<string, string>> worlds, IEnumerable<Proposition> commonFact)
        {
            return commonFact.Where(p => _setEvaluator.IsNotTrueInAll(p, worlds));
        }
        private IEnumerable<Proposition> AllAttributesAsDistinctListOfFact(IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.SelectMany(world => GetSimpleFactFromWorld(world)).Distinct();
        }
        private IEnumerable<Proposition> GetSimpleFactFromWorld(Dictionary<string, string> world)
        {
            return world.Select(att => new Proposition(att.Key, Predicate.IS, att.Value));
        }
        private IEnumerable<Proposition> AllFactsThatRepeatInMoreThanOneWorld(IEnumerable<Dictionary<string, string>> worlds, IEnumerable<Proposition> fact)
        {
            return fact.Where(proposition => _setEvaluator.IsTrueInAtLeastTwo(proposition, worlds));
        }

        #endregion Pessimistic worlds
    }
}

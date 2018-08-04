using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeInferenceEngine.Services
{
    public class Utils
    {
        //private static List<IEvaluatable> GetValidatedRulesForWorld<WorldType>(IEnumerable<IEvaluatable> newRules, WorldType world)
        //{
        //    var ruleEngine = new DymeRuleEvaluator(new DictionaryWorldReader());
        //    var validRules = new List<IEvaluatable>();
        //    foreach (var rule in newRules)
        //    {
        //        if (ruleEngine.IsTrueIn(rule, world))
        //            validRules.Add(rule);
        //    }
        //    return validRules;
        //}

        //private IEnumerable<IEvaluatable> CreateNewRulesFromWorld<WorldType>(WorldType world, IEnumerable<string> attributeNames, BuildRulesFromAttributesUsing methodType)
        //{
        //    switch (methodType)
        //    {
        //        case BuildRulesFromAttributesUsing.CartesianImplication:
        //            return GetCartesianImplicationsFromWorld(attributeNames, world);
        //        case BuildRulesFromAttributesUsing.ByWorldDeltas:
        //            return GetCartesianImplicationsFromWorld(attributeNames, world);
        //        default:
        //            return GetCartesianImplicationsFromWorld(attributeNames, world);
        //    }
        //}

        //private Implication CreateFullyInclusiveImplicationFromWorld<WorldType>(IEnumerable<string> attNames, WorldType world1)
        //{
        //    var orScenario = CreateOptionalScenarioFromWorld(attNames, world1);
        //    var andScenario = CreateCompositeScenarioFromWorld(attNames, world1);
        //    return new Implication(orScenario, andScenario);
        //}

        //private IEnumerable<string> GetSimilarAttributes<WorldType>(IEnumerable<string> commonAttributes, WorldType world1, WorldType world2)
        //{
        //    return commonAttributes.Where(att => _worldReader.GetValueFromWorld(att, world1) == _worldReader.GetValueFromWorld(att, world2)).ToList();
        //}

        //private IEnumerable<string> GetDissimilarAttributes<WorldType>(IEnumerable<string> commonAttributes, WorldType world1, WorldType world2)
        //{
        //    return commonAttributes.Where(att => _worldReader.GetValueFromWorld(att, world1) != _worldReader.GetValueFromWorld(att, world2)).ToList();
        //}

        //private IEnumerable<string> GetCommonAttributes<WorldType>(WorldType world1, WorldType world2)
        //{
        //    return world1.Where(w => w.Value == world2[w.Key]).Select(w => w.Key).ToList();
        //}

        //private Disjunction CreateOptionalScenarioFromWorld<WorldType>(IEnumerable<string> attNames, WorldType world)
        //{
        //    var facts = CreateFactListFromWorld(attNames, world);
        //    return new Disjunction(facts);
        //}

        //private Conjunction CreateCompositeScenarioFromWorld<WorldType>(IEnumerable<string> attNames, WorldType world)
        //{
        //    var facts = CreateFactListFromWorld(attNames, world);
        //    return new Conjunction(facts);
        //}

        //private List<Proposition> CreateFactListFromWorld<WorldType>(IEnumerable<string> attNames, WorldType world)
        //{
        //    var facts = new List<Proposition>();
        //    foreach (var attName in attNames)
        //    {
        //        var fact = CreateUnaryFactFromAttribute(world, attName);
        //        facts.Add(fact);
        //    }

        //    return facts;
        //}




        //private IEnumerable<Tuple<string, string>> CreateAttributePairs(IEnumerable<string> attNames)
        //{
        //    var array = attNames.ToArray();
        //    var pairs = new List<Tuple<string, string>>();
        //    for (int x = 0; x < array.Length; x++)
        //        for (int y = 0; y < array.Length; y++)
        //            if (x != y && !pairs.Any(s => (s.Item1.Equals(array[x]) && s.Item2.Equals(array[y])) || (s.Item1.Equals(array[y]) && s.Item2.Equals(array[x]))))
        //                pairs.Add(new Tuple<string,string>(array[x], array[y]));
        //    return pairs;
        //}

        //private IEnumerable<Tuple<WorldType, WorldType>> CreateWorldPairs<WorldType>(IEnumerable<WorldType> worlds)
        //{
        //    var array = worlds.ToArray();
        //    var pairs = new List<Tuple<WorldType, WorldType>>();
        //    for (int x = 0; x < array.Length; x++)
        //        for (int y = 0; y < array.Length; y++)
        //            if (x != y && !pairs.Any(s => (s.Item1.Equals(array[x]) && s.Item2.Equals(array[y])) || (s.Item1.Equals(array[y]) && s.Item2.Equals(array[x]))))
        //                pairs.Add(new Tuple<WorldType, WorldType>(array[x], array[y]));
        //    return pairs;
        //}





        //private IEnumerable<string> GetAllAttributes<WorldType>(IEnumerable<WorldType> worlds)
        //{
        //    return worlds.SelectMany(w => w.Select(att=> att.Key)).Distinct().ToList();
        //}

        //private IEnumerable<string> GetAllAttributes<WorldType>(WorldType world)
        //{
        //    return world.Select(att => att.Key).Distinct().ToList();
        //}

        //private IEnumerable<string> GetAttributesBaseOnSimilarity<WorldType>(IEnumerable<WorldType> worlds, bool areSimilar)
        //{
        //    var worldPairs = CreateWorldPairs(worlds);
        //    var commonAttributes = worldPairs.SelectMany(worldPair => GetCommonAttributes(worldPair.Item1, worldPair.Item2)).Distinct().ToList();
        //    var relevantAttributes = worldPairs.SelectMany(worldPair => (areSimilar)
        //        ? GetSimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2)
        //        : GetDissimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2));
        //    var attributeCombinations = CreateAttributeCombinations(relevantAttributes);
        //    var validatedAttributePairs = attributeCombinations.Where(attPair => AttributesRelationAreConsistentAcrossWorldPairs(worldPairs, attPair)).ToList();

        //    var results = validatedAttributePairs.SelectMany(a => new List<string>() { a.Item1, a.Item2 }).Distinct();
        //    return results;
        //}

        //private bool AttributesRelationAreConsistentAcrossWorldPairs<WorldType>(IEnumerable<Tuple<WorldType, WorldType>> worldPairs, Tuple<string,string> attributeCombinations)
        //{
        //    foreach (var worldPair in worldPairs)
        //    {
        //        var attribute1ValueInWorld1 = worldPair.Item1[attributeCombinations.Item1];
        //        var attribute1ValueInWorld2 = worldPair.Item2[attributeCombinations.Item1];
        //        var attribute2ValueInWorld1 = worldPair.Item1[attributeCombinations.Item2];
        //        var attribute2ValueInWorld2 = worldPair.Item2[attributeCombinations.Item2];
        //        if (attribute1ValueInWorld1 == attribute1ValueInWorld2 && attribute2ValueInWorld1 != attribute2ValueInWorld2) return false;
        //    }
        //    return true;
        //}

        //private List<IEvaluatable> RemoveRulesWithEquivalentRelationships(IEnumerable<IEvaluatable> ruleSet, IEvaluatable ruleWithRelation)
        //{
        //    var satisfactoryRules = new List<IEvaluatable>();
        //    foreach (var ruleFromRuleSet in ruleSet)
        //    {
        //        if (!ruleFromRuleSet.RelationallyEquivalentTo(ruleWithRelation))
        //            satisfactoryRules.Add(ruleFromRuleSet);
        //    }
        //    return satisfactoryRules;
        //}

        //public IEnumerable<IEvaluatable> GetRulesForWorlds<WorldType>(IEnumerable<WorldType> worlds, PickAttriubutesBy methodType)
        //{
        //    var relevantAttributes = GetRelevantAttributes(worlds, methodType);
        //    var allRules = new List<IEvaluatable>();
        //    var invalidRules = new List<IEvaluatable>();
        //    foreach (var dymeWorld in worlds)
        //    {
        //        var newRules = GetCartesianImplicationsFromWorld(relevantAttributes, dymeWorld);
        //        invalidRules.AddRange(GetInvalidRules(worlds, newRules));
        //        newRules = newRules.Where(newRule => !allRules.Contains(newRule)).ToList();
        //        allRules.AddRange(newRules.Except(invalidRules));
        //    }
        //    foreach (var invalidRule in invalidRules)
        //    {
        //        allRules = RemoveRulesWithEquivalentRelationships(allRules, invalidRule);
        //    }
        //    return allRules;
        //}

        //private IEnumerable<string> GetRelevantAttributes<WorldType>(IEnumerable<WorldType> worlds, PickAttriubutesBy methodType)
        //{
        //    switch (methodType)
        //    {
        //        case PickAttriubutesBy.WorldDeltas:
        //            return GetAttributesBaseOnSimilarity(worlds, false);
        //        case PickAttriubutesBy.WorldMatching:
        //            return GetAttributesBaseOnSimilarity(worlds, true);
        //        case PickAttriubutesBy.World:
        //            return GetAllAttributes(worlds);
        //    }
        //    throw new ArgumentOutOfRangeException();
        //}


    }
}

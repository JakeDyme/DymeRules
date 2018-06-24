using Dyme.RuleEngine;
using DymeRuleEngine.Constructs;
using DymeRuleEngine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeInferenceEngine
{

   
    public enum InferenceMethod
    {
        Cartesian,
        ByWorldDeltas,
        ByWorldMatching
    }

    public class InferenceEngine
    {
        private RuleEngine _ruleEngineSvc = new RuleEngine();

        public IEnumerable<IEvaluatable> GetRulesForWorlds(IEnumerable<Dictionary<string, string>> worlds, InferenceMethod methodType)
        {
            //var dymeWorlds = ParseWorlds(worlds);
            var relevantAttributes = GetRelevantAttributes(worlds, methodType);

            var allRules = new List<IEvaluatable>();
            var invalidRules = new List<IEvaluatable>();
            foreach (var dymeWorld in worlds)
            {
                var newRules = CreateCartesianImplicationsFromWorld(relevantAttributes, dymeWorld);
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

        private IEnumerable<string> GetRelevantAttributes(IEnumerable<Dictionary<string,string>> worlds, InferenceMethod methodType)
        {
            switch (methodType)
            {
                case InferenceMethod.ByWorldDeltas:
                    return GetAttributesBaseOnSimilarity(worlds, false);
                case InferenceMethod.ByWorldMatching:
                    return GetAttributesBaseOnSimilarity(worlds, true);
                case InferenceMethod.Cartesian:
                    return GetAllAttributes(worlds);
            }
            throw new ArgumentOutOfRangeException();
        }

        //public IEnumerable<IEvaluatable> UpdateRulesWithWorldAndReturnNewRules(string inputWorld)
        //{
        //    // Consolidate worlds...
        //    var allWorlds = new List<Dictionary<string, string>>();
        //    var newWorld = ParseWorld(inputWorld);
        //    allWorlds.Add(newWorld);
        //    allWorlds.AddRange(GetPreviousWorldsFromDb());

        //    // Consolidate rules...
        //    var rules = new List<IEvaluatable>();
        //    rules.AddRange(GetExistingRulesFromDb());
        //    rules.AddRange(CreateNewRulesFromWorld(newWorld));

        //    // Validate consolidated rules against consolidated worlds
        //    rules = ValidateRulesAgainstAllWorlds(allWorlds, rules);
        //    UpdateRulesDatabase(rules);
        //    UpdateWorldsDatabase(inputWorld);
        //    return rules;
        //}

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

        //private List<IEvaluatable> ValidateRulesAgainstAllWorlds(IEnumerable<Dictionary<string, string>> worlds, List<IEvaluatable> rules)
        //{
        //    var validRules = new List<IEvaluatable>();
        //    var purgeList = new List<IEvaluatable>();
        //    var index = -1;
        //    while (++index < rules.Count())
        //    {
        //        var rule = rules[index];
        //        if (RuleIsValidInAllWorlds(worlds, rule))
        //            validRules.Add(rule);
        //        else
        //            rules = RemoveRulesWithEquivalentRelationships(rules, rule);
        //    }
        //    return validRules;
        //}

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
                if (!_ruleEngineSvc.ValidateRuleAgainstWorld(rule, world))
                    return false;
            }
            return true;
        }

        //private IEnumerable<IEvaluatable> GetExistingRulesFromDb()
        //{
        //    return _rulesDatabase;
        //}

        //private void UpdateWorldsDatabase(string inputWorld)
        //{
        //    _worldsDatabase.Add(inputWorld);
        //}

        //private void UpdateRulesDatabase(IEnumerable<IEvaluatable> newValidRules)
        //{
        //    _rulesDatabase = newValidRules;
        //}

        //private List<Dictionary<string, string>> GetPreviousWorldsFromDb()
        //{
        //    var dymeWorlds = new List<Dictionary<string, string>>();
        //    foreach (var jsonWorld in _worldsDatabase)
        //    {
        //        dymeWorlds.Add(_jsonParserSvc.ParseJson(jsonWorld));
        //    }
        //    return dymeWorlds;
        //}

        private static List<IEvaluatable> GetValidatedRulesForWorld(IEnumerable<IEvaluatable> newRules, Dictionary<string, string> world)
        {
            var ruleEngine = new RuleEngine();
            var validRules = new List<IEvaluatable>();
            foreach (var rule in newRules)
            {
                if (ruleEngine.ValidateRuleAgainstWorld(rule, world))
                    validRules.Add(rule);
            }
            return validRules;
        }

        //public IEnumerable<IEvaluatable> CreateRulesFromCompare(Dictionary<string, string> world1, Dictionary<string, string> world2)
        //{
        //    var evaluatables = new List<IEvaluatable>();
        //    var allAttributeNames = GetDistinctAttributeNames(world1);
        //    var allAttributes = new Dictionary<string, Attribute>();
        //    foreach (var attName in allAttributeNames)
        //    {
        //        Attribute attributeDetails = GetSetAttribute(attName);
        //        UpdateAttributeMetrics(attributeDetails, world1);
        //        allAttributes.Add(attName, attributeDetails);
        //    }
        //    //ValidateAttributes()
        //    //IsValueAnError()
        //    // Determining if discrete value is an error or if its a new value
        //    // - check if there are other values in the discrete set for which there are only one occurrance. +
        //    // - check other variables plus rules to see if there should possibly be another value here. -
        //    // - check if there is possibly a spelling mistake using Levenshtein distance.
        //    var commonAttributeNames = GetCommonAttributes(world1, world2);
        //    var dissimilarAttNames = GetDissimilarAttributes(commonAttributeNames, world1, world2);
        //    var similarAttNames = GetSimilarAttributes(commonAttributeNames, world1, world2);
        //    /// For Each world...
        //    ///   IF sunny THEN hot,
        //    ///   IF hot THEN sunny, 
        //    ///   IF windy THEN hot, 
        //    ///   IF hot THEN windy, 
        //    ///   IF sunny THEN windy, 
        //    ///   IF windy then sunny
        //    var newRules = new List<Imply>();
        //    newRules.AddRange(CreateCartesianImplicationsFromWorld(dissimilarAttNames, world1));
        //    newRules.AddRange(CreateCartesianImplicationsFromWorld(dissimilarAttNames, world2));
        //    /// For each world...
        //    ///  IF sunny OR hot OR windy THEN sunny AND hot AND windy
        //    //rules.Add(CreateFullyInclusiveImplicationFromWorld(dissimilarAttNames, world1));
        //    //rules.Add(CreateFullyInclusiveImplicationFromWorld(dissimilarAttNames, world2));
        //    evaluatables.AddRange(newRules);
        //    return evaluatables;
        //}

        private IEnumerable<IEvaluatable> CreateNewRulesFromWorld(Dictionary<string, string> world, IEnumerable<string> attributeNames, InferenceMethod methodType)
        {
            //var commonAttributeNames = GetCommonAttributes(world1, world2);
            //var dissimilarAttNames = GetDissimilarAttributes(commonAttributeNames, world1, world2);
            //    var similarAttNames = GetSimilarAttributes(commonAttributeNames, world1, world2);
            switch (methodType)
            {
                case InferenceMethod.Cartesian:
                    return CreateCartesianImplicationsFromWorld(attributeNames, world);
                case InferenceMethod.ByWorldDeltas:
                    return CreateCartesianImplicationsFromWorld(attributeNames, world);
                default:
                    return CreateCartesianImplicationsFromWorld(attributeNames, world);
            }
        }

        private Imply CreateFullyInclusiveImplicationFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world1)
        {
            var orScenario = CreateOptionalScenarioFromWorld(attNames, world1);
            var andScenario = CreateCompositeScenarioFromWorld(attNames, world1);
            return new Imply(orScenario, andScenario);
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

        private Scenario CreateOptionalScenarioFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = CreateFactListFromWorld(attNames, world);
            return new Scenario(facts, Junction.OR);
        }

        private Scenario CreateCompositeScenarioFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = CreateFactListFromWorld(attNames, world);
            return new Scenario(facts, Junction.AND);
        }

        private List<Fact> CreateFactListFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var facts = new List<Fact>();
            foreach (var attName in attNames)
            {
                var fact = CreateUnaryFactFromAttribute(world, attName);
                facts.Add(fact);
            }

            return facts;
        }

        private IEnumerable<Imply> CreateCartesianImplicationsFromWorld(IEnumerable<string> attNames, Dictionary<string, string> world)
        {
            var implications = new List<Imply>();
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

        private Imply CreateImplicationBetweenAttributes(string att1Name, string att2Name, Dictionary<string, string> world1)
        {
            var ifSide = CreateUnaryFactFromAttribute(world1, att1Name);
            var thenSide = CreateUnaryFactFromAttribute(world1, att2Name);
            var newRule = new Imply(ifSide, thenSide);
            return newRule;
        }

        private Fact CreateUnaryFactFromAttribute(Dictionary<string, string> world1, string attName)
        {
            return Fact.That(attName).Is(world1[attName]);
        }

        private IEnumerable<string> GetAllAttributes(IEnumerable<Dictionary<string, string>> worlds)
        {
            return worlds.SelectMany(w => w.Select(att=> att.Key)).Distinct().ToList();
        }

        private IEnumerable<string> GetAttributesBaseOnSimilarity(IEnumerable<Dictionary<string, string>> worlds, bool areSimilar)
        {
            var worldPairs = CreateWorldPairs(worlds);
            var commonAttributes = worldPairs.SelectMany(worldPair => GetCommonAttributes(worldPair.Item1, worldPair.Item2)).Distinct().ToList();
            var relevantAttributes = worldPairs.SelectMany(worldPair => (areSimilar)
                ? GetSimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2) 
                : GetDissimilarAttributes(commonAttributes, worldPair.Item1, worldPair.Item2));
            //relevantAttributes = ExcludeConstantAttributes(relevantAttributes, worlds);
            //relevantAttributes = ExcludeRandomAttributes(relevantAttributes, worlds);
            var attributeCombinations = CreateAttributeCombinations(relevantAttributes);
            var validatedAttributePairs = attributeCombinations.Where(attPair => AttributesRelationAreConsistentAcrossWorldPairs(worldPairs, attPair)).ToList();
            
            var results = validatedAttributePairs.SelectMany(a => new List<string>() { a.Item1, a.Item2 }).Distinct();
            return results;
        }

        //private IEnumerable<string> ExcludeConstantAttributes(IEnumerable<string> relevantAttributes, IEnumerable<Dictionary<string, string>> worlds)
        //{
        //    return relevantAttributes.Where(
        //}

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

        #region ATTRIBUTES
        private List<Attribute> _attributeDatabase = new List<Attribute>();

        public class Attribute
        {
            public string Name { get; set; }
            public int OccurrenceCount { get; set; }
            public ICollection<Option> DiscreteOptions { get; set; }
        }

        public enum AttributeContinuity { increasing, decreasing }
        public enum AttributeDataType { number, text, boolean }


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



        //private bool AttributeIsDiscrete(string attributeName, IEnumerable<Dictionary<string,string> allWorlds)
        //{
        //    var possibleValueCount = GetWorldCount(allWorlds);
        //    var discreteValues = GetDiscreetValuesForAttribute(attName, allWorlds);
        //    var discreteValuesCount = GetDiscreetValuesForAttribute(attName);
        //    if (discreteValues.Count == possibleValueCount)
        //        return true;
        //}
        #endregion
    }
}

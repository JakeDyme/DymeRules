using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DymeRuleEngine.Services;
using EasyRuleDymeRule.Services;
using DymeFluentSyntax.Models;
using DymeRuleEngine.Contracts;
using DymeRuleEngine.Models;
using System;

namespace Tests
{
	[TestFixture]
    public class DymeRuleEngineEvaluator_Tests
    {
		private IEvaluator sut;
        private IMetricService metricSvc;
        private IWorldReader worldReader;

        [SetUp]
		public void CreateSut()
		{
            metricSvc = new DefaultMetricService();
            worldReader = new DictionaryWorldReader();
            sut = new DymeRuleEvaluator(worldReader, metricSvc);
		}

		[Test]
		public void ValidateRulesAgainstWorld_GivenRuleAndworld_ExpectPass()
		{
			// Arrange ...
			var parser = new EasyRuleDymeRuleConverter();
			var stateOfTheWorld = new Dictionary<string, string>();
			stateOfTheWorld.Add("weather", "sunny");
			stateOfTheWorld.Add("destination", "beach");
			var inputRule = "IF (weather) IS (sunny) THEN (destination) IS (beach)";
			var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
			
			// Act ...
			var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);
			
			// Assert ...
			Assert.AreEqual(true, result);
		}

		Dictionary<string,string> GetWorldFromFlatWorld(string flatWorld)
		{
			return flatWorld
				.Split(',')
				.ToDictionary(x => x.Trim().Split(':')[0], x => x.Trim().Split(':')[1]);
		}

		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach)", "weather:sunny, destination:beach", true)]
        [TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach) OR (destination) IS (shop)", "weather:sunny, destination:beach", true)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach) OR (destination) IS (shop)", "weather:sunny, destination:shop", true)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach) OR (destination) IS (shop)", "weather:sunny, destination:home", false)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach)", "weather:sunny, destination:shop", false)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach)", "weather:rainy, destination:beach", true)]
		[TestCase("IF (relative) IS (father) THEN (age) GREATER THAN (40)", "relative:father, age:63", true)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach) AND (footwear) IS (slops)", "weather:sunny, destination:beach, footwear:slops", true)]
		[TestCase("IF (weather) IS (sunny) THEN (destination) IS (beach) AND (footwear) IS (slops)", "weather:sunny, destination:beach, footwear:shoes", false)]
		[TestCase("IF (weather) IS (sunny) AND (footwear) IS (slops) THEN (destination) IS (beach)", "weather:sunny, destination:beach, footwear:slops", true)]
		[TestCase("IF (weather) IS (sunny) AND (footwear) IS (slops) THEN (destination) IS (beach)", "weather:sunny, destination:beach, footwear:shoes", true)]
		[TestCase("IF (city[Chicago].weather) IS (very sunny) AND (my.footwear) IS (slops) THEN (destination) IS (beach)", "city[Chicago].weather:very sunny, destination:beach, my.footwear:shoes", true)]
		public void ValidateRulesAgainstWorld_GivenRuleAndworld_ExpectPass(string inputRule, string inputWorld, bool inputResult)
		{
			// Arrange ...
			var parser = new EasyRuleDymeRuleConverter();
			var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
			var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
			bool expectedResult = inputResult;

			// Act ...
			var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

			// Assert ...
			Assert.AreEqual(expectedResult, result);
		}

        [TestCase("weather:sunny, destination:beach, wind:mild, sky:blue", true)]
        public void ValidateRulesAgainstWorld_GivenComplexRuleAndWorld_ExpectPass(string inputWorld, bool inputResult)
        {
            // Arrange ...
            var inputRule = @"IF ((weather) IS (sunny) AND (wind) IS (mild)) or ((weather) IS (sunny) AND (sky) IS (blue))
                   THEN  (destination) IS (beach)";
            var parser = new EasyRuleDymeRuleConverter();
            var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
            var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
            bool expectedResult = inputResult;

            // Act ...
            var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

            // Assert ...
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("weather:sunny, destination:beach, wind:mild, sky:blue", true)]
        public void ValidateRulesAgainstWorld_GivenComplexRule2AndWorld_ExpectPass(string inputWorld, bool inputResult)
        {
            // Arrange ...
            var inputRule = @"IF ((weather) IS (sunny) AND (wind) IS (mild)) OR ((sky) IS (blue))
                            THEN  (destination) IS (beach)";
            var parser = new EasyRuleDymeRuleConverter();
            var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
            var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
            bool expectedResult = inputResult;

            // Act ...
            var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

            // Assert ...
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("weather:sunny, destination:beach, wind:mild, sky:blue", true)]
        public void ValidateRulesAgainstWorld_GivenComplexRule3AndWorld_ExpectPass(string inputWorld, bool inputResult)
        {
            // Arrange ...
            var inputRule = @"IF (if (weather) is (sunny) then (wind) is (mild)) OR ((sky) is (blue) and (weather) is (sunny))
                              THEN (destination) IS (beach)";
            var parser = new EasyRuleDymeRuleConverter();
            var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
            var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
            bool expectedResult = inputResult;

            // Act ...
            var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

            // Assert ...
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("weather:sunny, destination:beach, wind:mild, sky:blue", true)]
        public void ValidateRulesAgainstWorld_GivenSimpleRule1AndWorld_ExpectPass(string inputWorld, bool inputResult)
        {
            // Arrange ...
            var inputRule = @"IF (weather) is (sunny) AND (sky) is (blue)
                              THEN (destination) IS (beach)";
            var parser = new EasyRuleDymeRuleConverter();
            var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
            var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
            bool expectedResult = inputResult;

            // Act ...
            var result = sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

            // Assert ...
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void ImplicationEquals_Given2EqualImplications_ExpectEqual()
        {
            // Arrange...
            var compareItem1 = If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("weather").Is("sunny"));
            var compareItem2 = If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("weather").Is("sunny"));
            // Assert...
            Assert.AreEqual(compareItem1, compareItem2);
        }

        [Test]
        public void ImplicationEquals_Given2EqualImplicationsCreatedByDifferentMeans_ExpectEqual()
        {
            // Arrange...
            var parser = new EasyRuleDymeRuleConverter();
            var compareItem1 = parser.ConvertEasyRuleToDymeRule("IF (sky) IS (blue) THEN (weather) IS (sunny)");
            var compareItem2 = If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("weather").Is("sunny"));
            // Assert...
            Assert.AreEqual(compareItem1, compareItem2);
        }

        [Test]
        public void ImplicationEquals_GivenEqualImplicationsListDistinctified_Expect1Item()
        {
            // Arrange...
            var parser = new EasyRuleDymeRuleConverter();
            var compareItemList = new List<IEvaluatable>();
            compareItemList.Add(parser.ConvertEasyRuleToDymeRule("IF (sky) IS (blue) THEN (weather) IS (sunny)"));
            compareItemList.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("weather").Is("sunny")));            
            // Act...
            compareItemList = compareItemList.Distinct().ToList();
            // Assert...
            Assert.AreEqual(1, compareItemList.Count());
        }

        [Test]
        public void ConjunctionEquals_Given2EqualConjunctionsCreatedByDifferentMeans_ExpectEqual()
        {
            // Arrange...
            var parser = new EasyRuleDymeRuleConverter();
            var compareItem1 = parser.ConvertEasyRuleToDymeRule("(sky) IS (blue) AND (weather) IS (sunny)");
            var compareItem2 = All.Of(ItsAFact.That("sky").Is("blue")).And(ItsAFact.That("weather").Is("sunny")).IsTrue();
            // Assert...
            Assert.AreEqual(compareItem1, compareItem2);
        }

        [Test]
        public void ConjunctionEquals_Given2EqualConjunctionsWith3ArgumentsCreatedByDifferentMeans_ExpectSimilarHashes()
        {
            // Arrange...
            var parser = new EasyRuleDymeRuleConverter();
            var compareItem1 = parser.ConvertEasyRuleToDymeRule("(sky) IS (blue) AND (weather) IS (sunny) AND (water) IS (warm)");
            var compareItem2 = All.Of(
                ItsAFact.That("sky").Is("blue"))
                .And(ItsAFact.That("weather").Is("sunny"))
                .And(ItsAFact.That("water").Is("warm"))
                .IsTrue();

            // Act...
            var result1 = compareItem1.GetHashCode();
            var result2 = compareItem2.GetHashCode();

            // Assert...
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void ConjunctionEquals_Given2EqualConjunctionsWith3ArgumentsCreatedByDifferentMeans_ExpectEqual()
        {
            // Arrange...
            var parser = new EasyRuleDymeRuleConverter();
            var compareItem1 = parser.ConvertEasyRuleToDymeRule("(sky) IS (blue) AND (weather) IS (sunny) AND (water) IS (warm)");
            var compareItem2 = All.Of(
                ItsAFact.That("sky").Is("blue"))
                .And(ItsAFact.That("weather").Is("sunny"))
                .And(ItsAFact.That("water").Is("warm"))
                .IsTrue();
            // Assert...
            Assert.AreEqual(compareItem1, compareItem2);
        }

        [Test]
        public void GetHashCode_GivenConjunctionWith1Argument_ExpectHashCode()
        {
            // Arrange...
            var input = new Conjunction(new[] { new Proposition("sky", Predicate.IS, "blue") });
            var expected = -639322823;
            // Act...
            var result = input.GetHashCode();

            // Assert...
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ToString_GivenConjunctionWith1Argument_ExpectString()
        {
            // Arrange...
            var input = new Conjunction(new[] { new Proposition("sky", Predicate.IS, "blue") });
            var expected = "sky IS blue";
            // Act...
            var result = input.ToString();

            // Assert...
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MtricSvc_GivenRuleAndworld_ExpectPass()
        {
            // Arrange ...
            var parser = new EasyRuleDymeRuleConverter();
            var stateOfTheWorld = new Dictionary<string, string>();
            stateOfTheWorld.Add("weather", "sunny");
            stateOfTheWorld.Add("destination", "beach");
            var inputRule = "IF (weather) IS (sunny) THEN (destination) IS (beach)";
            var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
            var expectedResult = new Dictionary<string, int>();
            expectedResult.Add("EvaluateImplication", 1);
            expectedResult.Add("EvaluateProposition", 2);
            expectedResult.Add("GetValueFromWorld", 2);
            // Act ...
            sut.IsTrueIn(evaluatableRule, stateOfTheWorld);

            var result = (metricSvc as DefaultMetricService).metrics;
            // Assert ...
            CollectionAssert.AreEquivalent(expectedResult, result);
        }
    }

}

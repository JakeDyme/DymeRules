using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Dyme.RuleEngine;
using EasyRule.Dyme;

namespace Tests
{
	[TestFixture]
    public class EasyRule_Tests
    {
		private RuleEngine sut;

		[SetUp]
		public void CreateSut()
		{
			sut = new RuleEngine();
		}

		[Test]
		public void ValidateRulesAgainstWorld_GivenRuleAndworld_ExpectPass()
		{
			// Arrange ...
			var parser = new EasyRuleDymeParser();
			var stateOfTheWorld = new Dictionary<string, string>();
			stateOfTheWorld.Add("weather", "sunny");
			stateOfTheWorld.Add("destination", "beach");
			var inputRule = "IF (weather) IS (sunny) THEN (destination) IS (beach)";
			var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
			
			// Act ...
			var result = sut.ValidateRuleAgainstWorld(evaluatableRule, stateOfTheWorld);
			
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
			var parser = new EasyRuleDymeParser();
			var stateOfTheWorld = GetWorldFromFlatWorld(inputWorld);
			var evaluatableRule = parser.ConvertEasyRuleToDymeRule(inputRule);
			bool expectedResult = inputResult;

			// Act ...
			var result = sut.ValidateRuleAgainstWorld(evaluatableRule, stateOfTheWorld);

			// Assert ...
			Assert.AreEqual(expectedResult, result);
		}

	}
}

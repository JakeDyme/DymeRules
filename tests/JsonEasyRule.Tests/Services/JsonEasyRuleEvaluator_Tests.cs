﻿using NUnit.Framework;
using JsonEasyRule.Services;
using System.Collections.Generic;

namespace JsonEasyRulesTests
{
    [TestFixture]
    public class JsonEasyRuleEvaluator_Tests
    {
        [Test]
        public void JsonWorldsToRules_SubmitJsonObjects_ExpectInferredRules()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var expectedRules = new List<string>();
            expectedRules.Add("IF (Year) IS (2030) THEN (Age) IS (30)");

            var sut = JsonEasyRuleEvaluator.CreateEvaluator();
            // Act...
            var result = sut.InferEasyRules(jsonWorlds);

            // Assert...
            Assert.AreEqual(expectedRules, result);
        }

        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN SOME(Prices) are (Setting)(Buyers)", false)]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ALL(Buyers) are (Setting)(Owners)", false)]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN SOME(Buyers) are (Setting)(Owners)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ONLY ONE OF(Buyers) are (Setting)(Owners)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN SOME(Buyers) are SOME(Setting)(Owners)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN SOME OF(Prices) is (Setting)(MaxPrice)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ONLY ONE(Prices) is (Setting)(MaxPrice)", false)]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ONLY ONE OF(Buyers) is (Setting)(Seller)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN SINGLE(Buyers) is (Setting)(Seller)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ANY(Buyers) is (Setting)(Seller)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN ALL(Buyers) is (Setting)(Seller)", false) ]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN (Prices) is (Setting)(Prices)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN (Prices[0]) is (Setting)(Prices[0])")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN (Manufacturers[0].Products[0].Price) IS (50)")]
        [TestCase(@"IF (Stores[1]) IS (Willis Street) THEN (Manufacturers[0].Products[0].Price) IS (51)", false)]
        [TestCase(@"IF ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Name) IS (Anvil) THEN ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Price) IS (50)")]
        [TestCase(@"IF ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Name) IS (Anvil) 
                   THEN ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Price) IS (50)")]
        [TestCase(@"IF ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Name)        IS (Anvil)
                   THEN ($.Manufacturers[?(@.Name == 'Acme Co')].Products[0].Price) IS   (50)")]
        [TestCase(@"IF ($.Stores[0]) IS (Lambton Quay) THEN ($.Manufacturers[0].Name) IS NOT (Fireworks)")]
        [TestCase(@"if ($.Stores[0]) IS (Lambton Quay) then ($.Manufacturers[0].Products[0].Price) is greater than (49.36) AND ($.Manufacturers[0].Products[0].Price) is less than (51)")]
        public void IsTrueIn_GivenJsonObjectsAndEasyRule_ValidReturn(string easyRule, bool expectedResult = true)
        {
            // Arrange...
            var jsonWorld = @"{
              'Stores': [
                'Lambton Quay',
                'Willis Street'
              ],
              'Prices': [
                10,
                10
              ],
              'MaxPrice': 10,
              'Buyers': [
                'John',
                'Sam'
              ],
              'Seller': 'John',
              'Owners': [
                'Sam',
                'Steve'
              ],
              'Manufacturers': [
                {
                  'Name': 'Acme Co',
                  'Products': [
                    {
                      'Name': 'Anvil',
                      'Price': 50
                    }
                  ]
                },
                {
                  'Name': 'Contoso',
                  'Products': [
                    {
                      'Name': 'Elbow Grease',
                      'Price': 99.95
                    },
                    {
                      'Name': 'Headlight Fluid',
                      'Price': 4
                    }
                  ]
                }
              ]
            }";

            var sut = JsonEasyRuleEvaluator.CreateEvaluator();
            // Act...
            var result = sut.IsTrueIn(easyRule, jsonWorld);
            
            // Assert...
            Assert.AreEqual(expectedResult, result);
        }
    }


}



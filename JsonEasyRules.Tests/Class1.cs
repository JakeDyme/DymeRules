﻿using NUnit.Framework;
using JsonToEasyRules;
using System.Collections.Generic;

namespace JsonEasyRulesTests
{
    [TestFixture]
    public class Class1
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
            expectedRules.Add("IF (Year) IS (2040) THEN (Age) IS (40)");
            expectedRules.Add("IF (Year) IS (2030) THEN (Age) IS (30)");
            expectedRules.Add("IF (Year) IS (2010) THEN (Age) IS (30)");

            var sut = new JsonEasyRules();
            // Act...
            var result = sut.SubmitJsonWorldsAndGetInferredEasyRules(jsonWorlds);

            // Assert...
            Assert.AreEqual(expectedRules, result);
        }
    }
}

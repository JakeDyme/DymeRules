using DymeRuleEngine.Constructs;
using EasyRule.Dyme;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRuleToDymeRules.Tests
{
    [TestFixture]
    public class EasyRuleToDymeRulesTests
    {
        [Test]
        public void EasyRuleDymeParser_GivenEasyRules_ExpectDymeRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeParser();
            var inputEasyRule = "IF (Age IS 18) THEN (Movie IS Visible)";
            var expectDymeRule = Imply.Create().If(Fact.That("Age").Is("18")).Then(Fact.That("Movie").Is("Visible"));
            // Act...
            var result = sut.ConvertEasyRuleToDymeRule(inputEasyRule);

            Assert.AreEqual(expectDymeRule, result);
        }
        

    }
}

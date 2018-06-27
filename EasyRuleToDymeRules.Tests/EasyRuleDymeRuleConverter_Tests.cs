using DymeFluentSyntax.Models;
using EasyRuleDymeRule.Services;
using NUnit.Framework;

namespace EasyRuleToDymeRules.Tests
{
    [TestFixture]
    public class EasyRuleDymeRuleConverter_Tests
    {
        [Test]
        public void EasyRuleDymeParser_GivenEasyRules_ExpectDymeRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var inputEasyRule = "IF (Age) IS (18) THEN (Movie) IS (Visible)";
            var expectDymeRule = If.When(ItsAFact.That("Age").Is("18")).Then(ItsAFact.That("Movie").Is("Visible"));
            // Act...
            var result = sut.ConvertEasyRuleToDymeRule(inputEasyRule);

            Assert.AreEqual(expectDymeRule, result);
        }

        [Test]
        public void EasyRuleDymeParser_GivenDymeRules_ExpectEasyRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var expectEasyRule = "IF (Age) IS (18) THEN (Movie) IS (Visible)";
            var inputDymeRule = If.When(ItsAFact.That("Age").Is("18")).Then(ItsAFact.That("Movie").Is("Visible"));
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);

            Assert.AreEqual(expectEasyRule, result);
        }

        [Test]
        public void EasyRuleDymeParser_GivenConjunctiveDymeRules_ExpectEasyRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var expectEasyRule = "IF (Age) IS (18) AND (Name) IS (Bob) THEN (Movie) IS (Visible)";
            var inputDymeRule = If
                .When(All.Of(ItsAFact.That("Age").Is("18")).And(ItsAFact.That("Name").Is("Bob")).AreTrue())
                .Then(ItsAFact.That("Movie").Is("Visible"));
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);

            Assert.AreEqual(expectEasyRule, result);
        }

        [Test]
        public void EasyRuleDymeParser_GivenDisjunctiveDymeRules_ExpectEasyRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var expectEasyRule = "IF (Age) IS (18) OR (Name) IS (Bob) THEN (Movie) IS (Visible)";
            var inputDymeRule = 
                If.When(Any.Of(ItsAFact.That("Age").Is("18")).Or(ItsAFact.That("Name").Is("Bob")).IsTrue())
                  .Then(ItsAFact.That("Movie").Is("Visible"));
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);

            Assert.AreEqual(expectEasyRule, result);
        }

        [Test]
        public void EasyRuleDymeParser_GivenBothJunctiveDymeRules_ExpectEasyRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var expectEasyRule = "IF (age) IS (18) OR (chest) IS (hairy) THEN (movie) IS (visible) AND (there) IS (boobs)";
            var inputDymeRule =
                If.When(Any.Of
                    (ItsAFact.That("age").Is("18"))
                    .Or
                    (ItsAFact.That("chest").Is("hairy"))
                    .IsTrue())
                  .Then(All.Of
                    (ItsAFact.That("movie").Is("visible"))
                    .And
                    (ItsAFact.That("there").Is("boobs"))
                    .AreTrue());
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);
            Assert.AreEqual(expectEasyRule, result);
        }

    }
}

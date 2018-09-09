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
                .When(All.Of(ItsAFact.That("Age").Is("18")).And(ItsAFact.That("Name").Is("Bob")).IsTrue())
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
            var expectEasyRule = "IF (age) IS (10) OR (genre) IS (cartoon) THEN (movie) IS (visible) AND (there) NOT (violence)";
            var inputDymeRule =
                If.When(Any.Of
                    (ItsAFact.That("age").Is("10"))
                    .Or
                    (ItsAFact.That("genre").Is("cartoon"))
                    .IsTrue())
                  .Then(All.Of
                    (ItsAFact.That("movie").Is("visible"))
                    .And
                    (ItsAFact.That("there").IsNot("violence"))
                    .IsTrue());
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);
            Assert.AreEqual(expectEasyRule, result);
        }

        [Test]
        public void EasyRuleDymeParser_GivenNestedDymeRules_ExpectEasyRules()
        {
            // Arrange...
            var sut = new EasyRuleDymeRuleConverter();
            var expectEasyRule = "IF (age) GREATER THAN (18) OR ((age) LESS THAN (100) AND (date) PART OF (2000/01/01)) THEN (movie) IS (visible) AND (warnings) CONTAINS (violence)";
            var inputDymeRule =
                If.When(Any.Of
                    (ItsAFact.That("age").IsGreaterThan("18"))
                    .Or
                    (All.Of(ItsAFact.That("age").IsLessThan("100")).And(ItsAFact.That("date").IsIn("2000/01/01")).IsTrue() )
                    .IsTrue())
                  .Then(All.Of
                    (ItsAFact.That("movie").Is("visible"))
                    .And
                    (ItsAFact.That("warnings").Contains("violence"))
                    .IsTrue());
            // Act...
            var result = sut.ConvertDymeRuleToEasyRule(inputDymeRule);
            Assert.AreEqual(expectEasyRule, result);
        }

    }
}

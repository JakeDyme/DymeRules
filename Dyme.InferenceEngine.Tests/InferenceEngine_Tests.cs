using JsonToDymeWorldParser;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DymeRuleEngine.Contracts;
using DymeFluentSyntax;

namespace DymeInferenceEngine.Tests
{
    [TestFixture]
    public class InferenceEngine_Tests
    {
        [Test]
        public void CreateRule_GivenWorlds_ExpectSimpleImply()
        {
            // Arrange...
            var world1 = "{'Name':'Bob', 'Age':'40'}";
            
            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(If
                .When(ItsAFact.That("Name").Is("Bob"))
                .Then(ItsAFact.That("Age").Is("40"))
            );
            expected.Add(If
                .When(ItsAFact.That("Age").Is("40"))
                .Then(ItsAFact.That("Name").Is("Bob"))
            );
            var jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
            var world = jsonToDymeWorldParserSvc.ParseJson(world1);

            // Act...
            var results = sut.GetRulesForWorlds(new List<Dictionary<string,string>>() { world }, InferenceMethod.Cartesian);

            // Assert...
            Assert.AreEqual(expected, results);
        }
        
        [Test]
        public void GetRulesForWorlds_GivenWorlds_ExpectRuleSetfor2Worlds()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
            var worlds = jsonWorlds.Select(w=> jsonToDymeWorldParserSvc.ParseJson(w));

            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Name").Is("Bob")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Name").Is("Bob")));

            expected.Add(If.When(ItsAFact.That("Age").Is("40")).Then(ItsAFact.That("Name").Is("Bob")));
            expected.Add(If.When(ItsAFact.That("Age").Is("30")).Then(ItsAFact.That("Name").Is("Bob")));
            expected.Add(If.When(ItsAFact.That("Age").Is("40")).Then(ItsAFact.That("Year").Is("2040")));
            expected.Add(If.When(ItsAFact.That("Age").Is("30")).Then(ItsAFact.That("Year").Is("2030")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.Cartesian);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void GetRulesForWorlds_GivenWorlds_ExpectRuleSetFor3Worlds()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ParseJson(w));

            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();            
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));
            
            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.Cartesian);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void GetRulesForWorldsUsingDeltas_GivenWorlds_ExpectRuleSet()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ParseJson(w));

            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.ByWorldDeltas);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void GetRulesForWorldsUsingMatching_GivenWorlds_ExpectRuleSet()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldParser();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ParseJson(w));

            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.ByWorldMatching);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }
    }
}

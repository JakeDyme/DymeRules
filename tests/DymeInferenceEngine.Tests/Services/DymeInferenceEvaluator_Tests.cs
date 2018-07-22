using JsonDymeWorld.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DymeRuleEngine.Contracts;
using DymeFluentSyntax.Models;
using DymeInferenceEngine.Contracts;
using DymeInferenceEngine.Services;
using DymeRuleEngine.Models;

namespace DymeInferenceEngine.Tests
{
    [TestFixture]
    public class DymeInferenceEvaluator_Tests
    {
        [Test]
        public void CreateRule_GivenWorlds_ExpectSimpleImply()
        {
            // Arrange...
            var world1 = "{'Name':'Bob', 'Age':'40'}";
            
            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If
                .When(ItsAFact.That("Name").Is("Bob"))
                .Then(ItsAFact.That("Age").Is("40"))
            );
            expected.Add(If
                .When(ItsAFact.That("Age").Is("40"))
                .Then(ItsAFact.That("Name").Is("Bob"))
            );
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var world = jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(world1);

            // Act...
            var results = sut.GetRulesForWorlds(new List<Dictionary<string,string>>() { world }, PickAttriubutesBy.World);

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

            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w=> jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
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
            var results = sut.GetRulesForWorlds(worlds, PickAttriubutesBy.World);

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
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));
            
            // Act...
            var results = sut.GetRulesForWorlds(worlds, PickAttriubutesBy.World);

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
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, PickAttriubutesBy.WorldDeltas);

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
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2010")).Then(ItsAFact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, PickAttriubutesBy.WorldMatching);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void PessimisticWorld_GivenWorlds_ExpectRuleSet()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            jsonWorlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            jsonWorlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            jsonWorlds.Add("{'Name':'Joe', 'Age':'40', 'Year': '2040'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("Year").Is("2040")).Then(ItsAFact.That("Age").Is("40")));
            expected.Add(If.When(ItsAFact.That("Age").Is("40")).Then(ItsAFact.That("Year").Is("2040")));
            expected.Add(If.When(ItsAFact.That("Year").Is("2030")).Then(ItsAFact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesPessimisticallyFromWorlds(worlds);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }


        [Test]
        public void PessimisticWorld_GivenWorlds2_ExpectRuleSet()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'planet':'Earth', 'sky':'blue', 'ground': 'soft', 'cat': 'InCharge'}");
            jsonWorlds.Add("{'planet':'Venus', 'sky':'yellow', 'ground': 'hard', 'cat': 'InCharge'}");
            jsonWorlds.Add("{'planet':'Mars', 'sky':'red', 'ground': 'soft', 'cat': 'InCharge'}");
            jsonWorlds.Add("{'planet':'Pluto', 'sky':'blue', 'ground': 'soft', 'cat': 'InCharge'}");
            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var sut = new DymeInferenceEvaluator();
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));

            // Act...
            var results = sut.GetRulesPessimisticallyFromWorlds(worlds);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void MergeImplications_GivenImplicationsWithSameConsequent_ExpectMergedImplication()
        {
            // Arrange...
            var input = new List<IEvaluatable>();
            input.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));
            input.Add(If.When(ItsAFact.That("sky").Is("red")).Then(ItsAFact.That("ground").Is("soft")));
            
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(Any.Of(ItsAFact.That("sky").Is("blue")).Or(ItsAFact.That("sky").Is("red")).IsTrue()).Then(ItsAFact.That("ground").Is("soft")));

            // Act...
            var results = ImplicationHelper.MergeImplications(input);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void MergeImplications_GivenImplicationsWithSameAntecedent_ExpectMergedImplication2()
        {
            // Arrange...
            var input = new List<IEvaluatable>();
            input.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));
            input.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("hard")));
            
            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(Any.Of(ItsAFact.That("ground").Is("soft")).Or(ItsAFact.That("ground").Is("hard")).IsTrue()));
            // Act...
            var results = ImplicationHelper.MergeImplications(input);
            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void MergeImplications_GivenImplicationsDifferent_ExpectNotMergedImplication()
        {
            // Arrange...
            var input = new List<IEvaluatable>();
            input.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));
            input.Add(If.When(ItsAFact.That("sky").Is("red")).Then(ItsAFact.That("ground").Is("hard")));

            var expected = new List<IEvaluatable>();
            expected.Add(If.When(ItsAFact.That("sky").Is("red")).Then(ItsAFact.That("ground").Is("hard")));
            expected.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));
            // Act...
            var results = ImplicationHelper.MergeImplications(input);
            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }


        [Test]
        public void ConvertArgumentsToJunction_Given2Arguments_ExpectJunction()
        {
            // Arrange...
            var arguments = new List<IEvaluatable>();
            arguments.Add(ItsAFact.That("sky").Is("blue"));

            //var expected = new List<IEvaluatable>();
            var expected = All.Of(ItsAFact.That("sky").Is("blue")).IsTrue();

            // Act...
            var result = ImplicationHelper.ConvertArgumentsToJunction(arguments, Junction.AND);

            // Assert...
            Assert.AreEqual(expected, result);
        }

        public void MergeImplications_Given3ImplicationsSameConsequent_ExpectMergedImplication()
        {
            // Arrange...
            var input = new List<IEvaluatable>();
            input.Add(If.When(ItsAFact.That("sky").Is("blue")).Then(ItsAFact.That("ground").Is("soft")));
            input.Add(If.When(ItsAFact.That("sky").Is("red")).Then(ItsAFact.That("ground").Is("soft")));
            input.Add(If.When(ItsAFact.That("sky").Is("yellow")).Then(ItsAFact.That("ground").Is("soft")));

            var expected = new List<IEvaluatable>();
            expected.Add(If
                .When(Any.Of(ItsAFact.That("sky").Is("blue"))
                    .Or(ItsAFact.That("sky").Is("red"))
                    .Or(ItsAFact.That("sky").Is("yellow"))
                    .IsTrue())
                .Then(ItsAFact.That("ground").Is("soft")));
            // Act...
            var results = ImplicationHelper.MergeImplications(input);
            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }


        [Test]
        public void CreateRules_GivenWorlds_ExpectConjunctions()
        {
            // Arrange...
            var jsonWorlds = new List<string>();
            jsonWorlds.Add("{'planet':'Earth', 'sky':'blue', 'ground': 'soft', 'cat': 'InCharge'}");
            jsonWorlds.Add("{'planet':'Pluto', 'sky':'blue', 'ground': 'soft', 'cat': 'grumpy'}");

            var jsonToDymeWorldParserSvc = new JsonDymeWorldConverter();
            var worlds = jsonWorlds.Select(w => jsonToDymeWorldParserSvc.ConvertJsonToDymeWorld(w));

            var input = new List<Proposition>();
            input.Add(ItsAFact.That("sky").Is("blue") as Proposition);
            input.Add(ItsAFact.That("ground").Is("soft") as Proposition);
            input.Add(ItsAFact.That("cat").Is("InCharge") as Proposition);

            var expected = new List<IEvaluatable>();
            expected.Add(All.Of(ItsAFact.That("sky").Is("blue")).And(ItsAFact.That("ground").Is("soft")).IsTrue());

            var sut = new DymeInferenceEvaluator();

            // Act...
            var results = sut.AsConjunctionGetAllFactThatRepeatWhenOtherFactRepeat(worlds, input);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }
    }
}

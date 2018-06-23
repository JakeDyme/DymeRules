using Dyme.RuleEngine;
using DymeInferenceEngine;
using JsonToDymeWorldParser;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DymeInferenceEngine.Tests
{
    [TestFixture]
    public class InferenceEngine_Tests
    {
        //private JsonDymeWorldParser _jsonParser = new JsonException();
        //public Dictionary<string,string> GetWorldFromJson(string json){
        //    return _jsonParser.ParseJson(json);
        //}
        [Test]
        public void CreateRule_GivenWorlds_ExpectSimpleImplication()
        {
            // Arrange...
            var world1 = "{'Name':'Bob', 'Age':'40'}";
            
            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(Imply.That()
                .If(Fact.That("Name").Is("Bob"))
                .Then(Fact.That("Age").Is("40"))
            );
            expected.Add(Imply.That()
                .If(Fact.That("Age").Is("40"))
                .Then(Fact.That("Name").Is("Bob"))
            );
            // Act...
            var results = sut.GetRulesForWorlds(new[] { world1 }, InferenceMethod.Cartesian);

            // Assert...
            Assert.AreEqual(expected, results);
        }

        
        [Test]
        public void GetRulesForWorlds_GivenWorlds_ExpectRuleSetfor2Worlds()
        {
            // Arrange...
            var worlds = new List<string>();
            worlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            worlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");


            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(Imply.That().If(Fact.That("Year").Is("2040")).Then(Fact.That("Age").Is("40")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2030")).Then(Fact.That("Age").Is("30")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2040")).Then(Fact.That("Name").Is("Bob")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2030")).Then(Fact.That("Name").Is("Bob")));

            expected.Add(Imply.That().If(Fact.That("Age").Is("40")).Then(Fact.That("Name").Is("Bob")));
            expected.Add(Imply.That().If(Fact.That("Age").Is("30")).Then(Fact.That("Name").Is("Bob")));
            expected.Add(Imply.That().If(Fact.That("Age").Is("40")).Then(Fact.That("Year").Is("2040")));
            expected.Add(Imply.That().If(Fact.That("Age").Is("30")).Then(Fact.That("Year").Is("2030")));
            

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.Cartesian);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }


        [Test]
        public void GetRulesForWorlds_GivenWorlds_ExpectRuleSetFor3Worlds()
        {
            // Arrange...
            var worlds = new List<string>();
            worlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            worlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();            
            expected.Add(Imply.That().If(Fact.That("Year").Is("2040")).Then(Fact.That("Age").Is("40")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2030")).Then(Fact.That("Age").Is("30")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2010")).Then(Fact.That("Age").Is("30")));
            
            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.Cartesian);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

        [Test]
        public void GetRulesForWorldsUsingDeltas_GivenWorlds_ExpectRuleSet()
        {
            // Arrange...
            var worlds = new List<string>();
            worlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            worlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(Imply.That().If(Fact.That("Year").Is("2040")).Then(Fact.That("Age").Is("40")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2030")).Then(Fact.That("Age").Is("30")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2010")).Then(Fact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.ByWorldDeltas);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }


        [Test]
        public void GetRulesForWorldsUsingMatching_GivenWorlds_ExpectRuleSet()
        {
            // Arrange...
            var worlds = new List<string>();
            worlds.Add("{'Name':'Bob', 'Age':'40', 'Year': '2040'}");
            worlds.Add("{'Name':'Bob', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Sam', 'Age':'30', 'Year': '2030'}");
            worlds.Add("{'Name':'Tom', 'Age':'30', 'Year': '2010'}");
            var sut = new InferenceEngine();
            var expected = new List<IEvaluatable>();
            expected.Add(Imply.That().If(Fact.That("Year").Is("2040")).Then(Fact.That("Age").Is("40")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2030")).Then(Fact.That("Age").Is("30")));
            expected.Add(Imply.That().If(Fact.That("Year").Is("2010")).Then(Fact.That("Age").Is("30")));

            // Act...
            var results = sut.GetRulesForWorlds(worlds, InferenceMethod.ByWorldMatching);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);
        }

    }
}

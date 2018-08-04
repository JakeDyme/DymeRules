using JsonEasyRule.Models;
using NUnit.Framework;
using System.Collections.Generic;


namespace JsonEasyRule.Tests.Models
{
    public class JsonPathWorldAnalyser_Tests
    {
        [Test]
        public void GetAttributesAndValues_GivenJson_ExpectAttributesAndValues()
        {
            // Arrange...
            var input = "{'Name':'Bob', 'Age':'40', 'Year': '2040'}";
            var expected = new List<KeyValuePair<string,string>>();
            expected.Add(new KeyValuePair<string, string>("Name", "Bob"));
            expected.Add(new KeyValuePair<string, string>("Age", "40"));
            expected.Add(new KeyValuePair<string, string>("Year", "2040"));

            var sut = new JsonPathWorldAnalyser();


            // Act...
            var results = sut.GetAttributesAndValues(input);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);

        }

        [Test]
        public void GetAttributesAndValues_GivenMultiLevelJson_ExpectAttributesAndValues()
        {
            // Arrange...
            var input = "{'Year': '2040', 'Employees':[{'Name':'Bob','Age':'40'}, {'Name':'Sam','Age':'20'}]}";
            var expected = new List<KeyValuePair<string, string>>();
            expected.Add(new KeyValuePair<string, string>("Year", "2040"));
            expected.Add(new KeyValuePair<string, string>("Employees[0].Name", "Bob"));
            expected.Add(new KeyValuePair<string, string>("Employees[0].Age", "40"));
            expected.Add(new KeyValuePair<string, string>("Employees[1].Name", "Sam"));
            expected.Add(new KeyValuePair<string, string>("Employees[1].Age", "20"));

            var sut = new JsonPathWorldAnalyser();

            // Act...
            var results = sut.GetAttributesAndValues(input);

            // Assert...
            CollectionAssert.AreEquivalent(expected, results);

        }

    }
}

using JsonDymeWorld.Services;
using NUnit.Framework;
using System.Collections.Generic;

namespace JsonToDymeWorldParser.Tests
{
	[TestFixture]
    public class Parser_Tests
    {

		[Test]
		public void ParseWorld_GivenJsonObject_ExpectWorld()
		{
			// Arrange...
			var sut = new JsonDymeWorldConverter();
			var inputData = "{\"Person\":{\"Name\":\"Jake\",\"Age\":35,\"Married\":false}}";
			var expected = new Dictionary<string, string>();

			expected.Add("Person.Name", "Jake");
			expected.Add("Person.Age", "35");
			expected.Add("Person.Married", "false");

			// Act...
			var result = sut.ParseJson(inputData);

			// Assert...
			Assert.AreEqual(expected["Person.Age"], result["Person.Age"]);

		}

		[TestCase("{\"Person\": \"JakeDyme\"}", "Person", "JakeDyme")]
		[TestCase("{\"Person\":{\"Name\":\"Jake\"}}", "Person.Name", "Jake")]
		[TestCase("{\"Person\":[{\"Name\":\"Jake\"}]}", "Person[0].Name", "Jake")]
		public void ParseWorld_GivenJsonObject_ExpectWorld(string inputJson, string expectedKey, string expectedValue)
		{
			// Arrange...
			var sut = new JsonDymeWorldConverter();

			// Act...
			var result = sut.ParseJson(inputJson);

			// Assert...
			Assert.AreEqual(expectedValue, result[expectedKey]);
		}

	}


}

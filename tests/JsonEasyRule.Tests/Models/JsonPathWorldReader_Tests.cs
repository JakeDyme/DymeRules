using JsonEasyRule.Models;
using NUnit.Framework;

namespace JsonEasyRule.Tests.Models
{
    public class JsonPathWorldReader_Tests
    {
        [Test]
        public void JsonPathworldReader_GivenJsonPathAndWorld_ExpectValue() {
            // Arrange...
            var jsonWorld = "{'Name':'Bob', 'Age':'40', 'Year': '2040'}";
            var query = "$.Name";
            var sut = new JsonPathWorldReader();
            var expected = "Bob";
            // Act...
            var result = sut.GetValueFromWorld(query, jsonWorld);

            // Assert
            Assert.AreEqual(result, expected);
        }
        
    }
}

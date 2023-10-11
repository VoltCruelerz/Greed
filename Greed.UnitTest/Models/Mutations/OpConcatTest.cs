using Greed.Models.Mutations.Operations.Arrays;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpConcatTest
    {
        [TestMethod]
        public void Concat_Shallow_Int()
        {
            // Arrange
            var root = JObject.Parse("{ \"a\": [0, 1] }");
            var config = new JObject
            {
                { "path", "a[i]" },
                { "value", 3 }
            };
            var op = new OpConcat(config);

            // Act
            op.Exec(root);

            // Assert
            Assert.AreEqual(3, ((JArray)root["a"]!).Count);
        }

        [TestMethod]
        public void Concat_Shallow_Str()
        {
            // Arrange
            var root = new JObject
            {
                { "a", new JArray { "strA", "strB" } }
            };
            var config = new JObject
            {
                { "path", "a[i]" },
                { "value", "strC" }
            };
            var op = new OpConcat(config);

            // Act
            op.Exec(root);

            // Assert
            Assert.AreEqual(3, ((JArray)root["a"]!).Count);
        }
    }
}

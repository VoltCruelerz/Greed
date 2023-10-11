using Greed.Models.Mutations.Operations.Arrays;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpConcatTest
    {
        [TestMethod]
        public void Concat_Shallow_SingleArr_Int()
        {
            // Arrange
            var root = JObject.Parse("{ \"a\": [0, 1] }");
            var config = new JObject
            {
                { "path", "a[i]" },
                { "value", 2 }
            };
            var op = new OpConcat(config);

            // Act
            op.Exec(root);

            // Assert
            var arr = (JArray)root["a"]!;
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual(2, arr[arr.Count - 1]);
        }

        [TestMethod]
        public void Concat_Shallow_SingleArr_Str()
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
            var arr = (JArray)root["a"]!;
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual("strC", arr[arr.Count - 1]);
        }

        [TestMethod]
        public void Concat_Shallow_DoubleArr_Str()
        {
            // Arrange
            var root = new JObject
            {
                { "a", new JArray { new JArray { "strA", "strB" }, new JArray { "strC", "strD" } } }
            };
            var config = new JObject
            {
                { "path", "a[i][j]" },
                { "value", new JArray { "strA", "strB" } }
            };
            var op = new OpConcat(config);

            // Act
            op.Exec(root);

            // Assert
            var arr = (JArray)root["a"]!;
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual(2, ((JArray)arr[arr.Count - 1]).Count);
        }
    }
}

using Greed.Models.Mutations.Operations.Arrays;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations.Arrays
{
    [TestClass]
    public class ArrayIndexOfTests
    {
        #region Basic
        [TestMethod]
        public void Find_Int_Exists_First()
        {
            // Arrange
            var root = JObject.Parse("""{ "a": [0, 1] }""");
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 1,
                    "condition": {
                        "type": "EQ",
                        "params": [ "$i", 0 ]
                    }
                }
                """);
            var op = new OpArrayIndexOf(config);

            // Act
            var result = op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Find_Int_Exists_Last()
        {
            // Arrange
            var root = JObject.Parse("""{ "a": [0, 1] }""");
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 1,
                    "condition": {
                        "type": "EQ",
                        "params": [ "$i", 1 ]
                    }
                }
                """);
            var op = new OpArrayIndexOf(config);

            // Act
            var result = op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Find_Int_Missing()
        {
            // Arrange
            var root = JObject.Parse("""{ "a": [0, 1] }""");
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 1,
                    "condition": {
                        "type": "EQ",
                        "params": [ "$i", 999 ]
                    }
                }
                """);
            var op = new OpArrayIndexOf(config);

            // Act
            var result = op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(-1, result);
        }
        #endregion
    }
}
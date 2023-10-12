using Greed.Models.Mutations.Operations.Arrays;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpFilterTests
    {
        [TestMethod]
        public void Remove_Middle_By_Int_D0()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": [0,1,2] } """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_i", 1 ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var a = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(2, a.Count);
            var expected = JObject.Parse(""" { "a": [0,2] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }
    }
}

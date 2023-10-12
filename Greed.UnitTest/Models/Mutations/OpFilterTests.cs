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
        public void Remove_Solo_By_Int_D0()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": [0] } """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$i", 0 ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": [] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

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
            var expected = JObject.Parse(""" { "a": [0,2] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_First_By_Int_D0()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": [0,1,2] } """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_i", 0 ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": [1,2] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Last_By_Int_D0()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": [0,1,2] } """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_i", 2 ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": [0,1] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_D0()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": ["b", "c", "d"] } """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_i", "c" ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": ["b", "d"] } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_D1()
        {
            // Arrange
            var root = JObject.Parse(""" { "a": { "b": ["c", "d", "e"] } } """);
            var config = JObject.Parse("""
                {
                    "path": "a.b[i]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_i", "d" ]
                    }
                }
                """);
            var op = new OpFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": { "b": ["c", "e"] } } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_2D_Nested_BD_Default()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": ["c", "d", "e"]
                        },
                        {
                            "b": ["f", "g", "h"]
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i].b[j]",
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_j", "d" ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": ["c", "e"]
                         },
                         {
                             "b": ["f", "g", "h"]
                         }
                     ]
                 }
             """);

            // Act
            new OpFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_2D_Nested_BD3()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": ["c", "d", "e"]
                        },
                        {
                            "b": ["f", "g", "h"]
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i].b[j]",
                    "breakDepth": 3,
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_j", "d" ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": ["c", "e"]
                         },
                         {
                             "b": ["f", "g", "h"]
                         }
                     ]
                 }
             """);

            // Act
            new OpFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_2D_Nested_BD1()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": ["c", "d", "e"]
                        },
                        {
                            "b": ["f", "g", "h"]
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i].b[j]",
                    "breakDepth": 1,
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_j", "d" ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": ["f", "g", "h"]
                         }
                     ]
                 }
             """);

            // Act
            new OpFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_3D_Nested_BD4()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": [["c", "d"], ["e"]]
                        },
                        {
                            "b": [["f", "g"], ["h"]]
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i].b[j][k]",
                    "breakDepth": 4,
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_k", "d" ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": [["c"], ["e"]]
                         },
                         {
                             "b": [["f", "g"], ["h"]]
                         }
                     ]
                 }
             """);

            // Act
            new OpFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Remove_Middle_By_Str_3D_Nested_BD1()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": [["c", "d"], ["e"]]
                        },
                        {
                            "b": [["f", "g"], ["h"]]
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i].b[j][k]",
                    "breakDepth": 1,
                    "condition": {
                        "type": "NEQ",
                        "params": [ "$element_k", "d" ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": [["f", "g"], ["h"]]
                         }
                     ]
                 }
             """);

            // Act
            new OpFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }
    }
}

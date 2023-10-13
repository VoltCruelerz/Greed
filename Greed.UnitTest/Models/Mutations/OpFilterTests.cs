using Greed.Models.Mutations.Operations.Arrays;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpFilterTests
    {
        #region Basic Array Filtering
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
            var op = new OpArrayFilter(config);

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
            var op = new OpArrayFilter(config);

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
            var op = new OpArrayFilter(config);

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
            var op = new OpArrayFilter(config);

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
            var op = new OpArrayFilter(config);

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
            var op = new OpArrayFilter(config);

            // Act
            op.Exec(root);

            // Assert
            var expected = JObject.Parse(""" { "a": { "b": ["c", "e"] } } """);
            Assert.AreEqual(expected.ToString(), root.ToString());
        }
        #endregion

        #region Break Depth Testing
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
            new OpArrayFilter(config).Exec(root);

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
                    "resolutionDepth": 3,
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
            new OpArrayFilter(config).Exec(root);

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
                    "resolutionDepth": 1,
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
            new OpArrayFilter(config).Exec(root);

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
                    "resolutionDepth": 4,
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
            new OpArrayFilter(config).Exec(root);

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
                    "resolutionDepth": 1,
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
            new OpArrayFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }
        #endregion

        #region Element Exploration Testing

        [TestMethod]
        public void Remove_First_By_Child_Int_BD1()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [
                        {
                            "b": {
                                "c": 0
                            }
                        },
                        {
                            "b": {
                                "c": 1
                            }
                        }
                    ]
                }
            """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "resolutionDepth": 1,
                    "condition": {
                        "type": "GT",
                        "params": [ "$element_i.b.c", 0 ]
                    }
                }
                """);
            var expected = JObject.Parse("""
                 {
                     "a": [
                         {
                             "b": {
                                 "c": 1
                             }
                         }
                     ]
                 }
             """);

            // Act
            new OpArrayFilter(config).Exec(root);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }
        #endregion
    }
}

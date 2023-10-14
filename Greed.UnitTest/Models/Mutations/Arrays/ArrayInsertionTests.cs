using Greed.Exceptions;
using Greed.Models.Mutations.Operations.Arrays;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Greed.UnitTest.Models.Mutations.Arrays
{
    [TestClass]
    public class ArrayInsertionTests
    {
        #region Always
        /// <summary>
        /// Basic shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Concat_D0_SingleArr_Int()
        {
            // Arrange
            var root = JObject.Parse("""{ "a": [0, 1] }""");
            var config = new JObject
            {
                { "path", "a[i]" },
                { "value", 2 }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var expected = JObject.Parse("""{ "a": [0, 1, 2] }""");
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Concat_D0_SingleArr_Str()
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
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var arr = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual("strC", arr[arr.Count - 1]);
        }

        /// <summary>
        /// Given a[i][j], add an element to a[i]
        /// </summary>
        [TestMethod]
        public void Concat_D0_DoubleArr_Str()
        {
            // Arrange
            var root = new JObject
            {
                { "a", new JArray { new JArray { "strA", "strB" }, new JArray { "strC", "strD" } } }
            };
            var config = new JObject
            {
                { "path", "a[i]" },
                { "value", new JArray { "strE", "strF" } }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var arr = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual(2, ((JArray)arr[arr.Count - 1]).Count);
        }

        /// <summary>
        /// Given a[i][j], add an element to each [j]
        /// </summary>
        [TestMethod]
        public void Concat_ShallowDouble_DoubleArr_Str()
        {
            // Arrange
            var root = new JObject
            {
                { "a", new JArray { new JArray { "strA", "strB" }, new JArray { "strC", "strD" } } }
            };
            var config = new JObject
            {
                { "path", "a[i][j]" },
                { "value", "strE" }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var arr = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(2, arr.Count);
            Assert.AreEqual(3, ((JArray)arr[arr.Count - 1]).Count);
        }

        [TestMethod]
        public void Concat_D1_SingleArr_Int()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": [0, 1]
                    }
                }
                """);
            var config = new JObject
            {
                { "path", "a.b[i]" },
                { "value", 2 }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var b = (JArray)a["b"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, b.Count);
            Assert.AreEqual(2, b[b.Count - 1]);
        }

        [TestMethod]
        public void Concat_D2_SingleArr_Int()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": {
                            "c": [0, 1]
                        }
                    }
                }
                """);
            var config = new JObject
            {
                { "path", "a.b.c[i]" },
                { "value", 2 }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var b = (JObject)a["b"]!;
            var c = (JArray)b["c"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(2, c[c.Count - 1]);
        }

        /// <summary>
        /// Given Height -> Width -> Depth, adds to each depth.
        /// </summary>
        [TestMethod]
        public void Concat_D1_3DimensionalArr_Int_Depth()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": [
                            [[0, 1], [2, 3]],
                            [[4, 5], [6, 7]],
                        ]
                    }
                }
                """);
            var config = new JObject
            {
                { "path", "a.b[i][j][k]" },
                { "value", 8 }
            };
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var height = (JArray)a["b"]!;
            var width = (JArray)height[0]!;
            var depth = (JArray)width[0]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(2, height.Count);
            Assert.AreEqual(2, width.Count);
            Assert.AreEqual(3, depth.Count);
            Assert.AreEqual(8, depth[depth.Count - 1]);
        }

        /// <summary>
        /// Given Height -> Width -> Depth, adds to each width.
        /// </summary>
        [TestMethod]
        public void Concat_D1_3DimensionalArr_Int_Width()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": [
                            [[0, 1], [2, 3]],
                            [[4, 5], [6, 7]],
                        ]
                    }
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a.b[i][j]",
                    "value": [8,9]
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var height = (JArray)a["b"]!;
            var width = (JArray)height[0]!;
            var depth = (JArray)width[0]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(2, height.Count);
            Assert.AreEqual(3, width.Count);
            Assert.AreEqual(2, depth.Count);
        }

        /// <summary>
        /// Given Height -> Width -> Depth, adds to each width.
        /// </summary>
        [TestMethod]
        public void Concat_D1_3DimensionalArr_Int_Height()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": [
                            [[0, 1], [2, 3]],
                            [[4, 5], [6, 7]],
                        ]
                    }
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a.b[i]",
                    "value": [[8,9],[10,11]]
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var height = (JArray)a["b"]!;
            var width = (JArray)height[0]!;
            var depth = (JArray)width[0]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, height.Count);
            Assert.AreEqual(2, width.Count);
            Assert.AreEqual(2, depth.Count);
        }


        /// <summary>
        /// Given Height -> Width -> Depth, adds to each width.
        /// </summary>
        [TestMethod]
        public void Concat_D1_Disjoint_2D()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": [
                            {
                                "c": [0]
                            },
                            {
                                "c": [1]
                            }
                        ]
                    }
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a.b[i].c[j]",
                    "value": 2
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            var a = (JObject)root["a"]!;
            var b = (JArray)a["b"]!;
            var c0 = (JArray)b[0]["c"]!;
            var c1 = (JArray)b[1]["c"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(2, b.Count);
            Assert.AreEqual(2, c0.Count);
            Assert.AreEqual(2, c1.Count);
        }
        #endregion

        #region Conditional
        [TestMethod]
        public void Concat_Conditional_D1_TRUE_Var()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root);

            // Assert
            var a = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, a.Count);
        }
        [TestMethod]
        public void Concat_Conditional_D1_TRUE_Bool()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root);

            // Assert
            var a = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, a.Count);
        }

        [TestMethod]
        public void Concat_Conditional_D1_FALSE_Var()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root);

            // Assert
            var a = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, a.Count);
        }

        [TestMethod]
        public void Concat_Conditional_D1_FALSE_Bool()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2
                }
                """);
            var op = new OpArrayAppend(config);

            // Act
            op.Exec(root);

            // Assert
            var a = (JArray)root["a"]!;
            Debug.WriteLine(root.ToString());
            Assert.AreEqual(3, a.Count);
        }
        #endregion

        #region Insert Tests

        /// <summary>
        /// Shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Insert_D0_SingleArr_Int_Start()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2,
                    "index": 0
                }
                """);
            var expected = JObject.Parse("""
                {
                    "a": [2,0,1]
                }
                """);
            var op = new OpArrayInsert(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        /// <summary>
        /// Shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Insert_D0_SingleArr_Int_End()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2,
                    "index": 2
                }
                """);
            var expected = JObject.Parse("""
                {
                    "a": [0,1,2]
                }
                """);
            var op = new OpArrayInsert(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        /// <summary>
        /// Shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Insert_D0_SingleArr_Int_Middle()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2,
                    "index": 1
                }
                """);
            var expected = JObject.Parse("""
                {
                    "a": [0,2,1]
                }
                """);
            var op = new OpArrayInsert(config);

            // Act
            op.Exec(root, new Dictionary<string, Variable>());

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        /// <summary>
        /// Shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Insert_D0_SingleArr_Int_Overflow_Clamp()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2,
                    "index": 999
                }
                """);
            var expected = JObject.Parse("""
                {
                    "a": [0,1,2]
                }
                """);
            var op = new OpArrayInsert(config);

            // Act
            // Assert
            Assert.ThrowsException<ResolvableExecException>(() => op.Exec(root, new Dictionary<string, Variable>()));
        }

        /// <summary>
        /// Shallow array insertion, as seen in the manifest files.
        /// </summary>
        [TestMethod]
        public void Insert_D0_SingleArr_Int_Underflow_Clamp()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": [0,1]
                }
                """);
            var config = JObject.Parse("""
                {
                    "path": "a[i]",
                    "value": 2,
                    "index": -999
                }
                """);
            var expected = JObject.Parse("""
                {
                    "a": [2,0,1]
                }
                """);
            var op = new OpArrayInsert(config);

            // Act
            // Assert
            Assert.ThrowsException<ResolvableExecException>(() => op.Exec(root, new Dictionary<string, Variable>()));
        }
        #endregion
    }
}
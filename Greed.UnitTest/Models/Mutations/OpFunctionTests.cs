using Greed.Models.Mutations;
using Greed.Models.Mutations.Operations.Functions.Arithmetic;
using Greed.Models.Mutations.Operations.Functions.Comparison;
using Greed.Models.Mutations.Operations.Functions.Comparison.Inequalities;
using Greed.Models.Mutations.Operations.Functions.Logical;
using Greed.Models.Mutations.Operations.Functions.Sets;
using Greed.Models.Mutations.Operations.Functions.Strings;
using Greed.Models.Mutations.Operations.Functions.Variables;
using Greed.Models.Mutations.Operations.Primitive;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpFunctionTests
    {
        #region Comparison
        [TestMethod]
        public void Equals_True()
        {
            // Arrange
            var f = new OpEq(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Equals_False()
        {
            // Arrange
            var f = new OpEq(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void NotEquals_True()
        {
            // Arrange
            var f = new OpNeq(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void NotEquals_False()
        {
            // Arrange
            var f = new OpNeq(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Not_True()
        {
            // Arrange
            var f = new OpNot(new List<Resolvable> {
                new OpPrimitive(false)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Not_False()
        {
            // Arrange
            var f = new OpNot(new List<Resolvable> {
                new OpPrimitive(true)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        #region Comparison - Inequalities
        [TestMethod]
        public void Greater_True()
        {
            // Arrange
            var f = new OpGt(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Greater_False_Diff()
        {
            // Arrange
            var f = new OpGt(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Greater_False_Equals()
        {
            // Arrange
            var f = new OpGt(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Lesser_True()
        {
            // Arrange
            var f = new OpLt(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Lesser_False_Diff()
        {
            // Arrange
            var f = new OpLt(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Lesser_False_Equals()
        {
            // Arrange
            var f = new OpLt(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void GreaterEquals_True()
        {
            // Arrange
            var f = new OpGte(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void GreaterEquals_False_Diff()
        {
            // Arrange
            var f = new OpGte(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void GreaterEquals_True_Equals()
        {
            // Arrange
            var f = new OpGte(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void LesserEquals_True()
        {
            // Arrange
            var f = new OpLte(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void LesserEquals_False_Diff()
        {
            // Arrange
            var f = new OpLte(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void LesserEquals_True_Equals()
        {
            // Arrange
            var f = new OpLte(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(0)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }
        #endregion
        #endregion

        #region Logical
        [TestMethod]
        public void And_True()
        {
            // Arrange
            var f = new OpAnd(new List<Resolvable> {
                    new OpPrimitive(true),
                    new OpPrimitive(true)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void And_False()
        {
            // Arrange
            var f = new OpAnd(new List<Resolvable> {
                    new OpPrimitive(true),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Or_True()
        {
            // Arrange
            var f = new OpOr(new List<Resolvable> {
                    new OpPrimitive(true),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Or_False()
        {
            // Arrange
            var f = new OpOr(new List<Resolvable> {
                    new OpPrimitive(false),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Xor_True()
        {
            // Arrange
            var f = new OpXor(new List<Resolvable> {
                    new OpPrimitive(true),
                    new OpPrimitive(false),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Xor_False_None()
        {
            // Arrange
            var f = new OpXor(new List<Resolvable> {
                    new OpPrimitive(false),
                    new OpPrimitive(false),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void Xor_False_Two()
        {
            // Arrange
            var f = new OpXor(new List<Resolvable> {
                    new OpPrimitive(true),
                    new OpPrimitive(true),
                    new OpPrimitive(false)
                });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }
        #endregion

        #region Parameter Sets
        [TestMethod]
        public void In_True()
        {
            // Arrange
            var f = new OpIn(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void In_False()
        {
            // Arrange
            var f = new OpIn(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(1),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void NotIn_True()
        {
            // Arrange
            var f = new OpNin(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(1),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(result, new(), new()));
        }

        [TestMethod]
        public void NotIn_False()
        {
            // Arrange
            var f = new OpNin(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(0),
                new OpPrimitive(1)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(result, new(), new()));
        }
        #endregion

        #region Strings
        [TestMethod]
        public void Substring_Middle()
        {
            // Arrange
            var f = new OpStrSub(new List<Resolvable> {
                new OpPrimitive("food"),
                new OpPrimitive(1),
                new OpPrimitive(2)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual("oo", result);
        }

        [TestMethod]
        public void String_Concatenation()
        {
            // Arrange
            var f = new OpStrConcat(new List<Resolvable> {
                new OpPrimitive("food"),
                new OpPrimitive(" is "),
                new OpPrimitive("yummy")
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual("food is yummy", result);
        }
        #endregion

        #region Variables
        [TestMethod]
        public void Variable_Set()
        {
            // Arrange
            var f0 = new OpSetVar(new List<Resolvable> {
                new OpPrimitive("variable_name"),
                new OpPrimitive(0)
            });
            var f1 = new OpSetVar(new List<Resolvable> {
                new OpPrimitive("variable_name"),
                new OpPrimitive(1)
            });
            var variables = new Dictionary<string, Variable>();

            // Act
            var r0 = f0.Exec(new(), variables);
            var r1 = f1.Exec(new(), variables);

            // Assert
            Assert.AreEqual(1, variables["variable_name"].Value);
            Assert.AreEqual(0, r0);
            Assert.AreEqual(1, r1);
        }

        [TestMethod]
        public void Variable_Set_Field()
        {
            // Arrange
            var root = JObject.Parse("""
                {
                    "a": {
                        "b": 1
                    }
                }
            """);
            var config = JObject.Parse("""
                {
                    "type": "SET",
                    "params": ["$root.a.b", 2]
                }
            """);
            var expected = JObject.Parse("""
                 {
                     "a": {
                         "b": 2
                     }
                 }
            """);
            var variables = Variable.GetGlobals(root);

            // Act
            new OpSetVar(config).Exec(root, variables);

            // Assert
            Assert.AreEqual(expected.ToString(), root.ToString());
        }

        [TestMethod]
        public void Variable_Clear()
        {
            // Arrange
            var f0 = new OpClearVar(new List<Resolvable> {
                new OpPrimitive("variable_name")
            });
            var f1 = new OpClearVar(new List<Resolvable> {
                new OpPrimitive("variable_name")
            });
            var variables = new Dictionary<string, Variable>()
            {
                { "variable_name", new Variable("variable_name", 1, -1) }
            };

            // Act
            var r0 = f0.Exec(new(), variables);
            var r1 = f1.Exec(new(), variables);

            // Assert
            Assert.IsFalse(variables.ContainsKey("variable_name"));
            Assert.AreEqual(1, r0);
            Assert.AreEqual(null, r1);
        }
        #endregion

        #region Compacted Parsing

        [TestMethod]
        public void Compact_Parse_Add_1_2()
        {
            // Arrange

            // Act
            var result = Resolvable.GenerateResolvable("ADD(1,2)");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OpAdd));
            var op = (OpAdd)result;
            Assert.AreEqual(3.0, op.Exec(new(), new()));
        }

        [TestMethod]
        public void Compact_Parse_Not_1()
        {
            // Arrange

            // Act
            var result = Resolvable.GenerateResolvable("NOT(1)");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OpNot));
            var op = (OpNot)result;
            Assert.AreEqual(1, op.Parameters.Count);
            Assert.AreEqual(1, op.Parameters[0].Exec(new(), new()));
        }

        [TestMethod]
        public void Compact_Parse_Mul_Add()
        {
            // Arrange

            // Act
            var result = Resolvable.GenerateResolvable("MUL(ADD(1,2),ADD(3,4))");

            // Assert
            Assert.IsInstanceOfType(result, typeof(OpMul));
            var op = (OpMul)result;
            Assert.AreEqual(21.0, op.Exec(new(), new()));
        }

        [TestMethod]
        public void Compact_Parse_Add_All_Types()
        {
            // Arrange
            var variables = new Dictionary<string, Variable>
            {
                { "i", new Variable("i", 3, -1) }
            };

            // Act -------------------------------------------- 1    0     2 0  3 6
            var resolvable = Resolvable.GenerateResolvable("ADD(true,false,2,0,$i,MUL(2,3))");

            // Assert
            Assert.AreEqual(12.0, resolvable.Exec(new(), variables));
        }
        #endregion
    }
}

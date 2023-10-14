using Greed.Models.Mutations;
using Greed.Models.Mutations.Operations.Functions.Arithmetic;
using Greed.Models.Mutations.Operations.Primitive;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class OpArithmeticTests
    {
        [TestMethod]
        public void Add()
        {
            // Arrange
            var f = new OpAdd(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(0),
                new OpPrimitive(2)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void Sub()
        {
            // Arrange
            var f = new OpSub(new List<Resolvable> {
                new OpPrimitive(0),
                new OpPrimitive(1),
                new OpPrimitive(0),
                new OpPrimitive(2)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(-3.0, result);
        }

        [TestMethod]
        public void Mul()
        {
            // Arrange
            var f = new OpMul(new List<Resolvable> {
                new OpPrimitive(1),
                new OpPrimitive(2),
                new OpPrimitive(3)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(6.0, result);
        }

        [TestMethod]
        public void Div()
        {
            // Arrange
            var f = new OpDiv(new List<Resolvable> {
                new OpPrimitive(100),
                new OpPrimitive(10),
                new OpPrimitive(5)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(2.0, result);
        }

        [TestMethod]
        public void Mod()
        {
            // Arrange
            var f = new OpMod(new List<Resolvable> {
                new OpPrimitive(5),
                new OpPrimitive(2)
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(1.0, result);
        }

        /// <summary>
        /// (1 + 2) * (3 + 4) = 3 * 7 = 21
        /// </summary>
        [TestMethod]
        public void Chaining_Mul_Add()
        {
            // Arrange
            var f = new OpMul(new List<Resolvable> {
                new OpAdd(new List<Resolvable>
                {
                    new OpPrimitive(1),
                    new OpPrimitive(2)
                }),
                new OpAdd(new List<Resolvable>
                {
                    new OpPrimitive(3),
                    new OpPrimitive(4)
                })
            });

            // Act
            var result = f.Exec(new(), new());

            // Assert
            Assert.AreEqual(21.0, result);
        }
    }
}

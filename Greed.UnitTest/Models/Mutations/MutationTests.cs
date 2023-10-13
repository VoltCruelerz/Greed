using Greed.Models.Mutations;
using Greed.Models.Mutations.Operations.Primitive;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class MutationTests
    {
        public static readonly OpPrimitive TRUE = new(true);
        public static readonly OpPrimitive FALSE = new(false);
        public static readonly OpPrimitive NULL = new(null);

        [TestMethod]
        public void IsTruthy_True()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(Resolvable.IsTruthy(true, new(), new()));
            Assert.IsTrue(Resolvable.IsTruthy(new object(), new(), new()));
            Assert.IsTrue(Resolvable.IsTruthy(1, new(), new()));
            Assert.IsTrue(Resolvable.IsTruthy(TRUE.Exec(new JObject(), new Dictionary<string, Variable>()), new(), new()));
        }

        [TestMethod]
        public void IsTruthy_False()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsFalse(Resolvable.IsTruthy(false, new(), new()));
            Assert.IsFalse(Resolvable.IsTruthy(null, new(), new()));
            Assert.IsFalse(Resolvable.IsTruthy("", new(), new()));
            Assert.IsFalse(Resolvable.IsTruthy(0, new(), new()));
            Assert.IsFalse(Resolvable.IsTruthy(FALSE.Exec(new JObject(), new Dictionary<string, Variable>()), new(), new()));
            Assert.IsFalse(Resolvable.IsTruthy(NULL.Exec(new JObject(), new Dictionary<string, Variable>()), new(), new()));
        }
    }
}

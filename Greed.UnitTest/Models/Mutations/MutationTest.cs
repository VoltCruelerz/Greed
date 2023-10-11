using Newtonsoft.Json.Linq;
using Greed.Models.Mutations;
using Greed.Models.Mutations.Operations.Primitive;
using Greed.Models.Mutations.Variables;

namespace Greed.UnitTest.Models.Mutations
{
    [TestClass]
    public class MutationTest
    {
        [TestMethod]
        public void IsTruthy_True()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(Mutation.IsTruthy(true));
            Assert.IsTrue(Mutation.IsTruthy(new object()));
            Assert.IsTrue(Mutation.IsTruthy(1));
            Assert.IsTrue(Mutation.IsTruthy(OpPrimitive.TRUE.Exec(new JObject(), new Dictionary<string, Variable>())));
        }

        [TestMethod]
        public void IsTruthy_False()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsFalse(Mutation.IsTruthy(false));
            Assert.IsFalse(Mutation.IsTruthy(null));
            Assert.IsFalse(Mutation.IsTruthy(""));
            Assert.IsFalse(Mutation.IsTruthy(0));
            Assert.IsFalse(Mutation.IsTruthy(OpPrimitive.FALSE.Exec(new JObject(), new Dictionary<string, Variable>())));
            Assert.IsFalse(Mutation.IsTruthy(OpPrimitive.NULL.Exec(new JObject(), new Dictionary<string, Variable>())));
        }
    }
}

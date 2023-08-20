using Greed.Models;

namespace Greed.UnitTest.Models
{
    [TestClass]
    public class ModTests
    {
        [TestMethod]
        public void Import_Basic()
        {
            // Arrange
            // Act
            var mod = new Mod(new List<string>(), "..\\..\\..\\json\\mods\\modA");

            // Assert
            Assert.AreEqual(2, mod.Entities.Count);
            Assert.AreEqual(1, mod.LocalizedTexts.Count);
            Assert.AreEqual("modA", mod.Id);
        }
    }
}

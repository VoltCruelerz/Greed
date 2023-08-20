using Greed.Models;
using Newtonsoft.Json;

namespace Greed.UnitTest.Models
{
    [TestClass]
    public class MetadataTests
    {
        [TestMethod]
        public void Load_Metadata_Success()
        {
            // Arrange

            // Act
            var meta = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText("..\\..\\..\\json\\metadata\\greed.json"))!;
            var violations = meta.IsLegalVersion(new Version("1.14.3.0"));

            // Assert
            Assert.AreEqual(meta.Name, "Mod Name");
            Assert.AreEqual(meta.Author, "Author");
            Assert.AreEqual(meta.Url, "https://www.google.com");
            Assert.AreEqual(meta.Description, "If I were a rich man");
            Assert.AreEqual(meta.Version.ToString(), "1.0.0");
            Assert.AreEqual(meta.SinsVersion.ToString(), "1.14.3.0");
            Assert.AreEqual(meta.GreedVersion.ToString(), "1.1.0");
            Assert.AreEqual(0, violations.Count);
        }

        [TestMethod]
        public void Load_Deprecated_Greed()
        {
            // Arrange
            var meta = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText("..\\..\\..\\json\\metadata\\\\deprecatedGreed.json"))!;

            // Act
            var violations = meta.IsLegalVersion(new Version("1.14.3.0"));

            // Assert
            Assert.AreEqual(1, violations.Count);
        }

        [TestMethod]
        public void Load_Deprecated_Sins()
        {
            // Arrange
            var meta = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText("..\\..\\..\\json\\metadata\\\\deprecatedSins.json"))!;

            // Act
            var violations = meta.IsLegalVersion(new Version("1.14.3.0"));

            // Assert
            Assert.AreEqual(1, violations.Count);
        }
    }
}

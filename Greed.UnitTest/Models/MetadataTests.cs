using Greed.Extensions;
using Greed.Utils;
using Newtonsoft.Json;

namespace Greed.UnitTest.Models
{
    [TestClass]
    public class MetadataTests
    {
        public readonly LocalInstall ValidGreedMeta;
        public readonly LocalInstall DeprecatedGreedMeta;
        public readonly LocalInstall DeprecatedSinsMeta;

        public MetadataTests()
        {
            ValidGreedMeta = JsonConvert.DeserializeObject<LocalInstall>(File.ReadAllText("..\\..\\..\\json\\metadata\\greed.json"))!;
            ValidGreedMeta.GreedVersion = Constants.MinimumGreedVersion;
            ValidGreedMeta.SinsVersion = Constants.MinimumSinsVersion;


            DeprecatedGreedMeta = JsonConvert.DeserializeObject<LocalInstall>(File.ReadAllText("..\\..\\..\\json\\metadata\\\\deprecatedGreed.json"))!;
            DeprecatedGreedMeta.SinsVersion = Constants.MinimumSinsVersion;

            DeprecatedSinsMeta = JsonConvert.DeserializeObject<LocalInstall>(File.ReadAllText("..\\..\\..\\json\\metadata\\\\deprecatedSins.json"))!;
            DeprecatedSinsMeta.GreedVersion = Constants.MinimumGreedVersion;
        }

        [TestMethod]
        public void Load_Metadata_Success()
        {
            // Arrange

            // Act
            var violations = ValidGreedMeta.IsLegalVersion(Constants.MinimumSinsVersion);

            // Assert
            Assert.AreEqual(ValidGreedMeta.Name, "Mod Name");
            Assert.AreEqual(ValidGreedMeta.Author, "Author");
            Assert.AreEqual(ValidGreedMeta.Url, "https://www.google.com");
            Assert.AreEqual(ValidGreedMeta.Description, "If I were a rich man");
            Assert.AreEqual(ValidGreedMeta.Version.ToString(), "1.0.0");
            Assert.AreEqual(0, violations.Count, violations.Stringify());
        }

        [TestMethod]
        public void Load_Deprecated_Greed()
        {
            // Arrange

            // Act
            var violations = DeprecatedGreedMeta.IsLegalVersion(Constants.MinimumSinsVersion);

            // Assert
            Assert.AreEqual(1, violations.Count, violations.Stringify());
        }

        [TestMethod]
        public void Load_Deprecated_Sins()
        {
            // Arrange

            // Act
            var violations = DeprecatedSinsMeta.IsLegalVersion(new Version("1.14.3.0"));

            // Assert
            Assert.AreEqual(1, violations.Count, violations.Stringify());
        }
    }
}

using Greed.Interfaces;
using Moq;

namespace Greed.UnitTest.Models
{
    [TestClass]
    public class ModTests
    {
        private readonly Mock<IModManager> MockManager = new();
        private readonly Mock<IWarningPopup> MockWarning = new();
        private readonly Mock<IVault> MockVault = new();

        [TestMethod]
        public void Import_Basic()
        {
            // Arrange
            var index = 0;

            // Act
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);

            // Assert
            Assert.AreEqual(2, mod.Entities.Count);
            Assert.AreEqual(1, mod.LocalizedTexts.Count);
            Assert.AreEqual("modA", mod.Id);
        }

        #region Conflict Activation
        [TestMethod]
        public void Activate_Conflicts_Cancel()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var conflict = Helper.GetConflictMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                conflict
            };
            MockWarning
                .Setup(m => m.Conflicts(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Cancel);

            // Act
            mod.SetModActivity(mods, true);
            conflict.SetModActivity(mods, true);

            // Assert
            Assert.IsTrue(mod.IsActive);
            Assert.IsFalse(conflict.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Activate_Conflicts_DeactivateConflict()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var conflict = Helper.GetConflictMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                conflict
            };
            MockWarning
                .Setup(m => m.Conflicts(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Yes);

            // Act
            mod.SetModActivity(mods, true);
            conflict.SetModActivity(mods, true);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsTrue(conflict.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Activate_Conflicts_IgnoreConflict()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var conflict = Helper.GetConflictMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                conflict
            };
            MockWarning
                .Setup(m => m.Conflicts(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.No);

            // Act
            mod.SetModActivity(mods, true);
            conflict.SetModActivity(mods, true);

            // Assert
            Assert.IsTrue(mod.IsActive);
            Assert.IsTrue(conflict.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(2));
        }
        #endregion

        #region Dependent Activation
        [TestMethod]
        public void Activate_Dependency_Cancel()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependents(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Cancel);

            // Act
            dep.SetModActivity(mods, true);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsFalse(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Never);
        }

        [TestMethod]
        public void Activate_Dependency_ActivateDependencies()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependencies(It.IsAny<Mod>(), It.IsAny<List<string>>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Yes);

            // Act
            dep.SetModActivity(mods, true);

            // Assert
            Assert.IsTrue(mod.IsActive);
            Assert.IsTrue(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Activate_Dependency_IgnoreDependencies()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependencies(It.IsAny<Mod>(), It.IsAny<List<string>>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.No);

            // Act
            dep.SetModActivity(mods, true);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsTrue(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(1));
        }
        #endregion

        #region Dependency Deactivation
        [TestMethod]
        public void Deactivate_Dependency_Cancel()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependents(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Cancel);

            // Act
            mod.SetModActivity(mods, true);
            dep.SetModActivity(mods, true);
            mod.SetModActivity(mods, false);

            // Assert
            Assert.IsTrue(mod.IsActive);
            Assert.IsTrue(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Deactivate_Dependency_DeactivateDependents()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependents(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Yes);

            // Act
            mod.SetModActivity(mods, true);
            dep.SetModActivity(mods, true);
            mod.SetModActivity(mods, false);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsFalse(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(4));
        }

        [TestMethod]
        public void Deactivate_Dependency_IgnoreDependents()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependents(It.IsAny<Mod>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.No);

            // Act
            mod.SetModActivity(mods, true);
            dep.SetModActivity(mods, true);
            mod.SetModActivity(mods, false);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsTrue(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Exactly(3));
        }
        #endregion

        #region Future Dependent Activation
        [TestMethod]
        public void Activate_FutureDependency_ActivateDependencies()
        {
            // Arrange
            var index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var dep = Helper.GetDependentFutureMod(MockVault.Object, MockManager.Object, MockWarning.Object, ref index);
            var mods = new List<Mod>
            {
                mod,
                dep
            };
            MockWarning
                .Setup(m => m.Dependencies(It.IsAny<Mod>(), It.IsAny<List<string>>(), It.IsAny<List<Mod>>()))
                .Returns(() => System.Windows.MessageBoxResult.Yes);

            // Act
            dep.SetModActivity(mods, true);

            // Assert
            Assert.IsFalse(mod.IsActive);
            Assert.IsFalse(dep.IsActive);
            MockVault.Verify(m => m.ArchiveActiveOnly(It.IsAny<List<Mod>>()), Times.Never);
            MockWarning.Verify(m => m.FailedToResolveDependencies(It.IsAny<List<string>>()), Times.Once);
        }
        #endregion
    }
}

using Greed.Interfaces;
using Moq;
using System.Configuration;

namespace Greed.UnitTest
{
    [TestClass]
    public class ModManagerTests
    {
        private readonly Mock<IWarningPopup> MockWarning = new();
        private readonly ModManager Manager;

        public ModManagerTests()
        {
            Manager = new ModManager(MockWarning.Object);

            var mockModDir = Directory.GetCurrentDirectory() + "\\mods";
            if (Directory.Exists(mockModDir))
            {
                Directory.Delete(mockModDir, true);
            }
            Directory.CreateDirectory(mockModDir);
            ConfigurationManager.AppSettings["modDir"] = mockModDir;
        }

        #region Filler Move
        /// <summary>
        /// Move filler up
        /// </summary>
        [TestMethod]
        public void Move_SimpleReorderUp_NoDep()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(mods, filler, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move filler down
        /// </summary>
        [TestMethod]
        public void Move_SimpleReorderDown_NoDep()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(mods, filler, 2);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }
        #endregion

        #region Dependent Up/Down 1
        /// <summary>
        /// Move dependent up 1, no violation
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp1_Dependent()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(mods, dependent, 1);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }
        /// <summary>
        /// Move dependent down 1, user allows
        /// </summary>
        [TestMethod]
        public void Move_ReorderDown1_Dependent_Allow()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(mods, dependent, 3);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }
        #endregion

        #region Dependent Up 2
        /// <summary>
        /// Move dependent up 2, force hoist of dependency, user allows
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp2_Dependent_Allow()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(mods, dependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move dependent up 2, force hoist of dependency, user rejects
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp2_Dependent_Reject()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(mods, dependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move dependent up 2, force hoist of dependency, user forces bad order
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp2_Dependent_Force()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(mods, dependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }
        #endregion

        #region Grandependent Up 3
        /// <summary>
        /// Move grandependent up 3, trigger recursive hoist of dependency, user allows
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp3_Grandependent_Allow()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(mods, grandependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
        }

        /// <summary>
        /// Move grandependent up 3, trigger recursive hoist of dependency, user rejects
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp3_Grandependent_Reject()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(mods, grandependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move grandependent up 3, trigger recursive hoist of dependency, user forces bad order
        /// </summary>
        [TestMethod]
        public void Move_ReorderUp3_Grandependent_Force()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(mods, grandependent, 0);

            // Assert
            int pos = 0;
            Assert.AreEqual(grandependent, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
        }
        #endregion

        #region Dependency Down 2
        /// <summary>
        /// Move dependency down 3, trigger recursive hoist of dependency, user allows
        /// </summary>
        [TestMethod]
        public void Move_ReorderDown2_Dependency_Allow()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(mods, dependency, 2);

            // Assert
            int pos = 0;
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }
        #endregion

        #region Dependency Down 3
        /// <summary>
        /// Move dependency down 3, trigger recursive hoist of dependency, user allows
        /// </summary>
        [TestMethod]
        public void Move_ReorderDown3_Dependency_Allow()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(mods, dependency, 3);

            // Assert
            int pos = 0;
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move dependency down 3, trigger recursive hoist of dependency, user rejects
        /// </summary>
        [TestMethod]
        public void Move_ReorderDown3_Dependency_Reject()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(mods, dependency, 3);

            // Assert
            int pos = 0;
            Assert.AreEqual(dependency, mods[pos++]);
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
        }

        /// <summary>
        /// Move dependency down 3, trigger recursive hoist of dependency, user forces bad order
        /// </summary>
        [TestMethod]
        public void Move_ReorderDown3_Dependency_Force()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.WarnOfDependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(mods, dependency, 3);

            // Assert
            int pos = 0;
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
        }
        #endregion

        #region Helpers
        private List<Mod> ActivateAndSync(List<Mod> mods)
        {
            mods.ForEach(m =>
            {
                m.SetModActivity(mods, true);
            });
            Manager.SyncLoadOrder(mods);
            return mods;
        }
        #endregion
    }
}

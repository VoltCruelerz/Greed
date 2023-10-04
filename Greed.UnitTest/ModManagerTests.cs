using Greed.Interfaces;
using Moq;
using System.Configuration;

namespace Greed.UnitTest
{
    [TestClass]
    public class ModManagerTests
    {
        private readonly Mock<IWarningPopup> MockWarning = new();
        private readonly Mock<IVault> MockVault = new();
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

        #region Reorder Mod List
        #region Filler Move
        /// <summary>
        /// Move filler up
        /// </summary>
        [TestMethod]
        public void Move_SimpleReorderUp_NoDep()
        {
            // Arrange
            var index = 0;
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(MockVault.Object, mods, filler, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(MockVault.Object, mods, filler, 2);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependent, 1);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependent, 3);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.Is<Mod>(m => m == dependent), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(MockVault.Object, mods, grandependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(MockVault.Object, mods, grandependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(MockVault.Object, mods, grandependent, 0);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependentOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependency, 2);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependentOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Yes);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependency, 3);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependencyOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.Cancel);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependency, 3);

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
            var dependency = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var filler = Helper.GetFillerMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var dependent = Helper.GetDependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var grandependent = Helper.GetGrandependentMod(MockVault.Object, Manager, MockWarning.Object, ref index);

            var mods = ActivateAndSync(new List<Mod>()
            {
                dependency,
                filler,
                dependent,
                grandependent
            });

            MockWarning
                .Setup(w => w.DependentOrder(It.IsAny<Mod>(), It.IsAny<List<string>>()))
                .Returns(System.Windows.MessageBoxResult.No);

            // Act
            Manager.MoveMod(MockVault.Object, mods, dependency, 3);

            // Assert
            int pos = 0;
            Assert.AreEqual(filler, mods[pos++]);
            Assert.AreEqual(dependent, mods[pos++]);
            Assert.AreEqual(grandependent, mods[pos++]);
            Assert.AreEqual(dependency, mods[pos++]);
        }
        #endregion
        #endregion

        [TestMethod]
        public void IsInstalled_Yes()
        {
            // Arrange
            int index = 0;
            var mod = Helper.GetBasicMod(MockVault.Object, Manager, MockWarning.Object, ref index);
            var srcPath = Helper.ModsFolder + "\\modA";
            var destPath = ConfigurationManager.AppSettings["modDir"] + "\\modA";
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            CopyDirectory(srcPath, destPath, true);

            // Act
            var result = ModManager.IsModInstalled(mod.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInstalled_No()
        {
            // Arrange

            // Act
            var result = ModManager.IsModInstalled("sldkfjsldkfjsldkfjsldkfjsdkfj");

            // Assert
            Assert.IsFalse(result);
        }

        #region Helpers
        private List<Mod> ActivateAndSync(List<Mod> mods)
        {
            mods.ForEach(m =>
            {
                m.SetModActivity(mods, true);
            });
            ModManager.SyncLoadOrder(MockVault.Object, mods);
            return mods;
        }

        /// <summary>
        /// Recursively copies all contents from the source to the destination
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="purgeExisting"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void CopyDirectory(string sourceDir, string destDir, bool purgeExisting = false)
        {
            if (Directory.Exists(destDir))
            {
                if (purgeExisting)
                {
                    Directory.Delete(destDir, true);
                }
                else
                {
                    throw new InvalidOperationException("The destination folder already exists. Please delete it or pass purgeExisting=true");
                }
            }
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile);
            }

            foreach (string subDirectory in Directory.GetDirectories(sourceDir))
            {
                string destSubDirectory = Path.Combine(destDir, Path.GetFileName(subDirectory));
                CopyDirectory(subDirectory, destSubDirectory, purgeExisting);
            }
        }
        #endregion
    }
}

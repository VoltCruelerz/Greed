using Greed.Interfaces;

namespace Greed.UnitTest
{
    public class Helper
    {
        public static string ModsFolder = "..\\..\\..\\json\\mods";

        public static Mod GetBasicMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modA", ref index);
        }
        public static Mod GetFillerMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modFiller", ref index);
        }
        public static Mod GetConflictMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modConflict", ref index);
        }
        public static Mod GetDependentMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modDependent", ref index);
        }
        public static Mod GetGrandependentMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modGrandependent", ref index);
        }
        public static Mod GetDependentFutureMod(IVault vault, IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(vault, manager, warning, new List<string>(), ModsFolder + "\\modDependentFuture", ref index);
        }
    }
}

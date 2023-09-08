using Greed.Interfaces;

namespace Greed.UnitTest
{
    public class Helper
    {
        public static string ModsFolder = "..\\..\\..\\json\\mods";

        public static Mod GetBasicMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modA", ref index);
        }
        public static Mod GetFillerMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modFiller", ref index);
        }
        public static Mod GetConflictMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modConflict", ref index);
        }
        public static Mod GetDependentMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modDependent", ref index);
        }
        public static Mod GetGrandependentMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modGrandependent", ref index);
        }
        public static Mod GetDependentFutureMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), ModsFolder + "\\modDependentFuture", ref index);
        }
    }
}

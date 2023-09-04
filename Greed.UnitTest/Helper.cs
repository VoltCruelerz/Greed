using Greed.Interfaces;

namespace Greed.UnitTest
{
    public class Helper
    {
        public static Mod GetBasicMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modA", ref index);
        }
        public static Mod GetFillerMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modFiller", ref index);
        }
        public static Mod GetConflictMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modConflict", ref index);
        }
        public static Mod GetDependentMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modDependent", ref index);
        }
        public static Mod GetGrandependentMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modGrandependent", ref index);
        }
        public static Mod GetDependentFutureMod(IModManager manager, IWarningPopup warning, ref int index)
        {
            return new Mod(manager, warning, new List<string>(), "..\\..\\..\\json\\mods\\modDependentFuture", ref index);
        }
    }
}

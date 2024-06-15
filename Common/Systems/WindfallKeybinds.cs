namespace Windfall.Common.Systems
{
    public class WindfallKeybinds : ModSystem
    {
        public static ModKeybind GodlyDashHotkey { get; private set; }
        public static ModKeybind GodlyHarvestHotkey { get; private set; }
        public static ModKeybind GodlyAttack1Hotkey { get; private set; }

        public override void Load()
        {
            GodlyDashHotkey = KeybindLoader.RegisterKeybind(Mod, "GodlyEssenceDash", "F");
            GodlyHarvestHotkey = KeybindLoader.RegisterKeybind(Mod, "GodlyEssenceHarvest", "R");
            GodlyAttack1Hotkey = KeybindLoader.RegisterKeybind(Mod, "GodlyEssenceAttack1", "C");
        }
        public override void Unload()
        {
            GodlyDashHotkey = null;
            GodlyHarvestHotkey = null;
            GodlyAttack1Hotkey = null;
        }
    }
}

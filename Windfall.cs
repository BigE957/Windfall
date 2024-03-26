namespace Windfall
{
    public class Windfall : Mod
    {
        internal static Windfall Instance;
        public override void Load()
        {
            Instance = this;
        }
        public override void Unload()
        {
            Instance = null;
        }
        public override void PostSetupContent()
        {
            ModLoader.GetMod("CalamityMod").Call("RegisterModCooldowns", this);
        }
    }
}
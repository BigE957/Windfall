using CalamityMod.Skies;
using Terraria.Graphics.Effects;
using Windfall.Content.Skies;

namespace Windfall
{
    public class Windfall : Mod
    {
        internal static Windfall Instance;
        public override void Load()
        {
            Instance = this;
            SkyManager.Instance["Windfall:Orator"] = new OratorSky();
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
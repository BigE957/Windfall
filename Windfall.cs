using Terraria.ModLoader;

namespace Windfall
{
	public class Windfall : Mod
	{
        internal static Windfall Instance;
        public override void Load()
        {
            base.Load();
            Instance = this;
        }
        public override void Unload()
        {
            base.Unload();
            Instance = null;
        }

    }
}
using CalamityMod.UI;
using System.Reflection;
using Windfall.Content.UI.Events;

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
    }
}
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
        public override void PostSetupContent()
        {
            ModLoader.GetMod("CalamityMod").Call("RegisterModCooldowns", this);
            FieldInfo InvasionGUIsFieldInfo = typeof(InvasionProgressUIManager).GetField("gUIs", BindingFlags.NonPublic | BindingFlags.Static);
            List<InvasionProgressUI> guis = ((List<InvasionProgressUI>)InvasionGUIsFieldInfo.GetValue(null));
            guis.Add(Activator.CreateInstance(typeof(TailorEventBar)) as InvasionProgressUI);
            guis.Add(Activator.CreateInstance(typeof(CafeteriaEventBar)) as InvasionProgressUI); 
            guis.Add(Activator.CreateInstance(typeof(RitualEventBar)) as InvasionProgressUI);
        }
    }
}
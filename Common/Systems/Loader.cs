using CalamityMod.UI;
using MonoMod.Utils;
using System.Reflection;
using Terraria.Graphics.Effects;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Skies;
using Windfall.Content.Skies.CorruptCommunion;
using Windfall.Content.Skies.CrimsonCommunion;
using Windfall.Content.Skies.SlimyCommunion;
using DialogueHelper.Content.UI.Dialogue;
using Windfall.Content.UI.Events;
using System.Text.Json;
using Terraria.ModLoader;
using DialogueHelper.Content.UI.Dialogue.DialogueStyles;

namespace Windfall.Common.Systems
{
    public class Loading : ModSystem
    {
        public override void Load()
        {
            #region Custom Skies
            SkyManager.Instance["Windfall:Orator"] = new OratorSky();

            Filters.Scene["Windfall:CorruptCommunion"] = new Filter(new CorruptCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CorruptCommunion"] = new CorruptCommunionSky();

            Filters.Scene["Windfall:CrimsonCommunion"] = new Filter(new CrimsonCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.2f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CrimsonCommunion"] = new CrimsonCommunionSky();


            Filters.Scene["Windfall:SlimyCommunion"] = new Filter(new SlimyCommunionScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.4f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:SlimyCommunion"] = new SlimyCommunionSky();
            #endregion
        }
        public override void PostSetupContent()
        {
            #region Event Bars
            ModLoader.GetMod("CalamityMod").Call("RegisterModCooldowns", this);
            FieldInfo InvasionGUIsFieldInfo = typeof(InvasionProgressUIManager).GetField("gUIs", BindingFlags.NonPublic | BindingFlags.Static);
            List<InvasionProgressUI> guis = ((List<InvasionProgressUI>)InvasionGUIsFieldInfo.GetValue(null));
            guis.Add(Activator.CreateInstance(typeof(TailorEventBar)) as InvasionProgressUI);
            guis.Add(Activator.CreateInstance(typeof(CafeteriaEventBar)) as InvasionProgressUI);
            guis.Add(Activator.CreateInstance(typeof(RitualEventBar)) as InvasionProgressUI);
            #endregion
        }
    }
}

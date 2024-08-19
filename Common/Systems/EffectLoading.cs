using Terraria.Graphics.Effects;
using Windfall.Content.Skies;
using Windfall.Content.Skies.CorruptCommunion;
using Windfall.Content.Skies.CrimsonCommunion;
using Windfall.Content.Skies.ScreenShaders;

namespace Windfall.Common.Systems
{
    public class EffectLoading : ModSystem
    {
        public override void Load()
        {
            SkyManager.Instance["Windfall:Orator"] = new OratorSky();

            Filters.Scene["Windfall:CorruptCommunion"] = new Filter(new CorruptCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CorruptCommunion"] = new CorruptCommunionSky();

            Filters.Scene["Windfall:CrimsonCommunion"] = new Filter(new CrimsonCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.2f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CrimsonCommunion"] = new CrimsonCommunionSky();


            Filters.Scene["Windfall:SlimyCommunion"] = new Filter(new SlimyCommunionScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.4f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:SlimyCommunion"] = new SlimyCommunionSky();
        }
    }
}

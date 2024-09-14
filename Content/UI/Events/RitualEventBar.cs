using CalamityMod.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.UI.Events
{
    public class RitualEventBar : InvasionProgressUI
    {
        public override bool IsActive => LunarCultBaseSystem.IsRitualActivityActive();
        public override float CompletionRatio => Clamp((float)LunarCultBaseSystem.PortalsDowned / (float)LunarCultBaseSystem.RequiredPortalKills, 0f, 1f);
        public override string InvasionName => GetWindfallTextValue("Events.Ritual");
        public override Color InvasionBarColor => Color.LimeGreen;
        public override Texture2D IconTexture => ModContent.Request<Texture2D>("Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head").Value;
    }
}

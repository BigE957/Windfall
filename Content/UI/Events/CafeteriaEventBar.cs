using CalamityMod.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.UI.Events
{
    public class CafeteriaEventBar : InvasionProgressUI
    {
        public override bool IsActive => LunarCultActivitySystem.IsCafeteriaActivityActive();
        public override float CompletionRatio => (float)LunarCultActivitySystem.SatisfiedCustomers / (float)LunarCultActivitySystem.CustomerGoal;
        public override string InvasionName => GetWindfallTextValue("Events.Cafeteria");
        public override Color InvasionBarColor => Color.LimeGreen;
        public override Texture2D IconTexture => ModContent.Request<Texture2D>("Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head").Value;
    }
}

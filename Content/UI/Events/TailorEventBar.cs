using CalamityMod.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.UI.Events
{
    public class TailorEventBar : InvasionProgressUI
    {
        public override bool IsActive => LunarCultActivitySystem.IsTailorActivityActive() && Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Seamstress>())].ai[0] == 0;
        public override float CompletionRatio => (float)LunarCultActivitySystem.CompletedClothesCount / (float)LunarCultActivitySystem.ClothesGoal;
        public override string InvasionName => GetWindfallTextValue("Events.Tailor");
        public override Color InvasionBarColor => Color.LimeGreen;
        public override Texture2D IconTexture => ModContent.Request<Texture2D>("Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head").Value;
    }
}

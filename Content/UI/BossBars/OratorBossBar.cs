using Terraria.GameContent.UI.BigProgressBar;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.UI.BossBars
{
    public class OratorBossBar : ModBossBar
    {
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC target = Main.npc[info.npcIndexToAimAt];
            if (!target.active)
                return false;

            // Get the boss health, obviously
            life = target.life;
            lifeMax = target.lifeMax;

            // Reset the shield
            shield = 0f;
            shieldMax = 0f;

            // Determine the shield health
            if(Main.npc.Any(n => n != null && n.active && n.type == ModContent.NPCType<OratorHand>()))
            {
                NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == ModContent.NPCType<OratorHand>());
                shield = mainHand.life;
                shieldMax = mainHand.lifeMax;
            }
            return true;
        }
    }
}

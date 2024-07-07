using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Common.Players
{
    public class DeadPlayer : ModPlayer
    {
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if(Player.Buff().Entropy)
                    damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Entropy." + Main.rand.Next(1, 3 + 1)).Format(Player.name));
            }
            if (damageSource.SourceNPCIndex >= 0)
            {
                if (Main.npc[damageSource.SourceNPCIndex].type == ModContent.NPCType<TheOrator>())
                    damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Orator." + Main.rand.Next(1, 3 + 1)).Format(Player.name));
            }
            return true;
        }
    }
}

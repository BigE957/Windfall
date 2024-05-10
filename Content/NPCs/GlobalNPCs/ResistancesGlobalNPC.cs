using CalamityMod.Projectiles.Rogue;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.NPCs.GlobalNPCs
{
    public class ResistancesGlobalNPC : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitByItem(npc, player, item, ref modifiers);
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if(npc.type == ModContent.NPCType<TheOrator>())
            {
                if(projectile.type ==  ModContent.ProjectileType<DukesDecapitatorProj>())
                {
                    modifiers.SourceDamage *= 0.6f;
                }
            }
        }
    }
}

using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Rogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.NPCs.GlobalNPCs
{
    public class ResistancesGlobalNPC : GlobalNPC
    {
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

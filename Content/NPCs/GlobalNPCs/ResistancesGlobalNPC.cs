using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Windfall.Content.Items.Weapons.Summon;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.NPCs.GlobalNPCs;

public class ResistancesGlobalNPC : GlobalNPC
{
    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (npc.type == ModContent.NPCType<TheOrator>())
        {
            if (item.IsTrueMelee())
                modifiers.SourceDamage *= 0.75f;
        }
    }
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if(npc.type == ModContent.NPCType<TheOrator>())
        {
            if(projectile.type ==  ModContent.ProjectileType<DukesDecapitatorProj>() || projectile.type == ModContent.ProjectileType<DukesDecapitatorBubble>())
                modifiers.SourceDamage *= 0.2f;

            if (projectile.IsTrueMelee())
                modifiers.SourceDamage *= 0.75f;
        }

        if(npc.type == ModContent.NPCType<AstrumDeusBody>() || npc.type == ModContent.NPCType<AstrumDeusHead>() || npc.type == ModContent.NPCType<AstrumDeusTail>())
        {
            if (projectile.type == ModContent.ProjectileType<ShadowHand_Minion>())
                modifiers.SourceDamage *= 0.2f;
        }
    }
}

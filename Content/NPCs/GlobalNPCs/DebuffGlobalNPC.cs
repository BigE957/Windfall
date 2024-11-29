using Windfall.Content.Projectiles.Weapons.Rogue;

namespace Windfall.Content.NPCs.GlobalNPCs;
public class DebuffGlobalNPC : GlobalNPC
{
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (npc.onFrostBurn2)
        {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            int projectileCount = Main.projectile.Where(p => p.active && p.ai[0] == 1f && p.ai[1] == npc.whoAmI && (
                    p.type == ModContent.ProjectileType<OratorSpearProj>()
                )).Count();

            if (projectileCount > 0)
            {                
                npc.lifeRegen -= projectileCount * 25;

                if (damage < projectileCount * 5)
                    damage = projectileCount * 5;
            }
        }
    }
}

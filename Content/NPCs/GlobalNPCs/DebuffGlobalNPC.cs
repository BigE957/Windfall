using CalamityMod;
using CalamityMod.NPCs;
using Windfall.Content.Items.Weapons.Rogue;

namespace Windfall.Content.NPCs.GlobalNPCs;
public class DebuffGlobalNPC : GlobalNPC
{
    public bool Entropy = false;
    public bool Wildfire = false;

    public override bool InstancePerEntity => true;

    public override void ResetEffects(NPC npc)
    {
        Entropy = false;
        Wildfire = false;
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (npc.onFrostBurn2)
        {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            int projectileCount = Main.projectile.Where(p => p.active && p.ai[0] == 1f && p.ai[1] == npc.whoAmI && (
                    p.type == ModContent.ProjectileType<ExodiumSpearProj>()
                )).Count();

            if (projectileCount > 0)
            {                
                npc.lifeRegen -= projectileCount * 25;

                if (damage < projectileCount * 5)
                    damage = projectileCount * 5;
            }
        }

        if(Entropy)
        {
            int DoT = npc.lifeMax / 3;
            npc.Calamity().ApplyDPSDebuff(DoT, DoT / 10, ref npc.lifeRegen, ref damage);
        }

        if(Wildfire)
        {
            int WildfireDamage = (int)(npc.Calamity().HeatDebuffMultiplier.ApplyTo(100));
            npc.Calamity().ApplyDPSDebuff(WildfireDamage, WildfireDamage / 5, ref npc.lifeRegen, ref damage);
        }
    }
}

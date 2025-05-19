
using CalamityMod.Projectiles.Enemy;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Projectiles;

public class WindfallGlobalProjectile : GlobalProjectile
{
    public bool hasHitMuck = false;
    public override void SetDefaults(Projectile entity)
    {
        base.SetDefaults(entity);
        hasHitMuck = false;
    }
    public override bool InstancePerEntity => true;
    public override bool PreAI(Projectile projectile)
    {
        //Noticed Acid Bubbles were spawning and causing my game to lag. So I added a fix. No clue why they are spawning, but someone shoudl prolly do smth about that.
        if (projectile.type == ModContent.ProjectileType<SulphuricAcidBubble>())
        {
            Player p = Main.player[Player.FindClosest(projectile.position, projectile.width, projectile.height)];
            if(p.Center.DistanceSQ(projectile.Center) > 1440000)
                projectile.active = false;
        }
        
        if (LunarCultBaseSystem.CultBaseTileArea.Intersects(new((int)projectile.position.X / 16, (int)projectile.position.Y / 16, projectile.width / 16, projectile.height / 16)) || LunarCultBaseSystem.CultBaseBridgeArea.Intersects(new((int)projectile.position.X / 16, (int)projectile.position.Y / 16, projectile.width / 16, projectile.height / 16)) || DraconicRuinsSystem.DraconicRuinsArea.Intersects(new((int)projectile.position.X / 16, (int)projectile.position.Y / 16, projectile.width / 16, projectile.height / 16)))
        {
            if (projectile.type == ProjectileID.SandBallFalling || projectile.type == ProjectileID.SiltBall || projectile.type == ProjectileID.SlushBall || ProjectileID.Sets.IsAGravestone[projectile.type])
            {
                projectile.active = false;
                return false;
            }
        }

        return base.PreAI(projectile);
    }
}

﻿
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
        if (LunarCultBaseSystem.CultBaseTileArea.Intersects(new((int)projectile.position.X / 16, (int)projectile.position.Y / 16, projectile.width, projectile.height)) || LunarCultBaseSystem.CultBaseBridgeArea.Intersects(new((int)projectile.position.X / 16, (int)projectile.position.Y / 16, projectile.width, projectile.height)))
        {
            if (projectile.type == ProjectileID.SandBallFalling || projectile.type == ProjectileID.SiltBall || projectile.type == ProjectileID.SlushBall)
            {
                projectile.active = false;
                return false;
            }
        }
        return base.PreAI(projectile);
    }
}

using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Content.Projectiles.Other;

public class HideoutSeeker : ModProjectile
{
    public override string Texture => "Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head";
    public new static string LocalizationCategory => "Projectiles.Other";
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 200;
    }
    public override void AI()
    {
        if (ActivityCoords != new Point(-1, -1))
        {
            Vector2 DistanceVector = new(ActivityCoords.X - Projectile.Center.X, ActivityCoords.Y - Projectile.Center.Y);
            Projectile.velocity += DistanceVector.SafeNormalize(Vector2.UnitX);
        }

        int dustType = DustID.GoldFlame;
        Dust.NewDustPerfect(Projectile.Center, dustType);
    }
}

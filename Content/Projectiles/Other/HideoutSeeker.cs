using Windfall.Common.Systems.WorldEvents;
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
        Vector2 DistanceVector = new(CultBaseWorldArea.Center.X - Projectile.Center.X, CultBaseWorldArea.Center.Y - Projectile.Center.Y);
        Projectile.velocity += DistanceVector.SafeNormalize(Vector2.Zero);
        if (Projectile.velocity.LengthSquared() > 256)
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 16;

        int dustType = DustID.DungeonSpirit;
        Dust.NewDustPerfect(Projectile.Center, dustType).noGravity = true;
    }
}

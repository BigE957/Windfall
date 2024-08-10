using static Windfall.Common.Systems.WorldEvents.LunarCultActivitySystem;

namespace Windfall.Content.Projectiles.Other
{
    public class HideoutSeeker : ModProjectile
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_Head";
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
            if (SolarHideoutLocation.X == ActivityCoords.X / 16)
                dustType = DustID.SolarFlare;
            else if (VortexHideoutLocation.X == ActivityCoords.X / 16)
                dustType = DustID.Vortex;
            else if (NebulaHideoutLocation.X == ActivityCoords.X / 16)
                dustType = DustID.Gastropod;
            else if (StardustHideoutLocation.X == ActivityCoords.X / 16)
                dustType = DustID.BlueFlare;
            Dust.NewDustPerfect(Projectile.Center, dustType);
        }
    }
}

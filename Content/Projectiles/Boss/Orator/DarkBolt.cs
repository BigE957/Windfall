using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkBolt : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Rogue/DestructionBolt";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.damage = 100;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 250;
            Projectile.tileCollide = false;
        }
        private int aiCounter
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float Velocity
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        Vector2 DirectionalVelocity = Vector2.Zero;
        public override void AI()
        {
            if (aiCounter == 0)
                DirectionalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.rotation = DirectionalVelocity.ToRotation() + PiOver2;
            Projectile.spriteDirection = (int)Projectile.rotation;
            Projectile.velocity = DirectionalVelocity.SafeNormalize(Vector2.UnitX) * (Velocity / 2);
            Velocity += 1f;
            aiCounter++;
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
    }
}

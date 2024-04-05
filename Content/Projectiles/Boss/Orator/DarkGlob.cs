using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkGlob : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.damage = 100;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 390;
            Projectile.scale = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        private float Velocity
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                if (Velocity > 0)
                {
                    Projectile.hostile = false;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Velocity;
                    Velocity -= 0.2f;
                    Projectile.scale += 1 / 100f;
                    EmpyreanMetaball.SpawnParticle(Projectile.Center - Projectile.velocity * (2 * Projectile.scale), Vector2.Zero, 30 * (Projectile.scale * 2));
                }
                else
                {
                    if (Projectile.scale < 0.5f && Projectile.timeLeft > 30)
                        Projectile.scale = 0.5f;
                    Projectile.hostile = true;
                    Projectile.velocity = Vector2.Zero;
                    Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
                    if (Projectile.timeLeft < 30)
                    {
                        Projectile.hostile = false;
                        Projectile.scale -= 1 / 60f;
                    }
                }
            }
            else
            {
                if (Projectile.scale < 0.5f)
                {
                    Projectile.scale += 1 / 60f;
                    Projectile.timeLeft = 100;
                }
                else
                    Projectile.hostile = true;
                Projectile.velocity.Y += 0.3f;
                EmpyreanMetaball.SpawnParticle(Projectile.Center, Vector2.Zero, 30 * (Projectile.scale * 2));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Projectiles.GodlyAbilities
{
    public class GodlyEbonianGlob : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Boss/UnstableEbonianGlob";
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.alpha = 60;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 45;
        }
        public NPC Target = null;
        private Vector2 positionDiff = Vector2.Zero;
        public override void AI()
        {
            if (Target == null)
            {
                Projectile.velocity.X *= 0.985f;
                Projectile.velocity.Y *= 0.985f;
                Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.05f;
            }
            else
            {
                if (Target.active)
                {
                    Projectile.Center = Target.Center + positionDiff.RotatedBy(Target.rotation);
                    Projectile.velocity = Target.velocity;
                }
                else
                {
                    Target = null;
                    Projectile.velocity = Main.rand.NextVector2Circular(10f, 10f);
                }
            }
            if (Main.rand.NextBool())
            {
                Color dustColor = Color.Lavender;
                dustColor.A = 150;
                int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.TintableDust, 0f, 0f, Projectile.alpha, dustColor);
                Main.dust[dust].noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Target == null)
            {
                Target = target;
                positionDiff = new Vector2(Main.rand.NextFloat(-Target.Hitbox.Width / 2, Target.Hitbox.Width / 2), Main.rand.NextFloat(-Target.Hitbox.Height / 2, Target.Hitbox.Height / 2));
                Projectile.velocity = Vector2.Zero;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor.R = (byte)(255 * Projectile.Opacity);
            lightColor.G = (byte)(255 * Projectile.Opacity);
            lightColor.B = (byte)(255 * Projectile.Opacity);
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            for (int k = 0; k < 10; k++)
            {
                Color dustColor = Color.Lavender;
                dustColor.A = 150;
                Dust.NewDustPerfect(Projectile.Center, DustID.TintableDust, Main.rand.NextVector2Circular(5f, 5f), Projectile.alpha, dustColor);
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if(Target == null)
                return true;
            else
                if(target == Target)
                    return true;
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Content.Buffs.DoT;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkTide : ModProjectile
    {
        public ref float counter => ref Projectile.ai[1];
        private Vector2 initialPos;
        public override void SetDefaults()
        {
            Projectile.height = Projectile.width = 900;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.ai[0];
            Vector2 newPosition = Projectile.rotation.ToRotationVector2().RotatedBy(Pi) * 1800;
            Projectile.Center += newPosition;
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 4f;
        }
        public override void AI()
        {
            int slideDuration = 330;
            int holdDuration = 120;
            if(counter > slideDuration)
            {
                if (counter > slideDuration + holdDuration)
                    if(counter > slideDuration * 2 + holdDuration)
                        Projectile.active = false;
                    else
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * -4f;
                else
                    Projectile.velocity = Vector2.Zero;
            }
            foreach(Player player in Main.player)
            {
                if(player != null && player.active && !player.dead)
                {
                    if(player.Hitbox.Intersects(Projectile.Hitbox))
                        player.AddBuff(ModContent.BuffType<Entropy>(), 5);
                }
            }
            counter++;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
    }
}

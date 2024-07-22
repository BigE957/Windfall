using Windfall.Content.Buffs.DoT;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;


namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkTide : ModProjectile
    {
        public ref float counter => ref Projectile.ai[1];
        public override void SetDefaults()
        {
            Projectile.height = Projectile.width = 1440;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 1.25f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.ai[0];
            Vector2 newPosition = Projectile.rotation.ToRotationVector2().RotatedBy(Pi) * (1800 * Projectile.scale);
            Projectile.Center += newPosition;
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 4f;

            int particleCounter = 50;
            for(int i = 0; i < particleCounter; i++)
            {
                Vector2 spawnOffset = (Projectile.rotation.ToRotationVector2() * (Projectile.width / 2.7f)) + (Projectile.rotation.ToRotationVector2().RotatedBy(PiOver2) * ((Projectile.width / 2) - Projectile.width / particleCounter * i));
                SpawnBorderParticle(Projectile, spawnOffset, 0.5f * i, 30, Main.rand.NextFloat(80, 160), 0f, false);
            }
        }
        public override void AI()
        {
            const int timeToMid = 330;
            int slideDuration = timeToMid + 60;
            int holdDuration = 120;
            float velocityMultiplier = 1f;
            if (counter > slideDuration)
            {
                if (counter > slideDuration + holdDuration)
                    if (counter > slideDuration * 2 + holdDuration)
                        Projectile.active = false;
                    else
                    {
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * -4f;
                        velocityMultiplier = Main.rand.NextFloat(2f, 4f);
                    }
                else
                {
                    Projectile.velocity = Vector2.Zero;
                    velocityMultiplier = Main.rand.NextFloat(6f, 8f);
                }
            }
            else
                velocityMultiplier = Main.rand.NextFloat(9f, 12f);
            foreach (Player player in Main.player)
            {
                if(player != null && player.active && !player.dead)
                {
                    if(!isLeft(Projectile.Center + new Vector2(Projectile.width / 2, Projectile.height / 2).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(Projectile.width / 2, -Projectile.height / 2).RotatedBy(Projectile.rotation), player.Center))
                        player.AddBuff(ModContent.BuffType<Entropy>(), 5);
                }
            }
            Vector2 spawnPosition = Projectile.Center + (Projectile.rotation.ToRotationVector2() * (Projectile.width / 2.05f)) + (Projectile.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2));
            
            SpawnDefaultParticle(spawnPosition, Projectile.rotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi/2, Pi/2)) * velocityMultiplier, Main.rand.NextFloat(80f, 120f));
            counter++;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
        public bool isLeft(Vector2 a, Vector2 b, Vector2 c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0;
    }
}

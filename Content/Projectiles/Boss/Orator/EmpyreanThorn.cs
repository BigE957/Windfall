namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class EmpyreanThorn : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Projectiles/Misc/EmpyreanThorn";
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Ice Spike");
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.frame = Main.rand.Next(5);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 420;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() >= 1f && Projectile.timeLeft > 60)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 2;
            }
            else
                Projectile.velocity = Vector2.Zero;

            if(Projectile.timeLeft <= 60)
            {
                Projectile.Opacity -= 0.1f;
                Projectile.velocity -= Projectile.rotation.ToRotationVector2() * 4;
            }

            /*
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                Projectile.netUpdate = true;
            }

            int fadeInTime = 6;
            int fadeoutTime = 10;
            int lifetime = 26;
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.rotation = Projectile.velocity.ToRotation();
                
                for (int i = 0; i < 5; i++)
                {
                    Dust iceDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), 80, Projectile.velocity * Main.rand.NextFloat(0.15f, 0.525f));
                    iceDust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
                    iceDust.scale = 0.8f + Main.rand.NextFloat() * 0.5f;
                }
                for (int j = 0; j < 5; j++)
                {
                    Dust iceDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), 80, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity * Main.rand.NextFloat(0.15f, 0.375f));
                    iceDust.velocity += Main.rand.NextVector2Circular(0.5f, 0.5f);
                    iceDust.scale = 0.8f + Main.rand.NextFloat() * 0.5f;
                    iceDust.fadeIn = 1f;
                }
            }
            if (Time < fadeInTime)
            {
                Projectile.Opacity = Clamp(Projectile.Opacity + 0.2f, 0f, 1f);
                Projectile.scale = Projectile.Opacity * Projectile.ai[1] * 1.3f;
            }

            if (Time >= lifetime - fadeoutTime)
                Projectile.Opacity = Clamp(Projectile.Opacity - 0.2f, 0f, 1f);

            if (Time >= lifetime)
                Projectile.Kill();

            Time++;
            */
        }

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.Black, 0.36f) * Projectile.Opacity;

        public override bool ShouldUpdatePosition() => true;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = tex.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

            //Main.spriteBatch.Draw(tex, drawPosition, frame, Projectile.GetAlpha(Color.White), Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, 0, 0f);
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * Projectile.scale * 30f;
            Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * MathF.Max(65f, Projectile.scale * 110f);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.scale * 35f, ref _);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI, List<int> overWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
    }
}

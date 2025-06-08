using CalamityMod;
using CalamityMod.Particles;
using Luminance.Core.Graphics;

namespace Windfall.Content.Projectiles;
public class BlackSlash : ModProjectile, IPixelatedPrimitiveRenderer
{
    public override string Texture => "CalamityMod/ExtraTextures/Line";
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.damage = 100;
        Projectile.hostile = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 250;
        Projectile.tileCollide = false;
        Projectile.scale = 0f;
        Projectile.timeLeft = 120;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 60;

        Projectile.Calamity().DealsDefenseDamage = true;
    }

    private ref float initalAngle => ref Projectile.ai[0];
    private ref float angleShift => ref Projectile.ai[1];
    private ref float slashCount => ref Projectile.ai[2];

    private int Time = 0;
    private float widthScale = 2f;

    float length => 2400;
    float rotateDuration => 60f;

    int[] slashTimes;

    public override void OnSpawn(IEntitySource source)
    {
        initalAngle = Main.rand.NextFloat(0, Pi);
        angleShift = (PiOver2 + PiOver4) * (Main.rand.NextBool() ? -1 : 1);
        slashCount = Main.rand.Next(1, 4);

        slashTimes = new int[(int)slashCount];
        for (int i = 0; i < slashTimes.Length; i++)
            slashTimes[i] = -1;
    }

    public override void AI()
    {
        if (Time < rotateDuration)
        {
            Projectile.rotation = Lerp(initalAngle, initalAngle + angleShift, CircOutEasing(Time / rotateDuration));
            if (Time < 5)
                Projectile.scale = Lerp(0f, 1f, Time / 5f);
            else
            {
                Projectile.scale = 1f;
                widthScale = Lerp(2f, 0.75f, (Time - 5) / rotateDuration);
            }
        }
        else
        {
            if(Time == rotateDuration)
                SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);

            bool enabledSlash = false;
            for (int i = 0; i < slashTimes.Length; i++)
            {
                if (slashTimes[i] != -1)
                {
                    if (slashTimes[i] < 12f)
                        slashTimes[i]++;
                    continue;
                }
                if (!enabledSlash && Time % 3 == 0)
                {
                    enabledSlash = true;
                    slashTimes[i] = 0;
                    float myRotation = Projectile.rotation + (i * (Pi / slashCount));
                    for (int j = 0; j < 16; j++)
                    {
                        Vector2 spawnPos = Projectile.Center + myRotation.ToRotationVector2() * (length * Main.rand.NextFloat(0.1f, 1f) * Projectile.scale / 2f);
                        spawnPos += myRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(32f, 64f) * (Main.rand.NextBool() ? -1 : 1);
                        SparkParticle spark = new(spawnPos, myRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.01f, 0.01f)) * Main.rand.NextFloat(1f, 6f), false, 48, Main.rand.NextFloat(0.5f, 1.5f), Color.Red);
                        GeneralParticleHandler.SpawnParticle(spark);

                        spawnPos = Projectile.Center + myRotation.ToRotationVector2() * (-length * Main.rand.NextFloat(0.1f, 1f) * Projectile.scale / 2f);
                        spawnPos += myRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(32f, 64f) * (Main.rand.NextBool() ? -1 : 1);
                        SparkParticle spark2 = new(spawnPos, myRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.01f, 0.01f)) * -Main.rand.NextFloat(1f, 6f), false, 48, Main.rand.NextFloat(0.5f, 1.5f), Color.Red);
                        GeneralParticleHandler.SpawnParticle(spark2);
                    }
                }
            }

            Projectile.rotation = initalAngle + angleShift;
        }
        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        for(int i = 0; i < slashCount; i++)
        {
            if (slashTimes[i] == -1 || slashTimes[i] >= 12)
                continue;

            float myRotation = Projectile.rotation + (i * (Pi / slashCount));
            Vector2 start = Projectile.Center + myRotation.ToRotationVector2() * (length * Projectile.scale / 2f);
            Vector2 end = Projectile.Center + myRotation.ToRotationVector2() * (length * -Projectile.scale / 2f);
            float _ = 0;
            bool hit = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 64, ref _);
            
            if (hit)
                return true;
        }

        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if(Time < rotateDuration)
            Main.spriteBatch.UseBlendState(BlendState.Additive);

        for (int i = 0; i < slashCount; i++)
        {
            float myRotation = Projectile.rotation + (i * (Pi / slashCount));
            Vector2 start = Projectile.Center + myRotation.ToRotationVector2() * (length * Projectile.scale / 2f);
            Vector2 end = Projectile.Center + myRotation.ToRotationVector2() * (length * -Projectile.scale / 2f);

            if (Time < rotateDuration)
            {
                Color color = Color.White;
                if (Time > 5)
                    color = Color.Lerp(Color.White, Color.Red, (Time - 5) / 24f);

                DrawLineWith(Main.spriteBatch, start, end, color, 48 * widthScale, ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/BloomLine").Value);
            }
        }

        if (Time < rotateDuration)
            Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        for (int i = 0; i < slashCount; i++)
        {
            float myRotation = Projectile.rotation + (i * (Pi / slashCount));
            Vector2 start = Projectile.Center + myRotation.ToRotationVector2() * (length * Projectile.scale / 2f);
            Vector2 end = Projectile.Center + myRotation.ToRotationVector2() * (length * -Projectile.scale / 2f);

            if (Time < rotateDuration)
                DrawLineWith(Main.spriteBatch, start, end, Color.Black, 12f * widthScale, ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Line").Value);
        }
        
        
        return false;
    }

    public static void DrawLineWith(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width, Texture2D tex)
    {
        // Draw nothing if the start and end are equal, to prevent division by 0 problems.
        if (start == end)
            return;

        start -= Main.screenPosition;
        end -= Main.screenPosition;

        float rotation = (end - start).ToRotation() + PiOver2;
        Vector2 scale = new Vector2(width, Vector2.Distance(start, end)) / tex.Size();
        Vector2 origin = new(tex.Width * 0.5f, tex.Height);

        spriteBatch.Draw(tex, start, null, color, rotation, origin, scale, SpriteEffects.None, 0f);
    }

    private float slashWidth = 1f;

    internal float OuterWidthFunction(float completionRatio)
    {
        return 64 * (float)Math.Sin(completionRatio * Pi) * slashWidth;
    }

    internal float InnerWidthFunction(float completionRatio)
    {
        return 32 * Clamp((float)Math.Sin(completionRatio * Pi), 0f, 1f) * slashWidth;
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        if (Time >= rotateDuration)
        {
            float posCount = 24f;
            Vector2[] positions = new Vector2[(int)posCount];

            for (int i = 0; i < slashCount; i++)
            {
                if (slashTimes[i] == -1 || slashTimes[i] > 12)
                    continue;
                  
                float myRotation = Projectile.rotation + (i * (Pi / slashCount));
                float lengthScale = Lerp(1f, 0f, slashTimes[i] / 12f);
                slashWidth = Clamp(Lerp(1.5f, 0f, SineOutEasing(slashTimes[i] / 12f)), 0f, 1f);
                Vector2 start = Projectile.Center + myRotation.ToRotationVector2() * (length * Projectile.scale / 1.5f) * lengthScale;
                Vector2 end = Projectile.Center + myRotation.ToRotationVector2() * (length * -Projectile.scale / 1.5f) * lengthScale;

                for (int j = 0; j < posCount; j++)
                    positions[j] = Vector2.Lerp(start, end, j / posCount);
                Color color = Color.Red;
                if (slashTimes[i] > 2)
                    color = Color.Lerp(Color.Red, Color.White, (slashTimes[i] - 2) / 10f);
                PrimitiveRenderer.RenderTrail(positions, new(OuterWidthFunction, (_) => color, (_) => Vector2.Zero, true, true, null));

                start = Projectile.Center + myRotation.ToRotationVector2() * (length * Projectile.scale / 1.5f) * 0.75f * lengthScale;
                end = Projectile.Center + myRotation.ToRotationVector2() * (length * -Projectile.scale / 1.5f) * 0.75f * lengthScale;

                for (int j = 0; j < posCount; j++)
                    positions[j] = Vector2.Lerp(start, end, j / posCount);
                PrimitiveRenderer.RenderTrail(positions, new(InnerWidthFunction, (_) => Color.Black, (_) => Vector2.Zero, true, true, null));
            }
        }
    }
}

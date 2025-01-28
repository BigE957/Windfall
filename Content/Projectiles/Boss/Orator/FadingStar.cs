using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator;
public class FadingStar : ModProjectile, ILocalizedModType
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/FadingStar";

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.damage = TheOrator.GlobDamage;
        Projectile.hostile = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 390;
        Projectile.scale = 1f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    public override void AI()
    {
        if (Projectile.timeLeft % 5 == 0)
        {
            Vector2 position = new Vector2(Projectile.position.X, Projectile.Center.Y) + (Vector2.UnitY.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(-16f, 16f) - (Vector2.UnitX.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(-16f, 24f)));
            Particle spark = new SparkleParticle(position, Projectile.velocity.RotatedByRandom(PiOver4) * -0.5f, Color.White, Color.SkyBlue, Main.rand.NextFloat(0.25f, 0.5f), 24, Main.rand.NextFloat(0, 0.3f));
            GeneralParticleHandler.SpawnParticle(spark);
        }
    }

    internal Color ColorFunction(float completionRatio)
    {
        Color colorA = Color.LightSteelBlue;
        Color colorB = Color.Silver;

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.25f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.Black, endColor, fadeToEnd) * fadeOpacity;
    }

    internal float WidthFunction(float completionRatio)
    {
        float widthRatio = Utils.GetLerpValue(0f, 0.05f, completionRatio, true);
        float baseWidth = MathHelper.Lerp(0f, 60f, widthRatio) * MathHelper.Clamp(1f - (float)Math.Pow(completionRatio, 0.4D), 0.37f, 1f);
        return baseWidth;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2[] positions = new Vector2[30];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * (8 * i);

        if (positions.Length > 0)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(positions, new(WidthFunction, ColorFunction, (_) => Vector2.Zero, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 30);
        }

        Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8;
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 origin = texture.Size() * 0.5f;

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), Color.White * 0.5f, 0, origin, Projectile.scale * 0.33f, SpriteEffects.None, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        texture = TextureAssets.Projectile[Type].Value;
        origin = texture.Size() * 0.5f;

        Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), Color.White, Projectile.rotation + ((Main.GlobalTimeWrappedHourly * Projectile.velocity.Length() / 1.5f * (Projectile.whoAmI % 2 == 0 ? -1 : 1)) + (Projectile.whoAmI * 4)), origin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
}

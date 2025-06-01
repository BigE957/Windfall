

using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Terraria.Graphics.Shaders;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class DarkBolt : ModProjectile
{
    public override string Texture => "Windfall/Assets/Projectiles/Boss/NailShot";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.damage = 100;
        Projectile.hostile = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 250;
        Projectile.tileCollide = false;

        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 60;

        Projectile.Calamity().DealsDefenseDamage = true;
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
    private enum myColor
    {
        Green,
        Orange
    }
    private myColor MyColor
    {
        get => Projectile.ai[2] == 0 ? myColor.Green : myColor.Orange;
        set => Projectile.ai[2] = (int)value;
    }
    Vector2 DirectionalVelocity = Vector2.Zero;
    public override void OnSpawn(IEntitySource source)
    {
        DirectionalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
    }
    public override void AI()
    {
        Projectile.rotation = DirectionalVelocity.ToRotation();
        Projectile.velocity = DirectionalVelocity.SafeNormalize(Vector2.UnitX) * (Velocity / 2);
        Velocity += 1f;
        aiCounter++;
        if (Velocity > 5 && Main.rand.NextBool(4))
        {
            Vector2 position = new Vector2(Projectile.position.X, Projectile.Center.Y) + (Vector2.UnitY.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(-16f, 16f));

            Particle spark = new SparkParticle(position, Projectile.velocity.RotatedByRandom(PiOver4) * -0.5f, false, 12, Main.rand.NextFloat(0.25f, 1f), ColorFunction(0));
            GeneralParticleHandler.SpawnParticle(spark);
        }
        Lighting.AddLight(Projectile.Center, ColorFunction(0).ToVector3());
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Location = new(hitbox.Location.X + 39, hitbox.Center.Y);
        Vector2 rotation = Projectile.rotation.ToRotationVector2();
        rotation *= 39;           
        hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
    }

    internal Color ColorFunction(float completionRatio)
    {
        Color colorA = MyColor == myColor.Green ? Color.LimeGreen : Color.Orange;
        Color colorB = MyColor == myColor.Green ? Color.GreenYellow : Color.Goldenrod;

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.1f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
    }

    internal float WidthFunction(float completionRatio)
    {
        float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
        float maxWidth = Projectile.Opacity * Projectile.width * 2f;

        return Lerp(0f, maxWidth, expansionCompletion);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Velocity > 2)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/" + (MyColor == myColor.Green ? "ScarletDevilStreak" : "SylvestaffStreak")));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 30);
        }

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 origin = texture.Size() * 0.5f;

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Main.EntitySpriteDraw(texture, drawPos - Vector2.UnitX.RotatedBy(Projectile.rotation) * (MyColor == myColor.Green ? 48 : 28) + Vector2.UnitY * (MyColor == myColor.Green ? 2 : 0), texture.Frame(), ColorFunction(0) * 0.6f, Projectile.rotation, origin, new Vector2(MyColor == myColor.Green ? 3 : 2f, 1) * Projectile.scale * 0.33f, SpriteEffects.None, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        texture = (MyColor == myColor.Green ? TextureAssets.Projectile[Type] : ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/MagicShot")).Value;
        origin = texture.Size();
        origin.Y *= 0.5f;
        Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        
        return false;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(DirectionalVelocity.X);
        writer.Write(DirectionalVelocity.Y);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        DirectionalVelocity = reader.ReadVector2();
    }
}

using CalamityMod.Graphics.Primitives;
using CalamityMod.World;
using Terraria.Graphics.Shaders;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class DarkGlob : ModProjectile, ILocalizedModType
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

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
        Projectile.scale = 0f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }

    private float MaxSize
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public enum TrailType
    {
        None = -1,
        Implied,
        Shader,
        Particle
    }

    private TrailType Trail
    {
        get => (TrailType)Projectile.ai[2];
        set => Projectile.ai[2] = (int)value;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (Projectile.ai[2] != -1)
            switch(Projectile.ai[0])
            {                
                case 1:
                    Trail = TrailType.Shader;
                    break;
                case 2:
                case 0:
                    Trail = TrailType.Particle;
                    break;
                default:
                    Trail = TrailType.None;
                    break;
            }
        else
            Trail = TrailType.None;

        Projectile.netUpdate = true;
    }
    public override void AI()
    {
        if (Projectile.ai[0] == 0 || Projectile.ai[0] == 2)
        {
            if (Projectile.timeLeft > 30 && Projectile.scale < MaxSize)
                Projectile.scale += MaxSize/30;
            else
                Projectile.scale = MaxSize;            
            
            if (Projectile.scale < MaxSize || Projectile.friendly)
                Projectile.hostile = false;
            else
                Projectile.hostile = true;

            if (Projectile.velocity.Length() > 0.5f)
            {
                if (Projectile.ai[0] == 0)
                {
                    Projectile.velocity *= 0.985f;
                    if (Projectile.velocity.LengthSquared() < 4)
                        Projectile.velocity = Vector2.Zero;
                }
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
                if (Projectile.timeLeft < 30)
                {
                    Projectile.scale -= MaxSize / 30;
                    Projectile.netUpdate = true;
                    if(Projectile.timeLeft == 5)
                        for (int i = 0; i <= 10; i++)
                            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f) * Main.rand.NextFloat(1f, 2f) * MaxSize, 8 * Main.rand.NextFloat(3f, 5f) * MaxSize);
                }
            }
        }
        else
        {
            if (MaxSize > 1.5f)
                Projectile.scale = MaxSize;
            if (Projectile.scale < MaxSize)
            {
                Projectile.scale += 1 / 60f;
                Projectile.timeLeft = 500;
                Projectile.hostile = false;
            }
            else
            {
                Projectile.scale = MaxSize;
                if (!Projectile.friendly)
                    Projectile.hostile = true;
            }
            Projectile.velocity.Y += CalamityWorld.death ? 0.3f : CalamityWorld.revenge ? 0.25f : Main.expertMode ? 0.2f : 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if(MaxSize <= 1.5f && Main.rand.NextBool(3))
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - Projectile.velocity.RotatedBy(PiOver2).SafeNormalize(Vector2.UnitX) * (Main.rand.NextFloat(-32, 32) * Projectile.scale), Vector2.Zero, Projectile.scale * 48);
        }
        Projectile.Hitbox = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, (int)(70 * MaxSize), (int)(70 * MaxSize));

        if(Trail == TrailType.Particle)
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Vector2.Zero, Projectile.scale * 48);

        Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
    }
          
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CircularHitboxCollision(Projectile.Center, (projHitbox.Width / 2) * (Projectile.scale / MaxSize), targetHitbox))
            return true;
        return false;
    }

    private Color ColorFunction(float completionRatio)
    {
        Color colorA = Color.Lerp(Color.LimeGreen, Color.Orange, EmpyreanMetaball.BorderLerpValue);
        Color colorB = Color.Lerp(Color.GreenYellow, Color.Goldenrod, EmpyreanMetaball.BorderLerpValue);

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.1f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
    }

    private float WidthFunction(float completionRatio)
    {
        float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
        float maxWidth = Projectile.Opacity * Projectile.scale * 180f;

        return Lerp(0f, maxWidth, expansionCompletion);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Trail == TrailType.Shader)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 20);
        }

        Main.spriteBatch.UseBlendState(BlendState.Additive);
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition - (Projectile.ai[0] == 0 ? Vector2.Zero : Projectile.velocity.SafeNormalize(Vector2.UnitX) * (16 * Projectile.scale));
        Main.EntitySpriteDraw(tex, drawPos, tex.Frame(), ColorFunction(0) * 0.75f, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 0.8f, SpriteEffects.None, 0);
        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);
        
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 origin = tex.Size() * 0.5f;
        Vector2 drawPos = Projectile.Center - Main.screenPosition - (Projectile.ai[0] == 0 ? Vector2.Zero : Projectile.velocity.SafeNormalize(Vector2.UnitX) * (16 * Projectile.scale));
        Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
        
        //DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
    }

}

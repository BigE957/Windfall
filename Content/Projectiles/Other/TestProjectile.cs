using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Audio;
using Terraria.Graphics.Shaders;

namespace Windfall.Content.Projectiles.Other;
public class TestProjectile : ModProjectile
{
    public override string Texture => "CalamityMod/Projectiles/Melee/GalaxiaBolt";

    public new static string LocalizationCategory => "Projectiles.Other";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 1200;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();

        Player owner = Main.LocalPlayer;
        Projectile.velocity = Projectile.Center.SafeDirectionTo(owner.Center) * 8f;

        Lighting.AddLight(Projectile.Center, new Color(117, 255, 159).ToVector3());
    }

    internal Color ColorFunction(float completionRatio)
    {
        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.1f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(Color.LimeGreen, Color.DarkCyan, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
    }

    internal float WidthFunction(float completionRatio)
    {
        float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
        float maxWidth = Projectile.Opacity * Projectile.width * 1.3f;

        return MathHelper.Lerp(0f, maxWidth, expansionCompletion);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        //ScarletDevilStreak, FabstaffStreak
        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 30);

        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/MagicShot").Value;
        Vector2 origin = new(texture.Size().X / 2f, 0);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, 0.5f), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}

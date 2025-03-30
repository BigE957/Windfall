using CalamityMod.Particles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Projectiles.NPCAnimations;

public class DoGRift : ModProjectile
{
    public override string Texture => "CalamityMod/ExtraTextures/SoulVortex";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 75;
        Projectile.damage = 0;
        Projectile.scale = 4f;
    }
    private int aiCounter = 0;
    private float eyeOpacity = 0f;
    private int despawnCounter = 0;
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0f;
    }
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, Color.Violet.ToVector3() * (5f * Projectile.scale));
        Projectile.rotation += 0.05f;

        if (Projectile.ai[0] == 1)
        {
            if (eyeOpacity > 0f)
                eyeOpacity -= 0.025f;
            if(despawnCounter > 60)
                Projectile.scale = SineBumpEasing(0.5f + (despawnCounter - 60) / 40f) * 4f;
            despawnCounter++;
            if (Projectile.scale <= 0)
                Projectile.active = false;
            return;
        }
        if (aiCounter < 20)
            Projectile.scale = SineBumpEasing(aiCounter / 40f) * 4f;

        if (aiCounter > 20 && aiCounter % 10 == 0)
        {
            Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(80f * Projectile.scale, 80f * Projectile.scale);
            Color color = Color.Lerp(Color.Fuchsia, Color.Aqua, Main.rand.NextFloat(0f, 1f));
            Particle particle = new AltSparkParticle(spawnPosition, (spawnPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * (Main.rand.NextFloat(-4, -2f) * Projectile.scale), false, 200, Main.rand.NextFloat(0.25f, 0.5f) * Projectile.scale, color);
            GeneralParticleHandler.SpawnParticle(particle);
        }
        if (aiCounter > 60 && eyeOpacity < 1f)
            eyeOpacity += 0.025f;
        
        aiCounter++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = new(Main.graphics.GraphicsDevice);
        spriteBatch.Begin();
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
        Vector2 origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.Black), 0f, origin, Projectile.scale / 2f, SpriteEffects.None, 0f);            
        
        spriteBatch.UseBlendState(BlendState.Additive);
        texture = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoonBloom2").Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.Violet), -Projectile.rotation / 2.25f, origin, Projectile.scale * (0.24f + 0.02f * ((float)Math.Sin(aiCounter / -20f) / 2f + 0.5f)), SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.AlphaBlend);
        texture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.Black), 0f, origin, Projectile.scale / 3f, SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.Additive);
        texture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SoulVortex").Value;
        origin = texture.Size() * 0.5f;
        Color color = Color.Violet;
        spriteBatch.Draw(texture, drawPosition, null, color, Projectile.rotation, origin, Projectile.scale / 4f, SpriteEffects.None, 0f);
        color = Color.Lerp(Color.Violet, Color.Aqua, 0.33f) * 0.5f;
        color.A = 200;
        spriteBatch.Draw(texture, drawPosition, null, color, -Projectile.rotation / 1.25f, origin, Projectile.scale / 4.25f, SpriteEffects.None, 0f);
        color = Color.Lerp(Color.Violet, Color.Aqua, 0.66f) * 0.5f;
        color.A = 150;
        spriteBatch.Draw(texture, drawPosition, null, color, Projectile.rotation / 1.5f, origin, Projectile.scale / 4.5f, SpriteEffects.None, 0f);
        color = Color.Aqua;
        color.A = 100;
        spriteBatch.Draw(texture, drawPosition, null, color, -Projectile.rotation / 1.75f, origin, Projectile.scale / 4.75f, SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.AlphaBlend);
        texture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/NPCAnimations/DoGEyes").Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, Color.White * eyeOpacity, 0f, origin, Projectile.scale / 6f, SpriteEffects.None, 0f);

        spriteBatch.End();
        return false;
    }
}

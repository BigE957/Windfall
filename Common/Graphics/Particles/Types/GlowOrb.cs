using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Graphics.Particles;

public class GlowOrb : Particle
{
    public Color InitialColor;
    public bool AffectedByGravity;
    public bool UseAltVisual = true;
    public float fadeOut = 1;
    public bool imporant;
    public bool glowCenter;

    public override bool Additive => UseAltVisual;
    public override bool Important => imporant;

    private static Asset<Texture2D> Texture;// => "CalamityMod/Particles/GlowOrbParticle";

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/GlowOrbParticle");
    }

    public GlowOrb(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, bool AddativeBlend = true, bool needed = false, bool GlowCenter = true)
    {
        Position = relativePosition;
        Velocity = velocity;
        AffectedByGravity = affectedByGravity;
        Scale = scale * Vector2.One;
        Lifetime = lifetime;
        Color = InitialColor = color;
        UseAltVisual = AddativeBlend;
        imporant = needed;
        glowCenter = GlowCenter;
    }

    public override void Update()
    {
        fadeOut -= 0.1f;
        Scale *= 0.93f;
        Color = Color.Lerp(InitialColor, InitialColor * 0.2f, (float)Math.Pow(LifeRatio, 3D));
        Velocity *= 0.95f;
        if (Velocity.Length() < 12f && AffectedByGravity)
        {
            Velocity.X *= 0.94f;
            Velocity.Y += 0.25f;
        }
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 scale = new Vector2(1f, 1f) * Scale;
        Texture2D texture = Texture.Value;

        spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        if (glowCenter)
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.White * fadeOut, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.5f, 0.5f), 0, 0f);
    }
}

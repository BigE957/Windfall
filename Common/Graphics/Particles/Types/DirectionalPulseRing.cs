using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CalamityMod.CalamityUtils;

namespace Windfall.Common.Graphics.Particles;

public class DirectionalPulseRing : Particle
{
    private static Asset<Texture2D> Texture;
    public override bool Additive => true;

    private readonly float OriginalScale;
    private readonly float FinalScale;
    private float opacity;
    private Vector2 Squish;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/HollowCircleHardEdge");
    }

    public DirectionalPulseRing(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale * Vector2.One;
        Lifetime = lifeTime;
        Squish = squish;
        Rotation = rotation;
    }

    public override void Update()
    {
        float pulseProgress = PiecewiseAnimation(LifeRatio, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) });
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, pulseProgress) * Vector2.One;

        opacity = (float)Math.Sin(MathHelper.PiOver2 + LifeRatio * MathHelper.PiOver2);

        Color c = Color * opacity;
        Lighting.AddLight(Position, c.R / 255f, c.G / 255f, c.B / 255f);
        Velocity *= 0.95f;

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = Texture.Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity, Rotation, tex.Size() / 2f, Scale * Squish, SpriteEffects.None, 0);
    }
}

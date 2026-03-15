using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Windfall.Common.Graphics.Particles;

public class HeavySmoke : Particle
{
    public override bool Important => StrongVisual;
    public override bool Additive => Glowing;

    private static Asset<Texture2D> Texture;
    private const int FrameAmount = 6;

    private float Opacity;
    private readonly float Spin;
    private readonly bool StrongVisual;
    private readonly bool Glowing;
    private readonly float HueShift;
    private readonly int Variant;
    private readonly bool AffectedByLight;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/HeavySmoke");
    }

    public HeavySmoke(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f, bool glowing = false, float hueshift = 0f, bool required = false, bool affectedByLight = false)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        Scale = scale * Vector2.One;
        Variant = Main.rand.Next(7);
        Lifetime = lifetime;
        Opacity = opacity;
        Spin = rotationSpeed;
        StrongVisual = required;
        Glowing = glowing;
        HueShift = hueshift;
        AffectedByLight = affectedByLight;
    }

    public override void Update()
    {
        if (Time / (float)Lifetime < 0.2f)
            Scale += 0.01f * Vector2.One;
        else
            Scale *= 0.975f;

        Color = Main.hslToRgb((Main.rgbToHsl(Color).X + HueShift) % 1, Main.rgbToHsl(Color).Y, Main.rgbToHsl(Color).Z);
        Opacity *= 0.98f;
        Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
        Velocity *= 0.85f;

        float opacity = Terraria.Utils.GetLerpValue(1f, 0.85f, LifeRatio, true);
        Color *= opacity;

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = Texture.Value;
        int animationFrame = (int)Math.Floor(Time / ((float)(Lifetime / (float)FrameAmount)));
        Rectangle frame = new(80 * Variant, 80 * animationFrame, 80, 80);

        Color col = Color * Opacity;

        if (AffectedByLight)
            col = col.MultiplyRGBA(Lighting.GetColor((Position / 16).ToPoint()));

        spriteBatch.Draw(tex, Position - Main.screenPosition, frame, col, Rotation, frame.Size() / 2f, Scale, SpriteEffects.None, 0);
    }
}

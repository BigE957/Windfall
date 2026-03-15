using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Graphics.Particles;

public class CircularSmear : Particle
{
    private static Asset<Texture2D> Texture;
    public override bool Additive => true;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/CircularSmear");
    }

    public CircularSmear(Vector2 position, Color color, float rotation, float scale)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale * Vector2.One;
        Rotation = rotation;
        Lifetime = 2;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture.Value, Position - Main.screenPosition, null, Color, Rotation, Texture.Size() * 0.5f, Scale, SpriteEffects.None, 0);
    }
}

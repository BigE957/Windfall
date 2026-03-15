using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Graphics.Particles;

public class AltSpark : Particle
{
    public bool AffectedByGravity;
    public override bool Additive => false;

    private Asset<Texture2D> Texture;

    public override void Load()
    {
        Texture = Main.Assets.Request<Texture2D>("Images/Extra_189");
    }

    public AltSpark(Vector2 relativePosition, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color)
    {
        Position = relativePosition;
        Velocity = velocity;
        AffectedByGravity = affectedByGravity;
        Scale = scale * Vector2.One;
        Lifetime = lifetime;
        Color = color;
    }

    public override void Update()
    {
        Scale *= 0.95f;
        
        Velocity *= 0.95f;
        if (Velocity.Length() < 12f && AffectedByGravity)
        {
            Velocity.X *= 0.94f;
            Velocity.Y += 0.25f;
        }
        Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Vector2 scale = new Vector2(0.5f, 1.6f) * Scale;
        Texture2D texture = Texture.Value;

        Color c = Color.Lerp(Color, Color.Transparent, (float)Math.Pow(LifeRatio, 3D));

        spriteBatch.Draw(texture, Position - Main.screenPosition, null, c, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        spriteBatch.Draw(texture, Position - Main.screenPosition, null, c, Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Graphics.Particles;

public class SemiCircularSmear : Particle
{
    private static Asset<Texture2D> Texture;
    public override bool Additive => true;

    public bool PlayerCentered;
    public Vector2 Squish;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("WindfallAssetsGraphicsExtraSemiCircularSmear");
    }

    public SemiCircularSmear(Vector2 position, Color color, float rotation, float scale, Vector2 squish, bool playerCentered = false)
    {
        Position = position;
        Velocity = Vector2.Zero;
        Color = color;
        Scale = scale * Vector2.One;
        Squish = squish;
        Rotation = rotation;
        Lifetime = 2;
        PlayerCentered = playerCentered;
    }
    public override void Update()
    {
        if (PlayerCentered)
            Position = Main.LocalPlayer.MountedCenter;

        base.Update();
    }

    //Use custom draw for the squish
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = Texture.Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color, Rotation, tex.Size() / 2f, Squish * Scale, SpriteEffects.None, 0);
    }
}

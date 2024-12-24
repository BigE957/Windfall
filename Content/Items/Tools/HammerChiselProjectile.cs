using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Windfall.Content.Items.Tools;
public class HammerChiselProjectile : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Tools/HammerAndChiselProj";
    public override void SetDefaults()
    {
        Projectile.width = 28;
        Projectile.height = 17;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 24;
    }

    private int aiCounter
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    Vector2 spawnPos = Vector2.Zero;
    Vector2 goalPos = Vector2.Zero;
    Vector2 rotationVector = Vector2.Zero;
    float DistanceToTile = 0f;

    public override void OnSpawn(IEntitySource source)
    {
        spawnPos = Projectile.Center;
        
        if(Main.player[Projectile.owner] == Main.LocalPlayer)
        {
            if(Main.SmartCursorIsUsed && Main.SmartCursorShowing)
            {
                goalPos = new(Main.SmartCursorX * 16f, Main.SmartCursorY * 16f);
                goalPos += Vector2.UnitX * 8f;
                DistanceToTile = (spawnPos - goalPos).Length();
                rotationVector = (spawnPos - goalPos).SafeNormalize(Vector2.Zero);
            }
            else
            {
                goalPos = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates(autoAddY: 0);
                DistanceToTile = (spawnPos - goalPos).Length();
                rotationVector = (spawnPos - goalPos).SafeNormalize(Vector2.Zero);
            }

        }
    }

    public override void AI()
    {
        if (aiCounter < 5)
            Projectile.Center = Vector2.Lerp(spawnPos, goalPos, SineOutEasing(aiCounter / 5f, 1));
        else if (aiCounter > 21)
            Projectile.Center = Vector2.Lerp(goalPos, spawnPos, SineOutEasing((aiCounter - 20) / 4f, 1));
        else
            Projectile.Center = goalPos;

        if(aiCounter == 18)
        {
            Point tileLocation = goalPos.ToTileCoordinates();
            VanillaHammerSlopingLogic(tileLocation);
        }

        aiCounter++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle chiselFrame = tex.Frame(2, 1, 0, 0);
        float rotationOffset = Clamp(1 - aiCounter / 5f, 0, 1) * TwoPi;
        if (aiCounter > 20)
            rotationOffset = Clamp(1 - (aiCounter - 20) / 5f, 0, 1) * -TwoPi;
        rotationOffset *= rotationVector.X > 0 ? 1 : -1;
        Vector2 chiselPosition = Projectile.Center - Main.screenPosition + (aiCounter > 17 ? Vector2.Zero : Vector2.UnitY * -Projectile.height);

        Main.EntitySpriteDraw(tex, chiselPosition, chiselFrame, Color.White * Projectile.Opacity, Projectile.rotation + Pi + rotationOffset, chiselFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        Rectangle hammerFrame = tex.Frame(2, 1, 1, 0);
        Vector2 offset = new(24 * (rotationVector.X > 0 ? 1 : -1), Projectile.height * -2);
        if (aiCounter > 5)
        {
            int angleNegative = rotationVector.X > 0 ? -1 : 1;
            if (aiCounter <= 12)
            {
                float lerp = Clamp((aiCounter - 5) / 7f, 0, 1);
                offset += Vector2.Lerp(Vector2.Zero, new Vector2(rotationVector.X > 0 ? 24 : -24, -16), lerp);
                rotationOffset += Lerp(0f, Pi, lerp) * angleNegative;
            }
            else
            {
                float lerp = Clamp((aiCounter - 12) / 3f, 0, 1);
                offset += Vector2.Lerp(new Vector2(rotationVector.X > 0 ? 24 : -24, -16), Vector2.UnitX * (rotationVector.X > 0 ? -16 : 16), lerp);
                rotationOffset += Lerp(Pi, -PiOver4, lerp) * angleNegative;
            }
        }
        Vector2 hammerPosition = (Projectile.Center - Main.screenPosition + (offset * Clamp(aiCounter / 5f, 0, 1))) + rotationVector.RotatedBy(PiOver2 * (rotationVector.X > 0 ? -1 : 1)) * (aiCounter <= 5 ? (1f - ((float)Math.Abs(aiCounter / 5f - 0.5f) * 2)) * 72 : 0);

        Main.EntitySpriteDraw(tex, hammerPosition, hammerFrame, Color.White * Projectile.Opacity, Projectile.rotation - rotationOffset + (PiOver4 / 2f * rotationVector.X > 0 ? -1 : 1), chiselFrame.Size() * 0.5f, Projectile.scale, rotationVector.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

        return false;
    }
}

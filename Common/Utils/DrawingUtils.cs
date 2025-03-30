using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static void DrawLineBetween(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width)
    {
        if (!(start == end))
        {
            start -= Main.screenPosition;
            end -= Main.screenPosition;
            Texture2D value = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Line").Value;
            float rotation = (end - start).ToRotation();
            spriteBatch.Draw(scale: new Vector2(Vector2.Distance(start, end) / (float)value.Width, width), texture: value, position: start, sourceRectangle: null, color: color, rotation: rotation, origin: value.Size() * Vector2.UnitY * 0.5f, effects: SpriteEffects.None, layerDepth: 0f);
        }
    }

    public static void DrawCenteredAfterimages(Projectile proj, int mode, Color lightColor, int typeOneIncrement = 1, Texture2D texture = null, bool drawCentered = true)
    {
        texture ??= TextureAssets.Projectile[proj.type].Value;

        int num = texture.Height / Main.projFrames[proj.type];
        int y = num * proj.frame;
        float scale = proj.scale;
        float rotation = proj.rotation;
        Rectangle rectangle = new(0, y, texture.Width, num);
        Vector2 origin = rectangle.Size() / 2f;
        SpriteEffects effects = SpriteEffects.None;
        if (proj.spriteDirection == -1)
        {
            effects = SpriteEffects.FlipHorizontally;
        }

        bool flag = false;
        if (CalamityConfig.Instance.Afterimages)
        {
            Vector2 vector = (drawCentered ? (proj.Size / 2f) : Vector2.Zero);
            Color alpha = proj.GetAlpha(lightColor);
            switch (mode)
            {
                case 0:
                    {
                        for (int j = 0; j < proj.oldPos.Length; j++)
                        {
                            Vector2 position2 = proj.oldPos[j] + vector - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            Color color2 = alpha * ((float)(proj.oldPos.Length - j) / (float)proj.oldPos.Length);
                            Main.spriteBatch.Draw(texture, position2, rectangle, color2, rotation, origin, scale, effects, 0f);
                        }

                        break;
                    }
                case 1:
                    {
                        int num2 = Math.Max(1, typeOneIncrement);
                        Color color3 = alpha;
                        int num3 = ProjectileID.Sets.TrailCacheLength[proj.type];
                        float num4 = (float)num3 * 1.5f;
                        for (int k = 0; k < num3; k += num2)
                        {
                            Vector2 position3 = proj.oldPos[k] + vector - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            if (k > 0)
                            {
                                float num5 = num3 - k;
                                color3 *= num5 / num4;
                            }

                            Main.spriteBatch.Draw(texture, position3, rectangle, color3, rotation, origin, scale, effects, 0f);
                        }

                        break;
                    }
                case 2:
                    {
                        for (int i = 0; i < proj.oldPos.Length; i++)
                        {
                            float rotation2 = proj.oldRot[i];
                            SpriteEffects effects2 = ((proj.oldSpriteDirection[i] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                            Vector2 position = proj.oldPos[i] + vector - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                            Color color = alpha * ((float)(proj.oldPos.Length - i) / (float)proj.oldPos.Length);
                            Main.spriteBatch.Draw(texture, position, rectangle, color, rotation2, origin, scale, effects2, 0f);
                        }

                        break;
                    }
                default:
                    flag = true;
                    break;
            }
        }

        if (!CalamityConfig.Instance.Afterimages || ProjectileID.Sets.TrailCacheLength[proj.type] <= 0 || flag)
        {
            Vector2 vector2 = (drawCentered ? proj.Center : proj.position);
            Main.spriteBatch.Draw(texture, vector2 - Main.screenPosition + new Vector2(0f, proj.gfxOffY), rectangle, proj.GetAlpha(lightColor), rotation, origin, scale, effects, 0f);
        }
    }
}

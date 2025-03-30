using DialogueHelper.UI.Dialogue.Emoticons;
using System.ComponentModel.Design.Serialization;
using Terraria.UI;

namespace Windfall.Content.UI.Dialogue.Emoticons;
public class Angry : Emoticon
{
    public override void Update(GameTime gameTime)
    {
        if (Counter < 0)
        {
            Opacity = 0f;
            return;
        }

        base.Update(gameTime);

        if (Counter <= 60 && !Fading)
            ImageScale = Clamp(Lerp(0, 1f, SineOutEasing(Counter / 60f)), 0, 1f);
        else
            ImageScale = 1f;

        if (Counter <= 90)
            Opacity = Clamp(Lerp(0, 1f, SineOutEasing(Counter / 90f)), 0, 1f);
        else
            Opacity = 1f;
    }

    public override int TimeToAppear => 90;

    public override Vector2 OffsetPosition()
    {
        if (Counter >= 60 || Fading)
            return new(SpeakerHeadArea.Width * 0.75f, -SpeakerHeadArea.Height * 2);
        float lerp = SineOutEasing(Counter / 60f);
        return new(Lerp(0, SpeakerHeadArea.Width * 0.75f, lerp), -Lerp(0, SpeakerHeadArea.Height * 2, lerp));
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();

        int count = 3;

        for (int i = 0; i < count; i++)
        {
            bool isBottom = i == count - 1;
            Texture2D texture = isBottom ? ModContent.Request<Texture2D>("WindFall/Content/UI/Dialogue/Emoticons/Angry_Emote_Bottom", AssetRequestMode.ImmediateLoad).Value : ModContent.Request<Texture2D>("WindFall/Content/UI/Dialogue/Emoticons/Angry_Emote_Top", AssetRequestMode.ImmediateLoad).Value;

            Vector2 origin = texture.Size() * 0.5f;

            Vector2 position = dimensions.Position() + origin * ImageScale;

            float input = Counter / 12f;

            SpriteEffects flip = i == 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float angleOffset = 0;
            switch(i)
            {
                case 0:
                    angleOffset = Pi;
                    break;
                case 1:
                    angleOffset = -PiOver2;
                    break;
                case 2:
                    angleOffset = 3 * PiOver2;
                    break;
            }

            float angleAnimation = (float)Math.Sin(input / 2f) * Pi / 8f;
            Vector2 offset = Vector2.One.RotatedBy((2f * Pi / count * i) + angleAnimation) * -(24 + (float)TiltedSine(input, true) * 7);
            spriteBatch.Draw(texture, position + offset + OffsetPosition(), null, Color * Opacity, Rotation + ((2f * Pi / count * i) + angleAnimation) + angleOffset, origin, (((float)TiltedSine(input, true) / 8) + ImageScale) * 0.65f, flip, 0f);
        }
    }
    private static float TiltedSine(float x, bool cos = false)
    {
        float factor = -0.5f;
        if (cos)
            x += Pi/3f;
        return (float)(1 / factor * Math.Atan((factor * Math.Sin(x)) / (1 - factor * Math.Cos(x)) ));
    }
}

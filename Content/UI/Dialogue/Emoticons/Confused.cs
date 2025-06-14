using DialogueHelper.UI.Dialogue.Emoticons;
using Terraria.UI;

namespace Windfall.Content.UI.Dialogue.Emoticons;

public class Confused : Emoticon
{
    public override string[] TexturePaths => ["WindFall/Content/UI/Dialogue/Emoticons/Confused_Emote"];

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
            return new(SpeakerHeadArea.Width, -SpeakerHeadArea.Height * 3);
        float lerp = SineOutEasing(Counter / 60f);
        return new(Lerp(0, SpeakerHeadArea.Width, lerp), -Lerp(0, SpeakerHeadArea.Height * 3, lerp));
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Texture2D texture = Assets[0].Value;
        
        Vector2 origin = texture.Size() * 0.5f;

        Vector2 position = dimensions.Position() + origin * ImageScale;
        for (int i = 0; i < 3; i++)
        {
            float scale = ImageScale * (i == 1 ? 1f : 0.75f);
            Vector2 offset = new(60 * (i - 1), (float)Math.Cos((Counter + 8 * i) / 20f) * 12);
            Vector2 squishFactor = new((float)Math.Sin(Counter / 20f * (i-1)) / 4f + 1, (float)Math.Cos(Counter / 20f * (i - 1)) / 4f + 1);
            squishFactor = (squishFactor / 1.5f) + (Vector2.One * 0.25f);
            spriteBatch.Draw(texture, position + offset + OffsetPosition(), null, Color * Opacity, Rotation + (Pi / 12 * (i-1) + (float)Math.Sin(Counter / 20f) * Pi / 12), origin, scale * squishFactor, spriteEffects, 0f);
        }
    }
}

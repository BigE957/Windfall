using DialogueHelper.UI.Dialogue.Emoticons;
using Terraria.UI;

namespace Windfall.Content.UI.Dialogue.Emoticons;
public class Happy : BaseEmoticon
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
            ImageScale = Clamp(Lerp(0, 1f, SineOutEasing(Counter / 60f, 1)), 0, 1f);
        else
            ImageScale = 1f;

        if (Counter <= 90)
            Opacity = Clamp(Lerp(0, 1f, SineOutEasing(Counter / 90f, 1)), 0, 1f);
        else
            Opacity = 1f;
    }

    public override int TimeToAppear => 90;

    public override Vector2 OffsetPosition() => new(SpeakerHeadArea.Width * 0.5f, -SpeakerHeadArea.Height * 1.5f);

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Texture2D texture = ModContent.Request<Texture2D>("WindFall/Content/UI/Dialogue/Emoticons/Happy_Emote", AssetRequestMode.ImmediateLoad).Value;

        Vector2 origin = texture.Size() * 0.5f;
        origin.X /= 10f;

        Vector2 position = dimensions.Position() + origin * ImageScale;

        int lineCount = 3;
        for (int i = 0; i < lineCount; i++)
        {
            int halfCOunt = lineCount / 2;
            int ID = i - halfCOunt + Math.Sign(i-halfCOunt);
            if (ID == 0)
                ID++;

            //Main.NewText()

            float angle = ((-3 * Pi / 2f) + Pi - (Pi/12f)) + ((Pi - Pi / 12f) / 7 * (i+1));
            angle += Pi / 18f * (i - halfCOunt);
            Vector2 offset = new Vector2(1,-1) * 12 / Math.Abs(i- halfCOunt == 0 ? 0.75f : i- halfCOunt);
            offset += angle.ToRotationVector2() * 32;

            Vector2 squishFactor = new(FlattenedCos(Counter / 8f * Math.Sign(ID) + ID) * ExpOutEasing(1/(float)Math.Abs(ID*4), 1), 1 - (0.5f * Math.Abs(ID) * FlattenedCos(Counter / 8f * Math.Sign(ID)) * ExpOutEasing(1 / (float)Math.Abs(ID * 4), 1)));
            squishFactor *= 0.5f;
            squishFactor += Vector2.One / 2f;
            squishFactor.Y -= 0.25f;
            squishFactor.X -= 0.2f * Math.Abs(ID) - 0.1f;

            spriteBatch.Draw(texture, position + offset + OffsetPosition(), null, Color * Opacity, Rotation + angle, origin, ImageScale * squishFactor, spriteEffects, 0f);
        }
    }

    private static float FlattenedCos(float x) => (float)Math.Cos(x) / 4f + 1f;
}

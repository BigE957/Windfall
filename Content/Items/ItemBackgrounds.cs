using System.Collections.ObjectModel;

namespace Windfall.Content.Items;
public static class ItemBackgrounds
{
    public static bool OratorDropBackground(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        int lineCount = 0;
        float longestLength = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            string[] strings = lines[i].Text.Split('\n');
            lineCount += strings.Length;
            for (int j = 0; j < strings.Length; j++)
            {
                float measuredLength = FontAssets.MouseText.Value.MeasureString(strings[j]).X;
                if (measuredLength > longestLength)
                {
                    longestLength = measuredLength;
                }
            }
        }

        int xOffset = 15;
        int yOffset = 9;


        Rectangle box = new(x - xOffset, y - yOffset, (int)(longestLength) - yOffset * 7, lineCount * 28 + yOffset + (int)Math.Ceiling(yOffset / 2f));

        Utils.DrawInvBG(Main.spriteBatch, box, new Color(0, 150, 50) * 0.5f);
        for (int i = box.Y; i < box.Y + box.Height; i++)
        {
            float value = Utils.GetLerpValue(box.Y, box.Y + box.Height, i);
            Main.spriteBatch.DrawLineBetween(new Vector2(box.X, i) + Main.screenPosition, new Vector2(box.X + box.Width, i) + Main.screenPosition, Color.Lerp(new Color(0, 150, 50) * 0.5f, new Color(0, 64, 44), value), 1f);
        }

        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/JadeCrescent").Value;
        Rectangle frame = tex.Frame(verticalFrames: 2, frameY: 1);
        for (int i = 0; i < 4; i++)
        {
            float rotation = 0f;
            Vector2 position = Vector2.Zero;

            switch (i)
            {
                case 0:
                    position = box.TopLeft() + new Vector2(-2, -2);
                    rotation = PiOver2;
                    break;
                case 1:
                    position = box.TopRight() + new Vector2(2, -2);
                    rotation = Pi;
                    break;
                case 2:
                    position = box.BottomLeft() + new Vector2(-2, 2);
                    break;
                case 3:
                    position = box.BottomRight() + new Vector2(2, 2);
                    rotation = -PiOver2;
                    break;
            }

            Main.spriteBatch.Draw(tex, position, frame, Color.White, rotation, frame.Size() * 0.5f, 1f, 0, 0);
        }

        return true;
    }
}

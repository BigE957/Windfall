using Terraria.UI;

namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    public static Texture2D FlipTexture2D(Texture2D input, bool vertical, bool horizontal)
    {
        Texture2D flipped = new Texture2D(input.GraphicsDevice, input.Width, input.Height);
        Color[] data = new Color[input.Width * input.Height];
        Color[] flipped_data = new Color[data.Length];

        input.GetData<Color>(data);

        for (int x = 0; x < input.Width; x++)
        {
            for (int y = 0; y < input.Height; y++)
            {
                int index = 0;
                if (horizontal && vertical)
                    index = input.Width - 1 - x + (input.Height - 1 - y) * input.Width;
                else if (horizontal && !vertical)
                    index = input.Width - 1 - x + y * input.Width;
                else if (!horizontal && vertical)
                    index = x + (input.Height - 1 - y) * input.Width;
                else if (!horizontal && !vertical)
                    index = x + y * input.Width;

                flipped_data[x + y * input.Width] = data[index];
            }
        }

        flipped.SetData<Color>(flipped_data);

        return flipped;
    }

    public static void SetRectangle(this UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }

    public static Vector2 Center(this UIElement uIElement) => new(uIElement.Left.Pixels + (uIElement.Width.Pixels / 2f), uIElement.Top.Pixels + (uIElement.Height.Pixels / 2f));
}

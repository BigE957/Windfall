namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static Vector2 ClampMagnitude(this Vector2 v, float min, float max)
    {
        return v.SafeNormalize(Vector2.UnitY) * MathHelper.Clamp(v.Length(), min, max);
    }

    public static float ManhattanDistance(this Vector2 a, Vector2 b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    public static Color LerpColors(float increment, params Color[] colors)
    {
        increment %= 0.999f;
        int num = (int)(increment * (float)colors.Length);
        Color value = colors[num];
        Color value2 = colors[(num + 1) % colors.Length];
        return Color.Lerp(value, value2, increment * (float)colors.Length % 1f);
    }

    public static Rectangle Expand(this Rectangle rect, int amt)
    {
        Rectangle newRect = rect;
        newRect.Width += amt;
        newRect.Height += amt;
        newRect.X -= amt / 2;
        newRect.Y -= amt / 2;
        return newRect;
    }
    public static Rectangle Expand(this Rectangle rect, int width, int height)
    {
        Rectangle newRect = rect;
        newRect.Width += width;
        newRect.Height += height;
        newRect.X -= width / 2;
        newRect.Y -= height / 2;
        return newRect;
    }

    public static Rectangle RectFromPoints(Point a, Point b)
    {
        int x = Math.Min(a.X, b.X);
        int y = Math.Min(a.Y, b.Y);

        int width = Math.Abs(a.X - b.X);
        int height = Math.Abs(a.Y - b.Y);


        return new Rectangle(x, y, width, height);
    }

    public static int RandFromRange((int start, int end) range) => range.start == range.end ? range.start : Main.rand.Next(range.start, range.end + 1);
}

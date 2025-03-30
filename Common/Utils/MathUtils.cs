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
}

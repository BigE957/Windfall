using Terraria.Utilities;

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

    /// <summary>
    ///     Rotates a vector's direction towards an ideal angle at a specific incremental rate.
    /// </summary>
    /// <param name="originalVector">The original vector to rotated from.</param>
    /// <param name="idealAngle">The ideal direction to approach.</param>
    /// <param name="angleIncrement">The maximum angular increment to make in the pursuit of approaching the destination.</param>
    public static Vector2 RotateTowards(this Vector2 originalVector, float idealAngle, float angleIncrement)
    {
        float initialAngle = originalVector.ToRotation();
        float newAngle = initialAngle.AngleTowards(idealAngle, angleIncrement);
        Vector2 newDirection = newAngle.ToRotationVector2();
        return newDirection * originalVector.Length();
    }

    public static Vector2 ClampLength(this Vector2 v, float min, float max) => v.SafeNormalize(Vector2.UnitY) * Clamp(v.Length(), min, max);

    /// <summary>
    ///     Rolls a random 0-1 probability based on a <see cref="UnifiedRandom"/> RNG, and checks whether it fits the criteria of a certain probability.
    /// </summary>
    /// <param name="rng">The random number generator.</param>
    /// <param name="probability">The probability of a success.</param>
    public static bool NextBool(this UnifiedRandom rng, float probability) => rng.NextFloat() < probability;

    /// <summary>
    ///     Determines the angular distance between two vectors based on dot product comparisons. This method ensures underlying normalization is performed safely.
    /// </summary>
    /// <param name="v1">The first vector.</param>
    /// <param name="v2">The second vector.</param>
    public static float AngleBetween(this Vector2 v1, Vector2 v2) => MathF.Acos(Vector2.Dot(v1.SafeNormalize(Vector2.Zero), v2.SafeNormalize(Vector2.Zero)));

    /// <summary>
    ///     A shorthand for <see cref="Utils.GetLerpValue(float, float, float, bool)"/> with <paramref name="clamped"/> defaulting to true.
    /// </summary>
    /// <param name="from">The value to interpolate from.</param>
    /// <param name="to">The value to interpolate to.</param>
    /// <param name="x">The value to interpolate in accordance with.</param>
    /// <param name="clamped">Whether outputs should be clamped between 0 and 1.</param>
    public static float InverseLerp(float from, float to, float x, bool clamped = true)
    {
        float interpolant = (x - from) / (to - from);
        if (!clamped)
            return interpolant;

        return MathHelper.Clamp(interpolant, 0f, 1f);
    }
}

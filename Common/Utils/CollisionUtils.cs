namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static bool CircularHitboxCollision(Vector2 centerCheckPosition, float radius, Rectangle targetHitbox)
    {
        if (new Rectangle((int)centerCheckPosition.X, (int)centerCheckPosition.Y, 1, 1).Intersects(targetHitbox))
            return true;

        float topLeftDist = Vector2.Distance(centerCheckPosition, targetHitbox.TopLeft());
        float TopRightDist = Vector2.Distance(centerCheckPosition, targetHitbox.TopRight());
        float BottomLeftDist = Vector2.Distance(centerCheckPosition, targetHitbox.BottomLeft());
        float BottomRightDist = Vector2.Distance(centerCheckPosition, targetHitbox.BottomRight());
        float dist = topLeftDist;
        if (TopRightDist < dist)
        {
            dist = TopRightDist;
        }

        if (BottomLeftDist < dist)
        {
            dist = BottomLeftDist;
        }

        if (BottomRightDist < dist)
        {
            dist = BottomRightDist;
        }

        return dist <= radius;
    }
}

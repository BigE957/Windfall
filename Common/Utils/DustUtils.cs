namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static void DustBox(Rectangle rect, int divisions = 10)
    {
        for (int i = 0; i < divisions + 1; i++)
        {
            Dust.NewDustPerfect(Vector2.Lerp(rect.TopLeft(), rect.TopRight(), i / (float)divisions).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
            Dust.NewDustPerfect(Vector2.Lerp(rect.TopLeft(), rect.BottomLeft(), i / (float)divisions).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
            Dust.NewDustPerfect(Vector2.Lerp(rect.TopRight(), rect.BottomRight(), i / (float)divisions).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
            Dust.NewDustPerfect(Vector2.Lerp(rect.BottomLeft(), rect.BottomRight(), i / (float)divisions).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
        }
    }

    public static void DustBox(Rectangle rect, int divisionsX = 10, int divisionsY = 10)
    {
        for (int i = 0; i < divisionsX + 1; i++)
        {
            Dust.QuickDust(Vector2.Lerp(rect.TopLeft(), rect.TopRight(), i / (float)divisionsX).ToWorldCoordinates(), Color.White);
            Dust.QuickDust(Vector2.Lerp(rect.BottomLeft(), rect.BottomRight(), i / (float)divisionsX).ToWorldCoordinates(), Color.White);
        }
        for (int i = 0; i < divisionsY + 1; i++)
        {
            Dust.QuickDust(Vector2.Lerp(rect.TopLeft(), rect.BottomLeft(), i / (float)divisionsY).ToWorldCoordinates(), Color.White);
            Dust.QuickDust(Vector2.Lerp(rect.TopRight(), rect.BottomRight(), i / (float)divisionsY).ToWorldCoordinates(), Color.White);
        }
    }
}

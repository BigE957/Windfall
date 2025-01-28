using Luminance.Common.VerletIntergration;
using System;

namespace Windfall.Common.Graphics.Verlet;
public static class WFVerletSimulations
{
    public static List<VerletSegment> CalamitySimulationWithWind(List<VerletSegment> segments, float segmentDistance, int loops = 10, float gravity = 0.3f)
    {
        foreach (VerletSegment segment in segments)
        {
            if (!segment.Locked)
            {
                Vector2 vector = segment.Position;
                segment.Position += segment.Position - segment.OldPosition;
                segment.Position += Vector2.UnitY * gravity;
                segment.Position += Vector2.UnitX * Main.windSpeedCurrent;
                segment.OldPosition = vector;
            }
        }

        int segmentCount = segments.Count;
        for (int k = 0; k < loops; k++)
        {
            for (int j = 0; j < segmentCount - 1; j++)
            {
                var pointA = segments[j];
                var pointB = segments[j + 1];
                Vector2 segmentCenter = (pointA.Position + pointB.Position) / 2f;
                Vector2 segmentDirection = (pointA.Position - pointB.Position).SafeNormalize(Vector2.UnitY);

                if (!pointA.Locked)// && !groundHitSegments.Contains(j))
                    pointA.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.Locked)// && !groundHitSegments.Contains(j + 1))
                    pointB.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }
        return segments;
    }
}

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

    public static List<VerletSegment> LuminanceSimulationWithWind(List<VerletSegment> segments, float segmentDistance, VerletSettings settings, int loops = 10)
    {
        List<int> groundHitSegments = [];
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var segment = segments[i];
            if (!segment.Locked)
            {
                Vector2 positionBeforeUpdate = segment.Position;

                Vector2 gravityForce = Vector2.UnitY * settings.Gravity;
                float maxFallSpeed = settings.MaxFallSpeed;

                if (settings.SlowInWater && Collision.WetCollision(segment.Position, 1, 1))
                {
                    gravityForce *= 0.4f;
                    maxFallSpeed *= 0.3f;
                }

                Vector2 velocity = segment.Velocity + gravityForce;
                if (settings.TileCollision)
                {
                    velocity = Collision.TileCollision(segment.Position, velocity, (int)segmentDistance, (int)segmentDistance);
                    groundHitSegments.Add(i);
                }

                if (velocity.Y > maxFallSpeed)
                    velocity.Y = maxFallSpeed;

                if (settings.ConserveEnergy)
                    segment.Position += (segment.Position - segment.OldPosition) * 0.3f;

                segment.Position += velocity;
                segment.Velocity = velocity;
                segment.Position.X = Lerp(segment.Position.X, segments[0].Position.X, 0.04f);

                segment.OldPosition = positionBeforeUpdate;
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

                if (!pointA.Locked && !groundHitSegments.Contains(j))
                    pointA.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.Locked && !groundHitSegments.Contains(j + 1))
                    pointB.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }

        return segments;
    }


}

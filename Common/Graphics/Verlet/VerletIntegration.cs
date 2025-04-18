namespace Windfall.Common.Graphics.Verlet;
public static class VerletIntegration
{
    public class VerletPoint(Vector2 start, bool locked)
    {
        public Vector2 Position = start, OldPosition = start;
        public bool Locked = locked;
        public List<(VerletPoint Point, float Length)> Connections = [];
    }

    public static void VerletSimulation(List<VerletPoint> chain, int loops = 10, float gravity = 0.3f, bool windAffected = true)
    {
        foreach (VerletPoint point in chain)
        {
            if (point.Locked)
                continue;

            Vector2 posBeforeUpdate = point.Position;
            point.Position += point.Position - point.OldPosition;
            point.Position += Vector2.UnitY * gravity;
            if (windAffected)
                point.Position += Vector2.UnitX * Main.windSpeedCurrent;
            point.OldPosition = posBeforeUpdate;
        }

        for (int i = 0; i < loops; i++)
        {
            foreach (VerletPoint point in chain)
            {
                if (point.Connections.Count == 0)
                    continue;

                for (int j = 0; j < point.Connections.Count; j++)
                {
                    float segmentDistance = point.Connections[j].Length;
                    if (segmentDistance == -1)
                        continue;
                    Vector2 segmentCenter = (point.Position + point.Connections[j].Point.Position) / 2f;
                    Vector2 segmentDirection = (point.Position - point.Connections[j].Point.Position).SafeNormalize(Vector2.UnitY);

                    if (!point.Locked)
                        point.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                    if (!point.Connections[j].Point.Locked)
                        point.Connections[j].Point.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                }
            }
        }
    }

    public static void VerletSimulation(List<VerletPoint>[] chains, int loops = 10, float gravity = 0.3f, bool windAffected = true)
    {
        foreach (List<VerletPoint> chain in chains)
            VerletSimulation(chain, loops, gravity, windAffected);
    }

    public static List<VerletPoint> CreateVerletChain(Vector2 startPos, Vector2 endPos, int count, float distBetween, bool lockStart = true, bool lockEnd = false)
    {
        List<VerletPoint> output = [];

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = Vector2.Lerp(startPos, endPos, i / (float)(count - 1));

            VerletPoint newPoint = new(spawnPos, i == 0 ? lockStart : i == count - 1 && lockEnd);
            output.Add(newPoint);
            if(i != 0)
                ConnectVerlets(output[i - 1], output[i], distBetween);
        }

        return output;
    }

    public static List<VerletPoint> CreateVerletBox(Rectangle r)
    {
        List<VerletPoint> output = [];

        output.Add(new(r.TopLeft(), false));
        output.Add(new(r.TopRight(), false));
        output.Add(new(r.BottomRight(), false));
        output.Add(new(r.BottomLeft(), false));

        ConnectVerlets(output[0], output[1], r.Width);
        ConnectVerlets(output[0], output[3], r.Height);
        ConnectVerlets(output[1], output[2], r.Height);
        ConnectVerlets(output[2], output[3], r.Width);
        ConnectVerlets(output[0], output[2], (r.TopLeft() - r.BottomRight()).Length());

        return output;
    }

    public static void ConnectVerlets(VerletPoint a, VerletPoint b, float length)
    {
        a.Connections.Add((b, length));
        a.Connections.Add((b, -1));
    }

    public static void BreakVerletConnection(VerletPoint a, VerletPoint b)
    {
        int index = a.Connections.FindIndex(c => c.Point == b);
        if (index != -1)
            a.Connections.RemoveAt(index);
        index = b.Connections.FindIndex(c => c.Point == a);
        if (index != -1)
            b.Connections.RemoveAt(index);
    }

    public static void RemoveVerletPoint(this List<VerletPoint> myList, int index)
    {
        VerletPoint p = myList[index];
        for(int i = 0; i < p.Connections.Count; i++)
            BreakVerletConnection(p, p.Connections[i].Point);

        myList.RemoveAt(index);
    }
    public static void RemoveVerletPoint(this List<VerletPoint> myList, VerletPoint p)
    {
        myList.RemoveVerletPoint(myList.FindIndex(c => c == p));
    }

    public static void AffectVerlets(List<VerletPoint> verlet, float dampening, float cap)
    {
        for (int k = 0; k < verlet.Count; k++)
            if (!verlet[k].Locked)
            {
                foreach (Player p in Main.ActivePlayers)
                    MoveChainBasedOnEntity(verlet, p, dampening / 2f, cap / 2f);

                foreach (Projectile proj in Main.ActiveProjectiles)
                    MoveChainBasedOnEntity(verlet, proj, dampening, cap);
            }
    }

    public static void MoveChainBasedOnEntity(List<VerletPoint> chain, Entity e, float dampening = 0.425f, float cap = 5f)
    {
        Vector2 entityVelocity = (e.velocity * dampening).ClampMagnitude(0f, cap);

        for (int i = 1; i < chain.Count - 1; i++)
        {
            VerletPoint segment = chain[i];
            VerletPoint next = chain[i + 1];
            float _ = 0f;
            if (Collision.CheckAABBvLineCollision(e.TopLeft, e.Size, segment.Position, next.Position, 20f, ref _))
            {
                // Weigh the entity's distance between the two segments.
                float distanceBetweenSegments = segment.Position.Distance(next.Position);
                float distanceToChains = e.Distance(segment.Position);
                float currentMovementOffsetInterpolant = Utilities.InverseLerp(distanceToChains, distanceBetweenSegments, distanceBetweenSegments * 0.2f);
                float nextMovementOffsetInterpolant = 1f - currentMovementOffsetInterpolant;

                // Move the segments based on the weight values.
                segment.Position += entityVelocity * currentMovementOffsetInterpolant;
                if (!next.Locked)
                    next.Position += entityVelocity * nextMovementOffsetInterpolant;
            }
        }
    }


}

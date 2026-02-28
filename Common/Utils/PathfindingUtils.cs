using static Windfall.Common.Systems.PathfindingSystem;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static bool AntiGravityPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, float maxSpeed = 6f, float accelMult = 1f, float tileAvoidanceStrength = 0.33f)
    {
        foreach (NPC user in Main.npc.Where(n => n.active && n.type == npc.type && n.whoAmI != npc.whoAmI))
        {
            Vector2 direction = (npc.Center - user.Center);
            float distance = direction.Length();
            direction = direction.SafeNormalize(Vector2.Zero);
            if (distance < 128)
                npc.velocity += direction * 16f / (distance + 1);
        }

        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
            return false;

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        float distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());

        while (distanceToWaypoint < 24)
        {
            currentWaypoint++;
            if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
                return false;
            distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        }

        int casts = 20;
        float rayDist = 180;
        for (int i = 0; i <= casts; i++)
        {
            Vector2 dir = (TwoPi / casts * i).ToRotationVector2();
            Vector2? endPos = RayCast(npc.Center, dir, rayDist, out float distMoved);
            if (endPos.HasValue)
            {
                npc.velocity -= (dir * rayDist / distMoved) * tileAvoidanceStrength;
            }
        }

        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);
        npc.velocity += waypointDirection * accelMult;

        npc.velocity = npc.velocity.ClampLength(0f, maxSpeed);

        return true;
    }

    public static bool GravityAffectedPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, out Vector2 velocity, out bool jumpStarted, float maxXSpeed, float jumpForce, float xAccelMult = 0.5f)
    {
        jumpStarted = false;
        velocity = npc.velocity;

        if (!IsPathValid(pathFinding, currentWaypoint))
            return false;
        if (!UpdateWaypoint(pathFinding, ref currentWaypoint, npc.Center))
            return false;

        INPCJumpController jumpCtrl = npc.ModNPC as INPCJumpController;

        if (jumpCtrl != null && jumpCtrl.IsJumping)
        {
            Vector2 target = jumpCtrl.JumpEndPoint;
            float remainingY = target.Y - npc.Center.Y;
            float vy = velocity.Y;
            float g = 0.4f;

            // Estimate remaining flight time using current vertical position and velocity
            // Solve 0.5*g*t^2 + vy*t - remainingY = 0  (assuming Y increases downward)
            float discriminant = vy * vy + 2 * g * remainingY;
            if (discriminant > 0)
            {
                float remainingTime = (-vy + (float)Math.Sqrt(discriminant)) / g; // positive time to land

                if (remainingTime > 0)
                {
                    float remainingX = target.X - npc.Center.X;
                    float vx = velocity.X;

                    // Desired horizontal acceleration to land exactly at target.X in remainingTime
                    // remainingX = vx * remainingTime + 0.5 * a * remainingTime^2
                    // => a = 2*(remainingX - vx*remainingTime) / (remainingTime^2)
                    float desiredAccel = 2 * (remainingX - vx * remainingTime) / (remainingTime * remainingTime);
                    velocity.X += Math.Clamp(desiredAccel, -jumpCtrl.MaxAirAccel, jumpCtrl.MaxAirAccel);
                }
            }

            velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);

            if (npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true))
                jumpCtrl.IsJumping = false;

            return true;
        }

        if (pathFinding.MyPath.IsGroundedPath && currentWaypoint < pathFinding.MyPath.Points.Length)
        {
            Point currentPoint = pathFinding.MyPath.Points[currentWaypoint];

            foreach (var jump in pathFinding.MyPath.Jumps)
            {
                if (currentPoint != jump.StartPoint)
                    continue;

                if (!npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true))
                    break;

                Vector2 start = jump.StartPoint.ToWorldCoordinates();
                Vector2 end = jump.EndPoint.ToWorldCoordinates();

                float dx = end.X - start.X;
                float dy = end.Y - start.Y;

                float g = 0.4f;

                bool directClear;
                {
                    Vector2 upDir = new(0, -1);
                    float upDist = Math.Abs(dy) + 32f;
                    Vector2? upHit = RayCast(start, upDir, upDist, out _);
                    directClear = !upHit.HasValue;
                }

                float lateralSign = 0f;
                float lateralClear = 0f;
                bool needsSideJump = false;

                if (!directClear)
                {
                    Vector2[] sides = dx >= 0 ? [new Vector2(1, 0), new Vector2(-1, 0)] : [new Vector2(-1, 0), new Vector2(1, 0)];

                    float checkDist = 6 * 16f;

                    foreach (var sideDir in sides)
                    {
                        RayCast(start, sideDir, checkDist, out float wallDist);
                        if (wallDist < 16f) 
                            continue;

                        Vector2 offsetPos = start + sideDir * wallDist;
                        float upDist = Math.Abs(dy) + 32f;
                        Vector2? vertHit = RayCast(offsetPos, new Vector2(0, -1), upDist, out _);

                        if (!vertHit.HasValue)
                        {
                            lateralSign = sideDir.X;
                            lateralClear = wallDist;
                            needsSideJump = true;
                            break;
                        }
                    }

                    if (!needsSideJump)
                    {
                        velocity.X = maxXSpeed * Math.Sign(dx == 0 ? 1 : dx);
                        velocity.Y = -jumpForce;
                        jumpStarted = true;
                        if (jumpCtrl != null)
                        {
                            jumpCtrl.IsJumping = true;
                            jumpCtrl.JumpEndPoint = end;
                        }
                        return true;
                    }
                }

                float t;
                float vy0;

                if (!needsSideJump)
                {
                    float idealT = Math.Abs(dx) / maxXSpeed;

                    // Minimum vy0 to reach dy in that time:
                    // dy = vy0*t + 0.5*g*t^2  =>  vy0 = (dy - 0.5*g*t^2) / t
                    float minVy0 = idealT > 0f
                        ? (dy - 0.5f * g * idealT * idealT) / idealT
                        : -jumpForce;

                    vy0 = Math.Clamp(minVy0, -jumpForce, -0.1f); // -0.1f prevents zero/upward launch

                    // Recompute actual flight time from chosen vy0
                    float disc = vy0 * vy0 + 2f * g * dy;
                    t = disc >= 0f
                        ? (-vy0 + MathF.Sqrt(disc)) / g
                        : -2f * vy0 / g;
                }
                else
                {
                    vy0 = Math.Min(-jumpForce, -MathF.Sqrt(2f * g * (Math.Abs(dy) + 16f)));
                    float disc = vy0 * vy0 + 2f * g * dy;
                    t = disc >= 0f ? (-vy0 + MathF.Sqrt(disc)) / g : -2f * vy0 / g;
                }

                if (t <= 0f) 
                    t = -2f * vy0 / g;

                if (needsSideJump)
                {
                    velocity.X = maxXSpeed * lateralSign;

                    float requiredVy = -MathF.Sqrt(2f * g * (Math.Abs(dy) + 16f)); // +16 for extra tile clearance
                    velocity.Y = Math.Min(vy0, requiredVy);
                }
                else
                {
                    velocity.X = t > 0f ? dx / t : maxXSpeed * Math.Sign(dx);
                    velocity.Y = vy0;
                }

                velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);
                if (jumpCtrl != null)
                {
                    jumpCtrl.IsJumping = true;
                    jumpCtrl.JumpEndPoint = end;
                }

                jumpStarted = true;
                return true;
            }
        }

        if (npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true))
        {
            Vector2 targetPoint = (currentWaypoint + 2 >= pathFinding.MyPath.Points.Length
                ? pathFinding.MyPath.Points[^1]
                : pathFinding.MyPath.Points[currentWaypoint + 2]).ToWorldCoordinates();

            Vector2 toWaypoint = targetPoint - npc.Center;
            Vector2 wayPointDir = toWaypoint.SafeNormalize(Vector2.Zero);

            velocity.X += Math.Sign(wayPointDir.X) * xAccelMult;
            velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);

            if (velocity.Y <= 0)
                Collision.StepUp(ref npc.position, ref velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }

        return true;
    }

    /// <summary>
    /// Checks if the path is valid and has points to follow.
    /// </summary>
    private static bool IsPathValid(PathFinding pathFinding, int currentWaypoint)
    {
        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
            return false;

        if (currentWaypoint > pathFinding.MyPath.Points.Length - 1)
            return false;

        return true;
    }

    /// <summary>
    /// Updates the current waypoint index based on NPC position and path
    /// </summary>
    /// <returns>False if the end of path is reached, True otherwise</returns>
    public static bool UpdateWaypoint(PathFinding pathFinding, ref int currentWaypoint, Vector2 npcPosition)
    {
        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        // Direction of movement (positive X = right, negative X = left)
        int direction = 0;

        // Determine movement direction based on path points
        if (currentWaypoint < pathFinding.MyPath.Points.Length - 1)
        {
            Vector2 current = pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates();
            Vector2 next = pathFinding.MyPath.Points[currentWaypoint + 1].ToWorldCoordinates();
            direction = next.X > current.X ? 1 : (next.X < current.X ? -1 : 0);
        }

        // Initialize variables for finding the best waypoint
        int bestWaypoint = currentWaypoint;
        float bestScore = float.MaxValue;

        // Calculate the target X position (ahead of the NPC in the movement direction)
        float targetX = direction != 0 ?
            npcPosition.X + (direction * 20) : // Look ahead in the direction of movement
            npcPosition.X;                     // Or use current position if no clear direction

        // Search through all points in the path
        for (int i = 0; i < pathFinding.MyPath.Points.Length; i++)
        {
            // Convert path point to world coordinates
            Vector2 pointWorldPos = pathFinding.MyPath.Points[i].ToWorldCoordinates();

            if (!Collision.CanHit(npcPosition, 4, 4, pointWorldPos, 4, 4))
                continue;

            // Calculate horizontal distance component (more important)
            float xDistance = Math.Abs(pointWorldPos.X - targetX);

            // Calculate vertical distance component (less important)
            float yDistance = Math.Abs(pointWorldPos.Y - npcPosition.Y);

            // Weighted score - prioritize X progress heavily
            float score = xDistance + (yDistance * 0.5f);

            // Special case: favor points that continue horizontal progress
            // This helps with gap crossing
            if (direction != 0)
            {
                // If this point is in the direction we want to move
                bool isInCorrectDirection = (direction > 0 && pointWorldPos.X > npcPosition.X) ||
                                            (direction < 0 && pointWorldPos.X < npcPosition.X);

                // Give bonus to points in the correct direction
                if (isInCorrectDirection)
                {
                    score *= 0.8f;
                }
                else
                {
                    // Penalize backtracking severely
                    score *= 1.5f;
                }
            }

            // If this point has a better score than our current best
            if (score < bestScore)
            {
                bestScore = score;
                bestWaypoint = i;
            }
        }

        // Update the current waypoint
        currentWaypoint = bestWaypoint;

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        return true;
    }

    public static bool IsWalkableThroughDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (IsSolidNotDoor(toPoint))
            return false;

        for(int i = 0; i < Math.Ceiling(NPC.height / 16f); i++)
            if (IsSolidNotDoor(toPoint - new Point(0, i)))
                return false;

        //Point dir = toPoint - fromPoint;
        //bool diagonal = Math.Abs(dir.X) != 0 && Math.Abs(dir.Y) != 0;

        return true;
    }

    public static bool IsWalkableNoDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (WorldGen.SolidTile(toPoint.X, toPoint.Y))
            return false;

        //Point dir = toPoint - fromPoint;
        //bool diagonal = Math.Abs(dir.X) != 0 && Math.Abs(dir.Y) != 0;

        return true;
    }

    public static int GravityCostFunction(Point p)
    {
        Vector2 ground = FindSurfaceBelow(p).ToWorldCoordinates();
        int penalty = (int)Vector2.Distance(p.ToWorldCoordinates(), ground) * 24;
        //penalty = (int)Math.Pow(penalty, 2);

        if (TileID.Sets.Platforms[Main.tile[p].TileType])
            penalty += 100;

        if (TileID.Sets.Platforms[Main.tile[p + new Point(0, 1)].TileType])
            penalty -= 100;
        /*
        for (int i = -1; i < 1; i++)
        {
            if (IsSolidNotDoor(p + new Point(-1, i)))
            {
                penalty += 9999;
                break;
            }

            if (IsSolidNotDoor(p + new Point(1, i)))
            {
                penalty += 9999;
                break;
            }
        }
        
        if (IsSolidNotDoor(p + new Point(0, -1)))
            penalty += 9999;
        */
        return penalty;
    }

}

public interface INPCJumpController
{
    bool IsJumping { get; set; }
    Vector2 JumpEndPoint { get; set; }
    float MaxAirAccel { get; }          // per‑NPC air acceleration limit
}
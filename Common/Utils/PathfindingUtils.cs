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
        //Main.NewText(dist);
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
        //Calculate Missing Values
        float testJump = jumpForce;
        float jumpHeight = 0;
        float jumpLength = 0;
        while (testJump > 0)
        {
            jumpLength += maxXSpeed;
            jumpHeight += testJump;
            testJump -= 0.3f;
        }
        int finalJumpheight = (int)Math.Ceiling(jumpHeight / 16);

        while (jumpHeight > 0)
        {
            jumpLength += maxXSpeed;
            jumpHeight += testJump;
            if (testJump > 10)
                testJump -= 0.3f;
            else
                testJump = -10;
        }
        int finalJumpLength = (int)Math.Ceiling(jumpLength / 16) + 2;
        return GravityAffectedPathfindingMovement(npc, pathFinding, ref currentWaypoint, out velocity, out jumpStarted, maxXSpeed, jumpForce, xAccelMult, finalJumpheight, finalJumpLength);
    }
    
    public static bool GravityAffectedPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, out Vector2 velocity, out bool jumpStarted, float maxXSpeed = 4, float jumpForce = 12, float xAccelMult = 0.5f, int jumpHeight = 8, int jumpLength = 10)
    {
        // Initialize output parameters
        jumpStarted = false;
        velocity = npc.velocity;

        // Check if we have a valid path to follow
        if (!IsPathValid(pathFinding, currentWaypoint))
            return false;

        // Update our current waypoint if we've reached it
        if (!UpdateWaypoint(pathFinding, ref currentWaypoint, npc.Center))
            return false;

        //Dust.NewDustPerfect(pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates(), DustID.Terra).velocity /= 2f;

        // Get the direction to the next waypoint
        Vector2 waypointDirection = CalculateWaypointDirection(pathFinding, currentWaypoint, npc);

        // Check if we need to jump and can jump
        Point standingTilePosition = npc.Bottom.ToTileCoordinates();
        bool canJump = npc.IsGrounded(standingTilePosition, true);
        bool shouldJump = DetermineIfShouldJump(waypointDirection, standingTilePosition, npc, out bool gapJump);
        //Main.NewText(gapJump);
        // Handle advanced jump logic (when we might need to jump over obstacles)
        if (canJump && shouldJump && gapJump)
        {
            ProcessJumpLogic(ref shouldJump, pathFinding, currentWaypoint, standingTilePosition,
                velocity, jumpHeight, jumpLength, ref jumpStarted);
        }

        // Perform the jump if needed
        if (canJump && shouldJump)
        {
            //Main.NewText(waypointDirection.X);
            velocity.X = maxXSpeed * waypointDirection.X;
            velocity.Y = -jumpForce;
            jumpStarted = true;
        }

        // Apply horizontal movement and limit speed
        Vector2 targetPoint = (currentWaypoint + 2 >= pathFinding.MyPath.Points.Length ? pathFinding.MyPath.Points[^1] : pathFinding.MyPath.Points[currentWaypoint + 2]).ToWorldCoordinates();
        Vector2 toWaypoint = (targetPoint - npc.Center).SafeNormalize(npc.velocity.SafeNormalize(Vector2.Zero));
        //Dust.NewDustPerfect(targetPoint, DustID.Terra, Vector2.Zero);
        velocity.X += Math.Sign(toWaypoint.X) * xAccelMult;
        velocity.X = Clamp(velocity.X, -maxXSpeed, maxXSpeed);

        // Handle collision step-up
        if (velocity.Y <= 0)
            Collision.StepUp(ref npc.position, ref velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

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
    /// Calculates the direction to the next waypoint.
    /// </summary>
    private static Vector2 CalculateWaypointDirection(PathFinding pathFinding, int currentWaypoint, NPC npc)
    {
        // If we're heading to the last waypoint, aim directly at it       
        if (currentWaypoint + 1 >= pathFinding.MyPath.Points.Length)
        {
            Vector2 targetPoint = pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates();
            return (targetPoint - npc.Center).SafeNormalize(npc.velocity.SafeNormalize(Vector2.Zero));
        }
        // Otherwise aim at the next waypoint
        else
        {
            Vector2 targetPoint = pathFinding.MyPath.Points[currentWaypoint + 1].ToWorldCoordinates();
            return (targetPoint - pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates()).SafeNormalize(npc.velocity.SafeNormalize(Vector2.Zero));
        }
    }

    /// <summary>
    /// Determines if the NPC should attempt to jump based on path direction and terrain.
    /// </summary>
    private static bool DetermineIfShouldJump(Vector2 waypointDirection, Point standingTilePosition, NPC npc, out bool gapJump)
    {
        gapJump = false;

        // Don't jump if we're explicitly trying to go downward
        if (waypointDirection.Y == 1)
            return false;

        // Jump if we're trying to move sharply upward
        if (waypointDirection.Y < -0.94f)
            return true;
        //Main.NewText(waypointDirection);
        // Jump if there's no solid ground ahead in our movement direction
        bool solidAhead = IsSolidOrPlatform(standingTilePosition + new Point(npc.velocity.X == 0 ? npc.direction : Math.Sign(npc.velocity.X), 0));
        bool solidBelow = IsSolidOrPlatform(standingTilePosition);
        bool solidInWay = false;
        
        for (int i = 2; i <= Math.Ceiling(npc.height / 16f); i++)
        {
            Point checkPos = (npc.direction == -1 ? npc.BottomLeft : npc.BottomRight).ToTileCoordinates() + new Point(npc.direction == -1 ? -1 : 0, -i);
            //Dust.NewDustPerfect(checkPos.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
            if (IsSolidNotDoor(checkPos))
            {
                solidInWay = true;
                break;
            }
        }
        //Main.NewText(solidInWay);
        gapJump = !solidInWay;

        return !solidAhead || !solidBelow || solidInWay;
    }

    /// <summary>
    /// Processes complex jump logic to determine if jumping is necessary and possible.
    /// </summary>
    private static void ProcessJumpLogic(ref bool shouldJump, PathFinding pathFinding, int currentWaypoint,
        Point standingTilePosition, Vector2 velocity, int jumpHeight, int jumpLength, ref bool jumpStarted)
    {
        shouldJump = false;
        bool needJump = true;

        // Check if the path leads downward (no need to jump)
        int checkIndex = Math.Min(currentWaypoint + 8, pathFinding.MyPath.Points.Length - 1);
        if (pathFinding.MyPath.Points[checkIndex].Y > pathFinding.MyPath.Points[currentWaypoint].Y)
        {
            needJump = false;
        }
        else
        {
            // Check if there are obstacles that can be walked around
            needJump = !CanWalkAroundObstacle(standingTilePosition, velocity, jumpHeight, jumpLength);
        }

        // If we need to jump, check if we can make it
        if (needJump)
        {
            shouldJump = CanMakeJump(standingTilePosition, velocity.X, jumpLength);

            if (!shouldJump)
            {
                jumpStarted = true;
                // Cannot make the jump, will need alternate routing
            }
        }
    }

    /// <summary>
    /// Checks if the NPC can walk around an obstacle without jumping.
    /// </summary>
    private static bool CanWalkAroundObstacle(Point standingTilePosition, Vector2 velocity, int jumpHeight, int jumpLength)
    {
        // Dont care about one tile gaps
        if (IsSolidOrPlatform(standingTilePosition + new Point(Math.Sign(velocity.X) * 2, 0)))
            return true;

        // Check if there's any easy way to simply climb up instead of needing to jump the gap
        for (int i = jumpLength / 2; i > 1; i--)
        {
            int checkX = standingTilePosition.X + ((i - 1) * Math.Sign(velocity.X));

            for (int j = 1; j < 4; j++)
            {
                Point checkPoint = new(checkX, standingTilePosition.Y + j);
                if (IsSolidOrPlatform(checkPoint))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the NPC can make the jump over a gap.
    /// </summary>
    private static bool CanMakeJump(Point standingTilePosition, float velocityX, int jumpLength)
    {
        int movementDirection = Math.Sign(velocityX);

        // Look ahead to see if there's a landing point within jump distance
        for (int i = 0; i < jumpLength; i++)
        {
            int checkPointX = standingTilePosition.X + ((jumpLength - i) * movementDirection);
            Point checkPoint = new(checkPointX, standingTilePosition.Y);

            if (IsSolidOrPlatform(checkPoint))
                return true;
        }

        return false;
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
        int penalty = (int)Vector2.Distance(p.ToWorldCoordinates(), ground) * 12;

        if (TileID.Sets.Platforms[Main.tile[p].TileType])
            penalty += 100;
        
        if (TileID.Sets.Platforms[Main.tile[p + new Point(0,1)].TileType])
            penalty -= 100;

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

        return penalty;
    }

}

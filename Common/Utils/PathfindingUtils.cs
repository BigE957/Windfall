using static Windfall.Common.Systems.PathfindingSystem;

namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    // -------------------------------------------------------------------------
    // Flying NPC movement (unchanged)
    // -------------------------------------------------------------------------

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
            Vector2 dir = (MathHelper.TwoPi / casts * i).ToRotationVector2();
            Vector2? endPos = RayCast(npc.Center, dir, rayDist, out float distMoved);
            if (endPos.HasValue)
                npc.velocity -= (dir * rayDist / distMoved) * tileAvoidanceStrength;
        }

        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);
        npc.velocity += waypointDirection * accelMult;
        npc.velocity = npc.velocity.ClampLength(0f, maxSpeed);

        return true;
    }

    // -------------------------------------------------------------------------
    // Grounded NPC movement
    // -------------------------------------------------------------------------

    public static bool GravityAffectedPathfindingMovement(
        NPC npc,
        PathFinding pathFinding,
        ref int currentWaypoint,
        out Vector2 velocity,
        out bool jumpStarted,
        float maxXSpeed,
        float jumpForce,
        float xAccelMult = 0.5f)
    {
        jumpStarted = false;
        velocity = npc.velocity;

        if (!IsPathValid(pathFinding, currentWaypoint))
            return false;
        if (!UpdateWaypoint(pathFinding, ref currentWaypoint, npc.Center))
            return false;

        INPCJumpController jumpCtrl = npc.ModNPC as INPCJumpController;

        // ----- Active drop-through -----
        if (jumpCtrl != null && jumpCtrl.IsDroppingThrough)
        {
            Point floorTile = npc.Bottom.ToTileCoordinates();

            if (npc.IsGrounded(floorTile, true))
            {
                if (TileID.Sets.Platforms[Main.tile[floorTile.X, floorTile.Y].TileType])
                {
                    // Still on a platform — nudge through it
                    npc.position.Y += 2;
                    float ddx = jumpCtrl.DropThroughTarget.X - npc.Center.X;
                    velocity.X += Math.Sign(ddx) * xAccelMult;
                    velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);
                }
                else
                {
                    // Landed on solid ground below — drop complete
                    jumpCtrl.IsDroppingThrough = false;
                }
            }
            else
            {
                // Airborne during drop — steer toward target X
                float ddx = jumpCtrl.DropThroughTarget.X - npc.Center.X;
                velocity.X += Math.Sign(ddx) * xAccelMult;
                velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);

                // Check if we've landed
                if (npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true))
                    jumpCtrl.IsDroppingThrough = false;
            }

            return true;
        }

        // ----- Active jump — adaptive steering -----
        if (jumpCtrl != null && jumpCtrl.IsJumping)
        {
            Vector2 target = jumpCtrl.JumpEndPoint;
            float remainingY = target.Y - npc.Center.Y;
            float vy = velocity.Y;
            float g = 0.4f;

            float discriminant = vy * vy + 2 * g * remainingY;
            if (discriminant > 0)
            {
                float remainingTime = (-vy + (float)Math.Sqrt(discriminant)) / g;
                if (remainingTime > 0)
                {
                    float remainingX = target.X - npc.Center.X;
                    float vx = velocity.X;
                    float desiredAccel = 2 * (remainingX - vx * remainingTime) / (remainingTime * remainingTime);
                    float accel = Math.Clamp(desiredAccel, -jumpCtrl.MaxAirAccel, jumpCtrl.MaxAirAccel);
                    velocity.X += accel;
                }
            }

            velocity.X = Math.Clamp(velocity.X, -maxXSpeed, maxXSpeed);

            if (npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true))
            {
                jumpCtrl.IsJumping = false;
                jumpCtrl.JumpHorizontalAccel = 0f;
            }

            return true;
        }

        // ----- Check for jump/drop at current waypoint -----
        // Use proximity rather than exact tile match so the NPC triggers the jump
        // as it approaches the launch point instead of only on the exact tile.
        // This prevents the NPC from walking past the launch tile and then jumping
        // from a position that clips the ledge above.
        if (pathFinding.MyPath.IsGroundedPath && currentWaypoint < pathFinding.MyPath.Points.Length)
        {
            const float JumpTriggerRadius = 20f; // pixels — how close to launch point before jumping

            foreach (var jump in pathFinding.MyPath.Jumps)
            {
                Vector2 launchWorld = jump.StartPoint.ToWorldCoordinates();

                // Trigger when the NPC's foot position is within JumpTriggerRadius of the
                // launch point AND the NPC is moving toward or at the launch point X.
                float distToLaunch = Vector2.Distance(npc.Bottom, launchWorld);
                if (distToLaunch > JumpTriggerRadius) continue;

                if (!npc.IsGrounded(npc.Bottom.ToTileCoordinates(), true)) break;

                // ----- Drop-through -----
                if (jump.IsDropThrough)
                {
                    if (jumpCtrl != null)
                    {
                        jumpCtrl.IsDroppingThrough = true;
                        jumpCtrl.DropThroughTarget = jump.EndPoint.ToWorldCoordinates();
                    }
                    npc.position.Y += 2;
                    return true;
                }

                // ----- Regular drop (walk off edge) -----
                if (jump.IsDrop)
                {
                    if (jumpCtrl != null)
                    {
                        jumpCtrl.IsJumping = true;
                        jumpCtrl.JumpEndPoint = jump.EndPoint.ToWorldCoordinates();
                        jumpCtrl.JumpHorizontalAccel = 0f;
                        jumpCtrl.RemainingJumpTime = 0f;
                    }
                    return true;
                }

                // ----- Jump arc -----
                // Use NPC.Bottom for horizontal steering (dx) so vx tracks where the NPC
                // actually is.  However, derive vy0 and flight time from the *planned*
                // tile-to-tile dy (matching TryAddJumpEdge's own calculation).
                //
                // Why: NPC.Bottom sits at the TOP of the ground tile, which is 8 px below
                // the tile center used by ToWorldCoordinates().  Using npc.Bottom.Y for dy
                // makes same-height jumps appear as dy = -8 (target above), triggering
                // minimum-force mode and robbing the NPC of the velocity needed to clear
                // the gap.  Deriving dy from the stored tile positions avoids this mismatch.
                Vector2 actualLaunch = npc.Bottom;
                Vector2 end = jump.EndPoint.ToWorldCoordinates();

                float dx = end.X - actualLaunch.X;
                // Planned tile-to-tile vertical distance (same formula used in TryAddJumpEdge)
                float dyPlanned = (jump.EndPoint.Y - jump.StartPoint.Y) * 16f;
                float g = 0.4f;

                // Matches TryAddJumpEdge physics exactly:
                //   dyPlanned < 0  (target above)  → minimum vy0 to reach height, capped at jumpForce
                //   dyPlanned >= 0 (same/below)    → full jumpForce for maximum flight time
                float vy0 = dyPlanned < 0f
                    ? Math.Max(-MathF.Sqrt(2f * g * MathF.Abs(dyPlanned)), -jumpForce)
                    : -jumpForce;

                float disc = vy0 * vy0 + 2f * g * dyPlanned;
                float t = disc >= 0f ? (-vy0 + MathF.Sqrt(disc)) / g : 2f * MathF.Abs(vy0) / g;
                if (t <= 0f) t = 2f * MathF.Abs(vy0) / g;

                velocity.X = t > 0f ? Math.Clamp(dx / t, -maxXSpeed, maxXSpeed)
                                    : maxXSpeed * Math.Sign(dx);
                velocity.Y = vy0;

                if (jumpCtrl != null)
                {
                    jumpCtrl.IsJumping = true;
                    jumpCtrl.JumpEndPoint = end;
                    jumpCtrl.JumpHorizontalAccel = 0f;
                    jumpCtrl.RemainingJumpTime = t;
                }

                jumpStarted = true;
                return true;
            }
        }

        // ----- Normal grounded movement -----
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
        else
        {
            if (jumpCtrl != null)
                velocity.X += jumpCtrl.JumpHorizontalAccel;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Path validation helpers
    // -------------------------------------------------------------------------

    private static bool IsPathValid(PathFinding pathFinding, int currentWaypoint)
    {
        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
            return false;
        if (currentWaypoint > pathFinding.MyPath.Points.Length - 1)
            return false;
        return true;
    }

    public static bool UpdateWaypoint(PathFinding pathFinding, ref int currentWaypoint, Vector2 npcPosition)
    {
        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        int direction = 0;
        if (currentWaypoint < pathFinding.MyPath.Points.Length - 1)
        {
            Vector2 current = pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates();
            Vector2 next = pathFinding.MyPath.Points[currentWaypoint + 1].ToWorldCoordinates();
            direction = next.X > current.X ? 1 : (next.X < current.X ? -1 : 0);
        }

        int bestWaypoint = currentWaypoint;
        float bestScore = float.MaxValue;
        float targetX = direction != 0 ? npcPosition.X + (direction * 20) : npcPosition.X;

        for (int i = 0; i < pathFinding.MyPath.Points.Length; i++)
        {
            Vector2 pointWorldPos = pathFinding.MyPath.Points[i].ToWorldCoordinates();

            if (!Collision.CanHit(npcPosition, 4, 4, pointWorldPos, 4, 4))
                continue;

            float xDistance = Math.Abs(pointWorldPos.X - targetX);
            float yDistance = Math.Abs(pointWorldPos.Y - npcPosition.Y);
            float score = xDistance + (yDistance * 0.5f);

            if (direction != 0)
            {
                bool isInCorrectDirection =
                    (direction > 0 && pointWorldPos.X > npcPosition.X) ||
                    (direction < 0 && pointWorldPos.X < npcPosition.X);
                score *= isInCorrectDirection ? 0.8f : 1.5f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestWaypoint = i;
            }
        }

        currentWaypoint = bestWaypoint;

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        return true;
    }

    // -------------------------------------------------------------------------
    // Walkable functions
    // -------------------------------------------------------------------------

    public static bool IsWalkableThroughDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (IsSolidNotDoor(toPoint))
            return false;

        for (int i = 0; i < Math.Ceiling(NPC.height / 16f); i++)
            if (IsSolidNotDoor(toPoint - new Point(0, i)))
                return false;

        return true;
    }

    public static bool IsWalkableNoDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (WorldGen.SolidTile(toPoint.X, toPoint.Y))
            return false;
        return true;
    }

    // -------------------------------------------------------------------------
    // Cost functions (tile A*)
    // -------------------------------------------------------------------------

    public static int GravityCostFunction(Point p)
    {
        Vector2 ground = FindSurfaceBelow(p).ToWorldCoordinates();
        int penalty = (int)Vector2.Distance(p.ToWorldCoordinates(), ground) * 24;

        if (TileID.Sets.Platforms[Main.tile[p].TileType])
            penalty += 100;

        if (TileID.Sets.Platforms[Main.tile[p + new Point(0, 1)].TileType])
            penalty -= 100;

        return penalty;
    }
}

// -------------------------------------------------------------------------
// INPCJumpController interface
// -------------------------------------------------------------------------

public interface INPCJumpController
{
    bool IsJumping { get; set; }
    float JumpHorizontalAccel { get; set; }
    Vector2 JumpEndPoint { get; set; }
    float MaxAirAccel { get; }
    float RemainingJumpTime { get; set; }

    // Drop-through state
    bool IsDroppingThrough { get; set; }
    Vector2 DropThroughTarget { get; set; }
}
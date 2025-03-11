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
            if (distance < 64)
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

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                Vector2 dir = new(x, y);
                dir.Normalize();
                if (!Collision.CanHit(npc.Center, 1, 1, npc.Center + dir * 96, 1, 1))
                    npc.velocity -= dir * tileAvoidanceStrength;
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
        jumpStarted = false;
        velocity = npc.velocity;

        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
            return false;

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            return false;

        float distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        //Main.NewText(dist);
        while (distanceToWaypoint < Math.Max(npc.width, npc.height) / 1.66f)
        {
            currentWaypoint++;
            if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
                return false;
            
            distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        }
        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);

        Point beneathTilePoint = npc.Bottom.ToTileCoordinates();
        bool canJump = (npc.velocity.Y == 0 && npc.oldVelocity.Y == 0.3f) || Main.tile[beneathTilePoint + new Point(0, -1)].LiquidAmount > 0;
        bool shouldJump =
            waypointDirection.Y != 1 &&
            ((waypointDirection.Y < -0.95f) ||
            !IsSolidOrPlatform(beneathTilePoint + new Point(Math.Sign(velocity.X), 0)));
                
        if (canJump && shouldJump && !(waypointDirection.Y < -0.95f))
        {
            shouldJump = false;
            bool needJump = true;
            //Check if we need to Jump
            int checkIndex = 4;
            if (pathFinding.MyPath.Points.Length <= currentWaypoint + checkIndex)
                checkIndex = pathFinding.MyPath.Points.Length - 1;
            if (pathFinding.MyPath.Points[checkIndex].Y > pathFinding.MyPath.Points[currentWaypoint].Y)
            {
                //Main.NewText("Path moves downward");
                needJump = false;
            }
            else
            {
                for (int i = jumpLength / 2; i > 1; i--)
                {
                    int X = beneathTilePoint.X + ((i - 1) * Math.Sign(velocity.X));
                    if (!Collision.CanHit(npc.Center, 1, 1, new Point(X, (int)(npc.Center.Y / 16f)).ToWorldCoordinates(), 1, 1))
                    {
                        //Main.NewText("Jump is blocked");
                        needJump = false;
                        break;
                    }
                    if (!needJump)
                        break;

                    for (int j = 1; j < jumpHeight; j++)
                    {
                        Point P = new(X, beneathTilePoint.Y + j);
                        if (IsSolidOrPlatform(P))
                        {
                            //Dust.NewDustPerfect(P.ToWorldCoordinates(), DustID.Terra, Vector2.Zero, Scale: 2f);
                            //Main.NewText("Don't need Jump");
                            needJump = false;
                            break;
                        }
                    }
                    if (!needJump)
                        break;
                }
            }

            // Check if we can make the jump            
            if (needJump)
            {
                //Main.NewText("Need Jump");

                for (int i = 0; i < jumpLength; i++)
                {
                    int checkPointX = beneathTilePoint.X + ((jumpLength - i) * Math.Sign(velocity.X));
                    Point checkPoint = new(checkPointX, beneathTilePoint.Y);
                    if (IsSolidOrPlatform(checkPoint))
                    {
                        //Main.NewText("Found Jump");
                        //Dust.NewDustPerfect(checkPoint.ToWorldCoordinates(), DustID.Terra, Vector2.Zero, Scale: 2f);
                        shouldJump = true;
                        break;
                    }
                    //else
                    //Dust.NewDustPerfect(checkPoint.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero, Scale: 2f);
                }

                if (!shouldJump)
                {
                    jumpStarted = true;
                    //Main.NewText("CANT MAKE IT STOP!!!");
                    return false;
                }
            }
        }

        if (canJump && shouldJump)
        {
            //Main.NewText(waypointDirection.Y);
            velocity.X = maxXSpeed * Math.Sign(waypointDirection.X) * (waypointDirection.Y < -0.95f && Main.rand.NextBool() ? -1 : 1);
            velocity.Y = -jumpForce;
            jumpStarted = true;
        }
        velocity.X += Math.Sign(waypointDirection.X) * xAccelMult;
        velocity.X = Clamp(velocity.X, -maxXSpeed, maxXSpeed);

        if(velocity.Y <= 0)
            Collision.StepUp(ref npc.position, ref velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        //if (velocity.Y >= 0)
        //    Collision.StepDown(ref npc.position, ref velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        return true;
    }

    public static bool IsWalkableThroughDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (IsSolidNotDoor(new(toPoint.X, toPoint.Y)))
            return false;


        //Direction
        int dirX = toPoint.X - fromPoint.X;
        int dirY = toPoint.Y - fromPoint.Y;

        // Adjacent points can use simplified logic
        if (Math.Abs(dirX) <= 1 && Math.Abs(dirY) <= 1)
        {
            // For diagonal movement only, check the two orthogonal tiles
            if (dirX != 0 && dirY != 0)
            {
                if (IsSolidNotDoor(new(fromPoint.X + dirX, fromPoint.Y)) ||
                    IsSolidNotDoor(new(fromPoint.X, fromPoint.Y + dirY)))
                    return false;
            }
            return true;
        }

        // Scale direction for midpoint
        dirX /= 2;
        dirY /= 2;

        int midX = fromPoint.X + dirX;
        int midY = fromPoint.Y + dirY;

        // Check if the midpoint is solid
        if (IsSolidNotDoor(new(midX, midY)))
            return false;

        // For diagonal movement, check additional points
        if (dirX != 0 && dirY != 0)
        {
            if (IsSolidNotDoor(new(fromPoint.X + dirX, fromPoint.Y)) ||
                IsSolidNotDoor(new(fromPoint.X, fromPoint.Y + dirY)))
                return false;
        }

        // Calculate perpendicular direction
        int perpX = -dirY;
        int perpY = dirX;

        // Normalize to ensure we have direction, not magnitude
        if (perpX != 0 || perpY != 0)
        {
            int gcd = GreatestCommonDivisor(Math.Abs(perpX), Math.Abs(perpY));
            if (gcd > 1)
            {
                perpX /= gcd;
                perpY /= gcd;
            }
        }

        if (dirX == 0 || dirY == 0)
        {
            Point perpPos = fromPoint + new Point(perpX, perpY);
            Point perpNeg = fromPoint - new Point(perpX, perpY);
            bool startLeftBlocked = IsSolidNotDoor(new(perpNeg.X, perpNeg.Y));
            bool startRightBlocked = IsSolidNotDoor(new(perpPos.X, perpPos.Y));

            perpPos = toPoint + new Point(perpX, perpY);
            perpNeg = toPoint - new Point(perpX, perpY);
            bool toLeftBlocked = IsSolidNotDoor(new(perpNeg.X, perpNeg.Y));
            bool toRightBlocked = IsSolidNotDoor(new(perpPos.X, perpPos.Y));

            if ((startLeftBlocked && toRightBlocked) || (startRightBlocked && toLeftBlocked))
                return false;
        }

        // Determine the length to check based on NPC dimensions
        int npcTileWidth = (int)Math.Round(NPC.width / 16f);
        int npcTileHeight = (int)Math.Round(NPC.height / 16f);

        int length;
        if (perpX != 0 && perpY != 0)
            length = Math.Max(npcTileWidth, npcTileHeight);
        else if (perpX == 0)
            length = npcTileHeight;
        else
            length = npcTileWidth;

        int gapSize = 1; // Start at 1 due to midpoint being walkable
        //Dust.NewDustPerfect(fromPoint.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero).noGravity = true;
        //Dust.NewDustPerfect(toPoint.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero).noGravity = true;
        // Check in both positive and negative directions
        for (int i = 1; i < length && gapSize < length; i++)
        {
            bool posPathClear = true;
            bool negPathClear = true;

            int posX = midX + (perpX * i);
            int posY = midY + (perpY * i);

            int negX = midX - (perpX * i);
            int negY = midY - (perpY * i);

            // For diagonal perpendicular directions, additional corner checks
            if (perpX != 0 && perpY != 0)
            {
                // Check diagonal corners in positive direction
                if (IsSolidNotDoor(new(midX + (perpX * (i - 1)), posY)) ||
                    IsSolidNotDoor(new(posX, midY + (perpY * (i - 1)))))
                {
                    if (i == 1)
                        return false;
                    posPathClear = false;
                }

                // Check diagonal corners in negative direction
                if (IsSolidNotDoor(new(midX - (perpX * (i - 1)), negY)) ||
                    IsSolidNotDoor(new(negX, midY - (perpY * (i - 1)))))
                {
                    if (i == 1)
                        return false;
                    negPathClear = false;
                }
            }

            // Check main tile in positive direction
            if (posPathClear && !IsSolidNotDoor(new(posX, posY)) && !IsSolidNotDoor(new(posX - dirX, posY - dirY)))
            {
                //Dust.NewDustPerfect(new Point(posX, posY).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
                gapSize++;
            }
            else
                posPathClear = false;

            // Check main tile in negative direction
            if (negPathClear && !IsSolidNotDoor(new(negX, negY)) && !IsSolidNotDoor(new(negX - dirX, negY - dirY)))
            {
                //Dust.NewDustPerfect(new Point(negX, negY).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
                gapSize++;
            }
            else
                negPathClear = false;

            if (gapSize >= length)
                return true;

            if (!posPathClear && !negPathClear)
                return false;
        }

        return false;
    }

    public static bool IsWalkableNoDoors(this NPC NPC, Point fromPoint, Point toPoint)
    {
        if (WorldGen.SolidTile(toPoint.X, toPoint.Y))
            return false;


        //Direction
        int dirX = toPoint.X - fromPoint.X;
        int dirY = toPoint.Y - fromPoint.Y;

        // Adjacent points can use simplified logic
        if (Math.Abs(dirX) <= 1 && Math.Abs(dirY) <= 1)
        {
            // For diagonal movement only, check the two orthogonal tiles
            if (dirX != 0 && dirY != 0)
            {
                if (WorldGen.SolidTile(fromPoint.X + dirX, fromPoint.Y) ||
                    WorldGen.SolidTile(fromPoint.X, fromPoint.Y + dirY))
                    return false;
            }
            return true;
        }

        // Scale direction for midpoint
        dirX /= 2;
        dirY /= 2;

        int midX = fromPoint.X + dirX;
        int midY = fromPoint.Y + dirY;

        // Check if the midpoint is solid
        if (WorldGen.SolidTile(midX, midY))
            return false;

        // For diagonal movement, check additional points
        if (dirX != 0 && dirY != 0)
        {
            if (WorldGen.SolidTile(fromPoint.X + dirX, fromPoint.Y) ||
                WorldGen.SolidTile(fromPoint.X, fromPoint.Y + dirY))
                return false;
        }

        // Calculate perpendicular direction
        int perpX = -dirY;
        int perpY = dirX;

        // Normalize to ensure we have direction, not magnitude
        if (perpX != 0 || perpY != 0)
        {
            int gcd = GreatestCommonDivisor(Math.Abs(perpX), Math.Abs(perpY));
            if (gcd > 1)
            {
                perpX /= gcd;
                perpY /= gcd;
            }
        }

        if (dirX == 0 || dirY == 0)
        {
            Point perpPos = fromPoint + new Point(perpX, perpY);
            Point perpNeg = fromPoint - new Point(perpX, perpY);
            bool startLeftBlocked = WorldGen.SolidTile(perpNeg.X, perpNeg.Y);
            bool startRightBlocked = WorldGen.SolidTile(perpPos.X, perpPos.Y);

            perpPos = toPoint + new Point(perpX, perpY);
            perpNeg = toPoint - new Point(perpX, perpY);
            bool toLeftBlocked = WorldGen.SolidTile(perpNeg.X, perpNeg.Y);
            bool toRightBlocked = WorldGen.SolidTile(perpPos.X, perpPos.Y);

            if ((startLeftBlocked && toRightBlocked) || (startRightBlocked && toLeftBlocked))
                return false;
        }

        // Determine the length to check based on NPC dimensions
        int npcTileWidth = (int)Math.Round(NPC.width / 16f);
        int npcTileHeight = (int)Math.Round(NPC.height / 16f);

        int length;
        if (perpX != 0 && perpY != 0)
            length = Math.Max(npcTileWidth, npcTileHeight);
        else if (perpX == 0)
            length = npcTileHeight;
        else
            length = npcTileWidth;

        int gapSize = 1; // Start at 1 due to midpoint being walkable
        //Dust.NewDustPerfect(fromPoint.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero).noGravity = true;
        //Dust.NewDustPerfect(toPoint.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero).noGravity = true;
        // Check in both positive and negative directions
        for (int i = 1; i < length && gapSize < length; i++)
        {
            bool posPathClear = true;
            bool negPathClear = true;

            int posX = midX + (perpX * i);
            int posY = midY + (perpY * i);

            int negX = midX - (perpX * i);
            int negY = midY - (perpY * i);

            // For diagonal perpendicular directions, additional corner checks
            if (perpX != 0 && perpY != 0)
            {
                // Check diagonal corners in positive direction
                if (WorldGen.SolidTile(midX + (perpX * (i - 1)), posY) ||
                    WorldGen.SolidTile(posX, midY + (perpY * (i - 1))))
                {
                    if (i == 1)
                        return false;
                    posPathClear = false;
                }

                // Check diagonal corners in negative direction
                if (WorldGen.SolidTile(midX - (perpX * (i - 1)), negY) ||
                    WorldGen.SolidTile(negX, midY - (perpY * (i - 1))))
                {
                    if (i == 1)
                        return false;
                    negPathClear = false;
                }
            }

            // Check main tile in positive direction
            if (posPathClear && !WorldGen.SolidTile(posX, posY) && !WorldGen.SolidTile(posX - dirX, posY - dirY))
            {
                //Dust.NewDustPerfect(new Point(posX, posY).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
                gapSize++;
            }
            else
                posPathClear = false;

            // Check main tile in negative direction
            if (negPathClear && !WorldGen.SolidTile(negX, negY) && !WorldGen.SolidTile(negX - dirX, negY - dirY))
            {
                //Dust.NewDustPerfect(new Point(negX, negY).ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
                gapSize++;
            }
            else
                negPathClear = false;

            if (gapSize >= length)
                return true;

            if (!posPathClear && !negPathClear)
                return false;
        }

        return false;
    }

    public static int GravityCostFunction(Point p)
    {
        Vector2 ground = FindSurfaceBelow(p).ToWorldCoordinates();
        int distanceToGround = (int)Vector2.Distance(p.ToWorldCoordinates(), ground);
        if (distanceToGround < 36)
            return 0;
        //if (distanceToGround > 400)
        //    return 3200;
        return (distanceToGround - 36) * 8;
    }

}

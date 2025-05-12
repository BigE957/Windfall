using Windfall.Common.Systems;
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
        jumpStarted = false;
        velocity = npc.velocity;
        
        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
            return false;
        
        if (currentWaypoint > pathFinding.MyPath.Points.Length - 1)
            return false;
        
        float distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        
        while (distanceToWaypoint < Math.Max(npc.width, npc.height))
        {
            currentWaypoint++;
            if (currentWaypoint > pathFinding.MyPath.Points.Length - 1)
                return false;
            
            distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        }

        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);

        Point beneathTilePoint = npc.Bottom.ToTileCoordinates();
        bool canJump = (npc.velocity.Y == 0 && npc.oldVelocity.Y == 0.3f) || Main.tile[beneathTilePoint + new Point(0, -1)].LiquidAmount > 0;
        bool shouldJump =
            waypointDirection.Y != 1 &&
            ((waypointDirection.Y < -0.8f) ||
            !IsSolidOrPlatform(beneathTilePoint + new Point(Math.Sign(velocity.X), 0)));
        
        if (canJump && shouldJump && !(waypointDirection.Y < -0.8f))
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
            velocity.X = maxXSpeed * waypointDirection.X;
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


        foreach(Point dir in PathfindingSystem.Dirs)
        {
            if (IsSolidNotDoor(p + dir))
            {
                penalty += 9999;
                break;
            }
        }

        return penalty;
    }

}

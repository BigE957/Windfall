using static Windfall.Common.Systems.PathfindingSystem;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static void AntiGravityPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, float accelMult = 1f)
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
        {
            npc.velocity *= 0.9f;
            return;
        }

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
        {
            npc.velocity *= 0.9f;
            return;
        }

        float distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        //Main.NewText(dist);
        while (distanceToWaypoint < 24)
        {
            currentWaypoint++;
            distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        }

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
        {
            npc.velocity *= 0.9f;
            return;
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
                    npc.velocity -= dir * 0.33f;
            }
        }

        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);
        npc.velocity += waypointDirection * accelMult;

        npc.velocity = npc.velocity.ClampLength(0f, 6f);
        npc.rotation = npc.velocity.ToRotation() + Pi;
    }
    
    public static bool GravityAffectedPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, out Vector2 velocity, out bool shouldStop, out bool jumpStarted, float maxXSpeed, float xAccelMult, float jumpForce, float gravityForce)
    {
        //Calculate Missing Values
        float testJump = jumpForce;
        float jumpHeight = 0;
        float jumpLength = 0;
        while (testJump > 0)
        {
            jumpLength += maxXSpeed;
            jumpHeight += testJump;
            testJump -= gravityForce;
        }
        int finalJumpheight = (int)Math.Ceiling(jumpHeight / 16);

        while (jumpHeight > 0)
        {
            jumpLength += maxXSpeed;
            jumpHeight += testJump;
            testJump -= gravityForce;
        }
        int finalJumpLength = (int)Math.Ceiling(jumpLength / 16);

        return GravityAffectedPathfindingMovement(npc, pathFinding, ref currentWaypoint, out velocity, out shouldStop, out jumpStarted, maxXSpeed, xAccelMult, jumpForce, gravityForce, finalJumpheight, finalJumpLength);
    }
    
    public static bool GravityAffectedPathfindingMovement(NPC npc, PathFinding pathFinding, ref int currentWaypoint, out Vector2 velocity, out bool shouldStop, out bool jumpStarted, float maxXSpeed = 4, float xAccelMult = 0.5f, float jumpForce = 12, float gravityForce = 0.66f, int jumpHeight = 8, int jumpLength = 10)
    {
        shouldStop = false;
        jumpStarted = false;
        velocity = npc.velocity;

        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0)
        {
            velocity.X *= 0.9f;
            velocity.Y += gravityForce;
            return true;
        }

        if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
        {
            velocity.X *= 0.9f;
            velocity.Y += gravityForce;
            return true;
        }

        float distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        //Main.NewText(dist);
        while (distanceToWaypoint < 24)
        {
            currentWaypoint++;
            if (currentWaypoint >= pathFinding.MyPath.Points.Length - 1)
            {
                velocity.X *= 0.9f;
                velocity.Y += gravityForce;
                return true;
            }
            distanceToWaypoint = Vector2.Distance(npc.Center, pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates());
        }

        Vector2 waypointDirection = (pathFinding.MyPath.Points[currentWaypoint].ToWorldCoordinates() - npc.Center).SafeNormalize(Vector2.Zero);

        Point beneathTilePoint = npc.Bottom.ToTileCoordinates();
        bool canJump = (npc.oldVelocity.Y == 0.66f || npc.oldVelocity.Y == 0) && velocity.Y == 0;
        bool shouldJump =
            waypointDirection.Y != 1 &&
            ((waypointDirection.Y < -0.975f) ||
            !IsSolidOrPlatform(beneathTilePoint + new Point(Math.Sign(velocity.X), 0)));

        if (canJump && shouldJump && !(waypointDirection.Y < -0.975f))
        {
            shouldJump = false;
            bool needJump = true;
            //Check if we need to Jump
            int checkIndex = 6;
            if (pathFinding.MyPath.Points.Length <= currentWaypoint + checkIndex)
                checkIndex = pathFinding.MyPath.Points.Length - 1;
            if (pathFinding.MyPath.Points[checkIndex].Y * 16 > npc.Center.Y + 48)
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

                for (int i = 0; i <= jumpLength; i++)
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
                    shouldStop = true;
                    //Main.NewText("CANT MAKE IT STOP!!!");
                }
            }
        }


        if (shouldStop)
        {
            velocity = Vector2.Zero;
            return false;
        }
        else
        {
            if (canJump && shouldJump)
            {
                velocity.X = maxXSpeed * Math.Sign(velocity.X);
                velocity.Y = -jumpForce;
                jumpStarted = true;
            }
            else
                velocity.Y += gravityForce;

            velocity.X += waypointDirection.X * xAccelMult;
            velocity.X = Clamp(velocity.X, -maxXSpeed, maxXSpeed);

            Collision.StepUp(ref npc.position, ref velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            //Collision.StepDown(ref NPC.position, ref velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }

        return true;
    }
}

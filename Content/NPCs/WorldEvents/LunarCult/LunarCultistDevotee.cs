using DialogueHelper.UI.Dialogue;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Projectiles.Other;
using static Windfall.Common.Systems.PathfindingSystem;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class LunarCultistDevotee : ModNPC
{
    private enum States
    {
        //Selenic Order Base
        Idle,
        Chatting,
        CafeteriaEvent,
        RitualEvent,
        StaticCharacter,
        //Other States
        TabletChase,
        TabletGrabCutscene,
        SlowToStop,
        Enemy
    }
    private States AIState
    {
        get => (States)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }
    public enum Character
    {
        NewClothes,
        Eeper
    }
    public Character myCharacter;
    public bool characterSpokenTo = false;

    public Vector2 goalPosition = Vector2.Zero;
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/SelenicCultistDevotee";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[Type] = 14;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        NPCID.Sets.AllowDoorInteraction[Type] = true;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 24;
        NPC.height = 48;
        NPC.aiStyle = -1;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 400;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;

        Time = 0;
        pathFinding = new();
        CurrentWaypoint = 0;
        jumpTimer = 0;
        airTime = 0;
        tripTime = 0;
        TargetPos = Vector2.Zero;
    }
    public override void OnSpawn(IEntitySource source)
    {
        // AIState = States.Enemy;
        switch (AIState)
        {
            case States.CafeteriaEvent:
                NPC.ai[3] = LunarCultBaseSystem.CustomerQueue.Count;
                LunarCultBaseSystem.CustomerQueue.Add(new LunarCultBaseSystem.Customer(NPC, LunarCultBaseSystem.MenuFoodIDs[Main.rand.Next(LunarCultBaseSystem.MenuFoodIDs.Count)]));                    
                NPC.direction = -1;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.X = 3;
                break;
            case States.StaticCharacter:
                NPC.alpha = 0;
                NPC.noGravity = false;
                NPC.aiStyle = -1;
                break;
            case States.Enemy:
                NPC.damage = 100;
                // NPC.friendly = false;
                NPC.dontTakeDamageFromHostiles = true;
                break;
            default:
                NPC.alpha = 255;
                Vector2 oldPos = NPC.position;
                NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

                float altY = 0;
                for (int i = 0; i < 2; i++)
                {
                    altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
                    if (altY < NPC.position.Y)
                        NPC.position.Y = altY;
                }

                NPC.alpha = 0;
                for (int i = 0; i <= 50; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                SoundEngine.PlaySound(SpawnSound, NPC.Center);
                break;
        }
    }

    private int Time = 0;
    private PathFinding pathFinding = new();
    private int CurrentWaypoint = 0;
    private int jumpTimer = 0;
    private int airTime = 0;
    private int tripTime = 0;
    public Vector2 TargetPos = Vector2.Zero;

    private int playerAgro
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }
    private enum AttackState
    {
        None,
        Ranged,
        Melee
    }
    private AttackState Attack
    {
        get => (AttackState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    public override void AI()
    {
        switch (AIState)
        {
            #region Selenic Order Base
            case States.Idle:
                int index = NPC.FindClosestPlayer();
                if (index != -1)
                {
                    Player player = Main.player[index];
                    if (player.Center.X > NPC.Center.X)
                        NPC.direction = 1;
                    else
                        NPC.direction = -1;
                }
                break;
            case States.CafeteriaEvent:
                const int queueGap = 50;
                int queueIndex = (int)NPC.ai[3];
                if (queueIndex == -1 || !LunarCultBaseSystem.Active)
                {
                    if (NPC.velocity.X < 1.5f)
                        NPC.velocity.X += 0.05f;
                    else
                        NPC.velocity.X = 1.5f;

                    float goalY = (LunarCultBaseSystem.LunarCultBaseLocation.Y * 16 - 96) - NPC.height;
                    if (NPC.velocity.Y >= 0 && NPC.position.Y >= goalY)
                    {
                        NPC.position.Y = goalY;
                        if (NPC.velocity.Y != 0)
                            NPC.velocity.Y = 0;
                    }
                    if (NPC.position.Y < goalY)
                        NPC.velocity.Y += 0.5f;

                    NPC.direction = 1;
                    if (NPC.Center.X - (LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - (380 * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1))) > 800)
                    {
                        for (int i = 0; i <= 50; i++)
                        {
                            int dustStyle = Main.rand.NextBool() ? 66 : 263;
                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                            dust.noGravity = true;
                            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                        }
                        SoundEngine.PlaySound(SpawnSound, NPC.Center);
                        NPC.active = false;
                    }
                }
                else
                {
                    Vector2 goalPosition = new(LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - (320 * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1)) + queueGap * queueIndex, LunarCultBaseSystem.LunarCultBaseLocation.Y * 16 - 96);
                    float angerRatio = (LunarCultBaseSystem.CustomerQueue.Where(c => c.HasValue).Count() - 4) / ((float)LunarCultBaseSystem.CustomerLimit - 4);
                    if (LunarCultBaseSystem.CustomerQueue.Where(c => c.HasValue).Count() <= 4)
                        angerRatio = 0f;
                    if (NPC.velocity.Y >= 0 && NPC.position.Y >= goalPosition.Y - NPC.height)
                    {
                        NPC.position.Y = goalPosition.Y - NPC.height;
                        if (NPC.velocity.Y != 0)
                            NPC.velocity.Y = 0;
                    }
                    if (NPC.velocity.Y == 0 && NPC.position.Y == goalPosition.Y - NPC.height && Main.rand.NextBool(angerRatio))
                    {
                        if(Main.rand.NextBool(5))
                            CombatText.NewText(NPC.Hitbox, Color.Lerp(Color.White, Color.Red, angerRatio), GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Madge." + Main.rand.Next(6)));
                        if (Main.rand.NextBool())
                            NPC.velocity.Y = -4;
                    }
                    if (NPC.position.Y < goalPosition.Y - NPC.height)
                        NPC.velocity.Y += 0.5f;
                    if (queueIndex != 0 && !LunarCultBaseSystem.CustomerQueue[queueIndex - 1].HasValue)
                    {
                        goalPosition.X -= queueGap;
                        if (NPC.Center.X - goalPosition.X < queueGap / 2)
                        {
                            LunarCultBaseSystem.CustomerQueue[queueIndex - 1] = LunarCultBaseSystem.CustomerQueue[queueIndex];
                            if (queueIndex + 1 == LunarCultBaseSystem.CustomerQueue.Count)
                                LunarCultBaseSystem.CustomerQueue.RemoveAt(queueIndex);
                            else
                                LunarCultBaseSystem.CustomerQueue[queueIndex] = null;
                            NPC.ai[3] -= 1;
                        }
                    }
                    if (goalPosition.X < NPC.Center.X)
                        if (NPC.velocity.X > -1.5f)
                            NPC.velocity.X -= 0.05f;
                        else
                            NPC.velocity.X = -1.5f;
                    else
                    {
                        NPC.velocity.X = 0;
                        if (queueIndex == 0 && !Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI))
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(Main.rand.NextFloat(0f, 2f), -2.5f), ModContent.ProjectileType<FoodAlert>(), 0, 0f, ai0: LunarCultBaseSystem.CustomerQueue[queueIndex].Value.OrderID, ai1: Main.rand.Next(3), ai2: NPC.whoAmI);
                    }
                }
                break;
            case States.RitualEvent:
                if (NPC.velocity.Y != 0)
                {
                    bool touchingPortal = false;
                    foreach (NPC portal in Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<PortalMole>()))
                    {
                        if (portal.Hitbox.Contains((int)NPC.Center.X, (int)NPC.Center.Y))
                            touchingPortal = true;
                    }
                    if (touchingPortal)
                    {
                        NPC.scale -= 0.0175f;
                        if (NPC.scale < 0.5f)
                        {
                            NPC.Opacity -= 0.05f;
                            if (NPC.scale < 0.1f)
                            {
                                LunarCultBaseSystem.RemainingCultists--;
                                if (LunarCultBaseSystem.RemainingCultists <= 0)
                                    LunarCultBaseSystem.ResetTimer();
                                NPC.active = false;
                            }
                        }
                    }
                    else
                    {
                        if (NPC.oldRot[0] == NPC.rotation)
                        {
                            NPC.velocity.X *= 0.95f;
                            if (NPC.velocity.X < 0.1f)
                                NPC.velocity.X = 0f;
                        }
                        if (NPC.velocity.Y == 0)
                            NPC.rotation = 0f;
                        if (NPC.scale < 1f)
                            NPC.scale += 0.05f;
                        else
                            NPC.scale = 1f;

                        if (NPC.Opacity < 1f)
                            NPC.Opacity += 0.05f;
                        else
                            NPC.Opacity = 1f;
                    }
                }
                else if (Math.Abs(goalPosition.X - NPC.Center.X) > 4)
                {
                    NPC.rotation = 0f;
                    if (NPC.Center.X > goalPosition.X)
                    {
                        NPC.velocity.X = -2;
                        NPC.spriteDirection = -1;
                    }
                    else
                    {
                        NPC.velocity.X = 2;
                        NPC.spriteDirection = 1;
                    }
                }
                else
                {
                    NPC.velocity.X = 0;
                    if(NPC.Center.X > LunarCultBaseSystem.ActivityCoords.X)
                        NPC.spriteDirection = -1;
                    else
                        NPC.spriteDirection = 1;
                }

                break;
            case States.StaticCharacter:
                if (myCharacter == Character.Eeper)
                {
                    if (!characterSpokenTo && (Main.GlobalTimeWrappedHourly - (int)Main.GlobalTimeWrappedHourly) < 0.015)
                    {
                        CombatText z = Main.combatText[CombatText.NewText(new((int)NPC.Center.X, (int)NPC.Bottom.Y, 1, 1), Color.LimeGreen, "Z", true)];
                        z.lifeTime /= 2;
                    }
                }
                break;
            #endregion
            #region Tablet Chase Sequence
            case States.TabletChase:
                if (jumpTimer > 0)
                    jumpTimer--;

                if (tripTime > 0)
                    tripTime--;

                if ((NPC.velocity.Y != 0 || NPC.oldVelocity.Y != 0.3f) && !WorldGen.SolidOrSlopedTile(NPC.Bottom.ToTileCoordinates().X, NPC.Bottom.ToTileCoordinates().Y))
                {
                    if (jumpTimer == 0)
                        airTime++;
                    else
                        airTime = 16;
                }
                else
                {
                    if (airTime >= 76)
                    {
                        NPC.velocity.X = 0;
                        tripTime = 120;
                    }
                    airTime = 0;
                    jumpTimer = 0;
                }               

                if (airTime < 76 && tripTime == 0)
                {
                    if ((NPC.velocity == Vector2.Zero && NPC.oldVelocity.Y == 0.3f) || (pathFinding.MyPath.Points.Length - CurrentWaypoint <= 3 && Vector2.DistanceSquared(TargetPos, pathFinding.MyPath.Points[^1].ToWorldCoordinates()) > 1600))
                    {
                        pathFinding.FindPathInArea(NPC.Center, TargetPos, NPC.IsWalkableThroughDoors, DraconicRuinsSystem.DraconicRuinsArea, NPC.noGravity ? null : GravityCostFunction);

                        CurrentWaypoint = 1;
                    }

                    float MoveSpeed = 2f;
                    if (pathFinding.MyPath != null)
                    {
                        /*
                        for (int i = 0; i < pathFinding.MyPath.Points.Length; i++)
                        {
                            Particle p = new GlowOrbParticle(pathFinding.MyPath.Points[i].ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, i == CurrentWaypoint ? Color.White : Color.Red);
                            GeneralParticleHandler.SpawnParticle(p);
                        }
                        */
                        MoveSpeed = pathFinding.MyPath.Points.Length / 10f + 2;
                    }
                    if (MoveSpeed > 4f)
                        MoveSpeed = 4f;

                    bool MovementSuccess = GravityAffectedPathfindingMovement(NPC, pathFinding, ref CurrentWaypoint, out NPC.velocity, out bool jumpStarted, MoveSpeed, 8.5f, 0.25f);
                    float dist = Vector2.DistanceSquared(TargetPos, NPC.Center);

                    if (Collision.CanHit(NPC.Center, 1, 1, TargetPos, 1, 1) && dist < 50000)
                    {
                        Vector2 TabletRoom = DraconicRuinsSystem.TabletRoom.ToWorldCoordinates();
                        airTime = 0;
                        jumpTimer = 0;                     

                        if (TargetPos == TabletRoom && DraconicRuinsSystem.State == DraconicRuinsSystem.CutsceneState.CultistFumble && !DraconicRuinsSystem.CutsceneActive)
                        {
                            DraconicRuinsSystem.StartCutscene();

                            NPC.direction = Math.Sign(NPC.velocity.X);

                            AIState = States.TabletGrabCutscene;
                            Time = 0;
                        }
                        else if(TargetPos == TabletRoom || dist < 1600)
                        {
                            AIState = States.SlowToStop;
                            if (TargetPos == TabletRoom)
                                Time = Main.rand.Next(-60, 0);
                            else
                                Time = 0;
                        }
                        return;
                    }

                    if (!MovementSuccess)
                        NPC.velocity.X *= 0.8f;
                    if (jumpStarted)
                        jumpTimer = 30;

                    if (NPC.velocity.X != 0 && airTime < 60 && tripTime == 0)
                        NPC.direction = Math.Sign(NPC.velocity.X);

                    if (NPC.direction == -1)
                    {
                        Point p = NPC.Left.ToTileCoordinates();
                        p.X -= 1;
                        bool opened = TryOpenDoor(p, -1);
                    }
                    else
                    {
                        Point p = NPC.Right.ToTileCoordinates();
                        //p.X += 1;
                        bool opened = TryOpenDoor(p, -1);
                    }
                }

                if (airTime == 76)
                    NPC.direction = -NPC.direction;

                break;
            case States.TabletGrabCutscene:
                int time = Time - 16;
                if (time < 76)
                    NPC.velocity.X = 3 * (DraconicRuinsSystem.FacingLeft ? -1 : 1);
                else if (NPC.velocity.X != 0)
                    NPC.velocity.X *= 0.95f;
                if (time == 48)
                {
                    jumpTimer = 40;
                    NPC.velocity.Y = -6;
                }

                if (time == 56)
                {
                    NPC tablet = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())];
                    tablet.ai[0] = 4;
                    tablet.ai[1] = NPC.whoAmI;
                }

                if (time >= 88 && time < 148)
                {
                    if (time == 88)
                    {
                        NPC tablet = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())];
                        tablet.ai[0] = 5;
                        tablet.velocity = new(NPC.direction * 3f, -4f);

                        NPC.direction *= -1;
                    }
                    if (time < 136)
                    {                       
                        tripTime = -1;
                    }
                    else if(time == 136)
                    {
                        NPC.rotation -= 0.2f * NPC.direction;
                    }
                    else
                    {
                        NPC.rotation = 0;
                        tripTime = 2;
                    }

                }

                if (jumpTimer > 0)
                    jumpTimer--;

                break;
            #endregion
            case States.SlowToStop:
                if(NPC.velocity.X != 0)
                    NPC.direction = Math.Sign(NPC.velocity.X);
                if (Time >= 0)
                    NPC.velocity.X *= 0.8f;
                break;
            case States.Enemy:
                if(playerAgro != -1)
                {
                    Player target = Main.player[playerAgro];

                    switch(Attack)
                    {
                        case AttackState.Melee:
                            if (Time == 0)
                            {
                                NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
                                NPC.velocity.X = 0;
                            }
                            if(Time >= 12)
                            {
                                if (Time == 12)
                                    NPC.velocity.X = 8 * NPC.direction;
                                else
                                    NPC.velocity.X *= 0.95f;
                                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                                if (Math.Abs(NPC.velocity.X) < 1f)
                                {
                                    Attack = AttackState.None;
                                    pathFinding.FindPathInRadius(NPC.Center, target.Center, NPC.IsWalkableThroughDoors, NPC.noGravity ? null : GravityCostFunction, searchRadius: 800f);
                                }
                            }

                            NPC.spriteDirection = NPC.direction;
                            Time++;
                            return;
                        case AttackState.Ranged:
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction) * 12f, ProjectileID.WoodenArrowHostile, 100, 0.5f);
                            Attack = AttackState.None;
                            break;
                    }
                    if (jumpTimer > 0)
                        jumpTimer--;

                    if ((NPC.velocity.Y != 0 || NPC.oldVelocity.Y != 0.3f) && !WorldGen.SolidOrSlopedTile(NPC.Bottom.ToTileCoordinates().X, NPC.Bottom.ToTileCoordinates().Y))
                    {
                        if (jumpTimer == 0)
                            airTime++;
                        else
                            airTime = 16;
                    }
                    else
                    {
                        airTime = 0;
                        jumpTimer = 0;
                    }

                    if (pathFinding.MyPath == null || (NPC.velocity == Vector2.Zero && NPC.oldVelocity.Y == 0.3f) || (pathFinding.MyPath.Points.Length - CurrentWaypoint <= 3 && Vector2.DistanceSquared(TargetPos, pathFinding.MyPath.Points[^1].ToWorldCoordinates()) > 1600))
                    {
                        if (NPC.ai[3] == 0)
                            pathFinding.FindPathInRadius(NPC.Center, target.Center, NPC.IsWalkableThroughDoors, NPC.noGravity ? null : GravityCostFunction, searchRadius: 800f);
                        else
                            pathFinding.FindPathInArea(NPC.Center, target.Center, NPC.IsWalkableThroughDoors, LunarCultBaseSystem.CultBaseTileArea, NPC.noGravity ? null : GravityCostFunction);

                        CurrentWaypoint = 1;
                    }

                    float MoveSpeed = 4f;

                    if (pathFinding.MyPath != null)
                    {
                        /*
                        for (int i = 0; i < pathFinding.MyPath.Points.Length; i++)
                        {
                            Particle p = new GlowOrbParticle(pathFinding.MyPath.Points[i].ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, i == CurrentWaypoint ? Color.White : Color.Red);
                            GeneralParticleHandler.SpawnParticle(p);
                        }
                        */
                        if (Attack == AttackState.None)
                        {
                            if (pathFinding.MyPath.Points.Length < 4 && airTime == 0)
                            {
                                Attack = AttackState.Melee;
                                Time = 0;
                                return;
                            }
                            else if (Time % 60 == 0 && Main.rand.NextBool(4))
                            {
                                Attack = AttackState.Ranged;
                                Time++;
                                NPC.netUpdate = true;
                                return;
                            }
                        }
                    }

                    bool MovementSuccess = GravityAffectedPathfindingMovement(NPC, pathFinding, ref CurrentWaypoint, out NPC.velocity, out bool jumpStarted, MoveSpeed, 8.5f, 0.25f);
                    float dist = Vector2.DistanceSquared(TargetPos, NPC.Center);

                    if (!MovementSuccess)
                        playerAgro = -1;
                    if (jumpStarted)
                        jumpTimer = 30;

                    if (NPC.velocity.X != 0 && airTime < 60 && tripTime == 0)
                        NPC.direction = Math.Sign(NPC.velocity.X);

                    if (NPC.direction == -1)
                    {
                        Point p = NPC.Left.ToTileCoordinates();
                        p.X -= 1;
                        bool opened = TryOpenDoor(p, -1);
                    }
                    else
                    {
                        Point p = NPC.Right.ToTileCoordinates();
                        //p.X += 1;
                        bool opened = TryOpenDoor(p, -1);
                    }
                }
                else
                {
                    foreach(Player p in Main.player)
                    {
                        if (NPC.ai[3] == 0)
                        {
                            if (Collision.CanHit(NPC, p) && (p.Center - NPC.Center).LengthSquared() < 360000)
                            {
                                playerAgro = p.whoAmI;
                                break;
                            }
                        }
                        else
                        {
                            if(LunarCultBaseSystem.CultBaseWorldArea.Contains(p.Center.ToPoint()))
                            {
                                playerAgro = p.whoAmI;
                                break;
                            }
                        }
                    }

                    //Wander
                    NPC.velocity.X = 0;
                }
                break;
        }
        NPC.spriteDirection = NPC.direction;

        Time++;
    }
    public override bool CanChat() => AIState == States.StaticCharacter || AIState == States.CafeteriaEvent && NPC.ai[3] == 0 && NPC.velocity.X == 0;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        switch(AIState)
        {
            case States.StaticCharacter:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"SelenicCultists/{myCharacter}", new(Name, [NPC.whoAmI]), characterSpokenTo ? 1 : 0);
                characterSpokenTo = true;
                break;
            case States.CafeteriaEvent:
                if (Main.player[Main.myPlayer].HeldItem.type == LunarCultBaseSystem.CustomerQueue[0].Value.OrderID)
                {
                    Main.player[Main.myPlayer].HeldItem.stack--;

                    if (LunarCultBaseSystem.CustomerQueue.Count == 1)
                        LunarCultBaseSystem.CustomerQueue = [];
                    else
                        LunarCultBaseSystem.CustomerQueue[0] = null;
                    if (Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI))
                        Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI).ai[2] = -1;
                    NPC.ai[3] = -1;
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Thanks." + Main.rand.Next(3)));
                    LunarCultBaseSystem.SatisfiedCustomers++;
                    if (LunarCultBaseSystem.SatisfiedCustomers == LunarCultBaseSystem.CustomerGoal)
                    {
                        NPC chef = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())];
                        CombatText.NewText(chef.Hitbox, Color.LimeGreen, GetWindfallTextValue("Dialogue.LunarCult.TheChef.Activity.AlmostDone"), true);
                    }
                }
                else
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Where." + Main.rand.Next(3)));
                break;
        }

        return "Rizz"; //Won't actually be seen.
    }

    public override bool CheckActive() => false;

    const int frameCountX = 9;

    public override void FindFrame(int frameHeight)
    {
        int frameWidth = TextureAssets.Npc[NPC.type].Value.Width / frameCountX;
        NPC.frame.Width = frameWidth;

        switch (AIState)
        {
            case States.SlowToStop:
            case States.TabletGrabCutscene:
            case States.TabletChase:
                if(tripTime == -1)
                {
                    NPC.frame.X = frameWidth * 7;
                    NPC.frame.Y = 0;
                }
                else if(tripTime > 0)
                {
                    NPC.frame.X = frameWidth * 6;
                    NPC.frame.Y = 0;
                }
                else if (airTime < 16 && jumpTimer == 0)
                {
                    NPC.frame.X = frameWidth * 4;
                    NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);

                    if (NPC.velocity.X == 0 && (NPC.frame.Y / frameHeight == 4 || NPC.frame.Y / frameHeight == 5 || NPC.frame.Y / frameHeight == 12 || NPC.frame.Y / frameHeight == 13 || NPC.frame.Y / frameHeight == 14))
                    {
                        NPC.frame.X = frameWidth * 3;
                        NPC.frame.Y = 0;
                    }
                    else
                    {
                        NPC.frameCounter += 0.2f * (Math.Abs(NPC.velocity.X) + 1);
                        NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);
                    }

                }
                else
                {
                    if(airTime > 76)
                        NPC.frame.X = frameWidth * 8;
                    else
                        NPC.frame.X = frameWidth * 5;
                    NPC.frame.Y = 0;
                }
                break;
            case States.Enemy:
                if (airTime < 16 && jumpTimer == 0)
                {
                    NPC.frame.X = frameWidth * 4;
                    NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);

                    if (NPC.velocity.X == 0 && (NPC.frame.Y / frameHeight == 4 || NPC.frame.Y / frameHeight == 5 || NPC.frame.Y / frameHeight == 12 || NPC.frame.Y / frameHeight == 13 || NPC.frame.Y / frameHeight == 14))
                    {
                        NPC.frame.X = frameWidth * 3;
                        NPC.frame.Y = 0;
                    }
                    else
                    {
                        NPC.frameCounter += 0.2f * (Math.Abs(NPC.velocity.X) + 1);
                        NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);
                    }

                }
                else
                {
                    NPC.frame.X = frameWidth * 5;
                    NPC.frame.Y = 0;
                }
                break;
            default:
                NPC.frame.X = frameWidth * 3;
                NPC.frame.Y = 0;
                break;
        }
        if (NPC.frame.Y < 0)
            NPC.frame.Y = 0;
    }

    public override bool? CanFallThroughPlatforms()
    {
        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0 || CurrentWaypoint >= pathFinding.MyPath.Points.Length)
            return false;
        int checkIndex = CurrentWaypoint + 4;
        if (checkIndex >= pathFinding.MyPath.Points.Length)
            checkIndex = pathFinding.MyPath.Points.Length - 1;

        return (pathFinding.MyPath.Points[checkIndex].Y > pathFinding.MyPath.Points[CurrentWaypoint].Y);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.frame.Width > 200)
            FindFrame(NPC.frame.Height);
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
        drawPosition.X += 12 * NPC.spriteDirection;
        drawPosition.Y += 4;
        if(tripTime != 0)
            drawPosition.X += 24 * -NPC.spriteDirection;

        Vector2 origin = NPC.frame.Size() * 0.5f;

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        
        spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
        
        return false;
    }
}

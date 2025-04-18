using Luminance.Common.VerletIntergration;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Common.Graphics.Verlet;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Common.Systems.WorldEvents;
public class DraconicRuinsSystem : ModSystem
{
    public static Point DraconicRuinsLocation = new(-1,-1);
    public static bool FacingLeft = false;

    public static Point RuinsEntrance => new(DraconicRuinsLocation.X + (FacingLeft ? -14 : 14), DraconicRuinsLocation.Y - 20);

    public static Point TabletRoom => new(DraconicRuinsLocation.X + (FacingLeft ? 9 : -9), DraconicRuinsLocation.Y + 46);

    public static Point TabletNeighbor => new(DraconicRuinsLocation.X + (FacingLeft ? -26 : 26), DraconicRuinsLocation.Y + 43);

    public static Point MiddleSideRoom => new(DraconicRuinsLocation.X + (FacingLeft ? -1 : 1), DraconicRuinsLocation.Y + 30);

    public static Point LeftSideRoom => new(DraconicRuinsLocation.X + (FacingLeft ? 29 : -29), DraconicRuinsLocation.Y + 18);

    public static Point UpperSideRoom => new(DraconicRuinsLocation.X + (FacingLeft ? 26 : -26), DraconicRuinsLocation.Y + 1);

    public static Rectangle DraconicRuinsArea => new(DraconicRuinsLocation.X - 36, DraconicRuinsLocation.Y - 38, 72, 88);

    public enum CutsceneState
    {
        Arrival,
        CultistFumble,
        PlayerFumble,
        CultistEnd,
        PlayerEnd,
        Finished
    }
    public static CutsceneState State = CutsceneState.Arrival;

    public static bool CutsceneActive = false;
    public static bool ZoomActive = false;

    public static bool AccessGranted = false;

    public static int CutsceneTime = 0;
    private static int CameraTime = 0;

    private static int BishopIndex = -1;

    public static VerletObject LeftChain = null;
    public static VerletObject RightChain = null;

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawTabletVerlets;
    }

    public override void ClearWorld()
    {
        DraconicRuinsLocation = new(-1, -1);
        FacingLeft = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["DraconicRuinsLocation"] = DraconicRuinsLocation;
        tag["FacingLeft"] = FacingLeft;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        DraconicRuinsLocation = tag.Get<Point>("DraconicRuinsLocation");
        FacingLeft = tag.GetBool("FacingLeft");
    }

    public override void PreUpdateWorld()
    {
        //Debug Dust
        /*
        Dust.NewDustPerfect(RuinsEntrance.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
        Dust.NewDustPerfect(TabletRoom.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
        Dust.NewDustPerfect(TabletNeighbor.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero);
        Dust.NewDustPerfect(MiddleSideRoom.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero);
        Dust.NewDustPerfect(LeftSideRoom.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero);
        Dust.NewDustPerfect(UpperSideRoom.ToWorldCoordinates(), DustID.LifeDrain, Vector2.Zero);

        Dust.NewDustPerfect(DraconicRuinsArea.TopLeft().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.Top().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.TopRight().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.Left().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.Right().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.BottomLeft().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.Bottom().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        Dust.NewDustPerfect(DraconicRuinsArea.BottomRight().ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);
        */

        //Reset State
        /*
        LeftChain = [];
        RightChain = [];
        State = CutsceneState.Arrival;
        ZoomActive = false;
        CutsceneActive = false;
        CutsceneTime = 0;
        CameraTime = 0;
        AccessGranted = false;
        */

        if (State == CutsceneState.Arrival && !NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()))
            NPC.NewNPC(Entity.GetSource_None(), TabletRoom.X * 16 + 8, TabletRoom.Y * 16 - 160, ModContent.NPCType<SealingTablet>());

        if (ZoomActive)
        {
            if (CameraTime < 60)
                CameraTime++;
            else
                CameraTime = 60;

            if (CameraTime > 0)
                switch (State)
                {
                    case CutsceneState.Arrival:
                        CameraPanSystem.PanTowards(RuinsEntrance.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CalamityMod.CalamityUtils.CircOutEasing(CameraTime / 60f, 1) / 2f;

                        NPC tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        int offset = 64;

                        switch(CutsceneTime)
                        {
                            case 60:
                                DisplayMessage("Wow...", tc, Color.Orange, 80);
                                break;
                            case 180:
                                DisplayMessage("I can't believe I'm really here.", tc, Color.Orange, 110);
                                break;
                            case 300:
                                DisplayMessage("These ruins...", tc, Color.Orange, 80);
                                break;
                            case 390:
                                DisplayMessage("I can't wait to learn their secrets!", tc, Color.Orange, 110);
                                break;
                            case 510:
                                DisplayMessage("Use the Wayfinder, and it should grant us access!", tc, Color.Orange, 110);
                                break;
                            case 569:
                                if(!AccessGranted)
                                    CutsceneTime--;
                                break;
                            case 690:
                                DisplayMessage("Wonderful!", tc, Color.Orange, 80);
                                break;
                            case 780:
                                DisplayMessage("Well then, let's head in-", tc, Color.Orange, 50);
                                break;
                            case 810:
                                Vector2 spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(84 + offset) : (84 + offset));
                                BishopIndex = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarBishop>());
                                break;
                            case 840:
                                DisplayMessage("What?!", tc, Color.Orange, 80);
                                break;
                            case 870:
                                DisplayMessage("Excellent work you two.", Main.npc[BishopIndex], Color.LimeGreen, 110);
                                break;
                            case 990:
                                DisplayMessage("We've had our eyes on this site for quite a while.", Main.npc[BishopIndex], Color.LimeGreen, 110);
                                break;
                            case 1110:
                                DisplayMessage("And you've done us the favor of opening it!", Main.npc[BishopIndex], Color.LimeGreen, 110);
                                break;
                            case 1230:
                                DisplayMessage("We won't let you enter!", tc, Color.Orange, 80);
                                break;
                            case 1350:
                                DisplayMessage("You're welcome to try.", Main.npc[BishopIndex], Color.LimeGreen, 80);
                                break;
                            case 1380:
                                spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(56 + offset) : (56 + offset)) - Vector2.UnitY * 16;
                                NPC cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                cultist.As<LunarCultistDevotee>().TargetPos = TabletRoom.ToWorldCoordinates();
                                break;
                            case 1410:
                                spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(112 + offset) : (112 + offset)) - Vector2.UnitY * 16;
                                cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                cultist.As<LunarCultistDevotee>().TargetPos = LeftSideRoom.ToWorldCoordinates();
                                break;
                            case 1440:
                                spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(12 + offset) : (12 + offset)) - Vector2.UnitY * 16;
                                cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                cultist.As<LunarCultistDevotee>().TargetPos = TabletRoom.ToWorldCoordinates();
                                break;
                            case 1470:
                                spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(156 + offset) : (156 + offset)) - Vector2.UnitY * 16;
                                cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                cultist.As<LunarCultistDevotee>().TargetPos = MiddleSideRoom.ToWorldCoordinates();
                                break;
                            case 1500:
                                spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(34 + offset) : (34 + offset)) - Vector2.UnitY * 16;
                                cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                cultist.As<LunarCultistDevotee>().TargetPos = UpperSideRoom.ToWorldCoordinates();
                                break;
                            case 1590:
                                DisplayMessage("But can you stop all of us?", Main.npc[BishopIndex], Color.LimeGreen, 80);
                                break;
                            case 1710:
                                DisplayMessage("Go! Claim everything for the cause!", Main.npc[BishopIndex], Color.LimeGreen, 110);
                                foreach(NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                                {
                                    npc.ai[2] = 5;
                                }
                                break;
                            case 1740:
                                DisplayMessage("Hurry!! We can't let them get the Tablet!", tc, Color.Orange, 110);
                                tc.As<TravellingCultist>().myBehavior = TravellingCultist.BehaviorState.FollowPlayer;
                                tc.As<TravellingCultist>().CanFly = false;
                                tc.As<TravellingCultist>().CanSpeedUp = false;
                                tc.As<TravellingCultist>().MoveSpeed = 5;
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.CultistFumble;
                                return;
                        }
                        break;
                    case CutsceneState.CultistFumble:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        switch (CutsceneTime)
                        {
                            case 240:
                                Main.npc[BishopIndex].Center = TabletRoom.ToWorldCoordinates() - Vector2.UnitY * 16;
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                    Dust dust = Dust.NewDustPerfect(Main.npc[BishopIndex].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[BishopIndex].Center);
                                break;
                            case 270:
                                DisplayMessage("Idiot!", Main.npc[BishopIndex], Color.LimeGreen, 30);
                                break;
                            case 330:
                                DisplayMessage("Ugh, no matter.", Main.npc[BishopIndex], Color.LimeGreen, 60);
                                break;
                            case 420:
                                DisplayMessage("Its better destroyed than in the hands of the enemy.", Main.npc[BishopIndex], Color.LimeGreen, 90);
                                break;
                            case 540:
                                DisplayMessage("We're leaving.", Main.npc[BishopIndex], Color.LimeGreen, 60);
                                break;
                            case 570:
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                    Dust dust = Dust.NewDustPerfect(Main.npc[BishopIndex].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[BishopIndex].Center);
                                Main.npc[BishopIndex].active = false;
                                break;
                            case 630:
                                foreach(NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                                {
                                    for (int i = 0; i <= 50; i++)
                                    {
                                        int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                        Dust dust = Dust.NewDustPerfect(npc.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                        dust.noGravity = true;
                                        dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                    }
                                    SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, npc.Center);
                                    npc.active = false;
                                }
                                break;
                            case 690:
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.CultistEnd;
                                break;
                        }                        
                        break;
                    case CutsceneState.PlayerFumble:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        switch (CutsceneTime)
                        {
                            case 240:
                                Main.npc[BishopIndex].Center = TabletRoom.ToWorldCoordinates() - Vector2.UnitY * 16;
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                    Dust dust = Dust.NewDustPerfect(Main.npc[BishopIndex].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[BishopIndex].Center);
                                break;
                            case 270:
                                DisplayMessage("What?!", Main.npc[BishopIndex], Color.LimeGreen, 30);
                                break;
                            case 330:
                                DisplayMessage("Trying to keep that relic out of our hands, are you?", Main.npc[BishopIndex], Color.LimeGreen, 60);
                                break;
                            case 420:
                                DisplayMessage("You've stopped nothing.", Main.npc[BishopIndex], Color.LimeGreen, 90);
                                break;
                            case 540:
                                DisplayMessage("Our plans shall still come to pass.", Main.npc[BishopIndex], Color.LimeGreen, 90);
                                break;
                            case 660:
                                DisplayMessage("But we'll be watching you...", Main.npc[BishopIndex], Color.LimeGreen, 60);
                                break;
                            case 690:
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                    Dust dust = Dust.NewDustPerfect(Main.npc[BishopIndex].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[BishopIndex].Center);
                                Main.npc[BishopIndex].active = false;
                                break;
                            case 750:
                                foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                                {
                                    for (int i = 0; i <= 50; i++)
                                    {
                                        int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                        Dust dust = Dust.NewDustPerfect(npc.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                        dust.noGravity = true;
                                        dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                    }
                                    SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, npc.Center);
                                    npc.active = false;
                                }
                                break;
                            case 810:
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.PlayerEnd;
                                break;
                        }
                        break;
                    case CutsceneState.CultistEnd:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        switch (CutsceneTime)
                        {
                            case 120:
                                DisplayMessage("Damn it...", tc, Color.Orange, 90);
                                break;
                            case 240:
                                DisplayMessage("This is certainly a setback.", tc, Color.Orange, 90);
                                break;
                            case 360:
                                DisplayMessage("But let's not lose hope.", tc, Color.Orange, 90);
                                break;
                            case 480:
                                DisplayMessage("I'm gonna have a look around.", tc, Color.Orange, 90);
                                break;
                            case 600:
                                DisplayMessage("Perhaps there's more here that can help us.", tc, Color.Orange, 90);
                                break;
                            case 720:
                                DisplayMessage("Let me know if you find anything.", tc, Color.Orange, 90);
                                break;
                            case 780:
                                tc.As<TravellingCultist>().myBehavior = TravellingCultist.BehaviorState.MoveToTargetLocation;
                                tc.As<TravellingCultist>().TargetLocation = TabletNeighbor.ToWorldCoordinates();
                                //tc.As<TravellingCultist>().CanSpeedUp = false;
                                //tc.As<TravellingCultist>().CanFly = false;
                                tc.As<TravellingCultist>().MoveSpeed = 3f;
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.Finished;
                                break;
                        }
                        break;
                    case CutsceneState.PlayerEnd:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        switch (CutsceneTime)
                        {
                            case 120:
                                DisplayMessage("Damn it...", tc, Color.Orange, 90);
                                break;
                            case 240:
                                DisplayMessage("This is certainly a setback.", tc, Color.Orange, 90);
                                break;
                            case 360:
                                DisplayMessage("Could you not have been more careful with that?!", tc, Color.Orange, 90);
                                break;
                            case 480:
                                DisplayMessage("Ugh, apologies.", tc, Color.Orange, 50);
                                break;
                            case 540:
                                DisplayMessage("It was so chaotic with those cultists...", tc, Color.Orange, 90);
                                break;
                            case 660:
                                DisplayMessage("I shouldn't be blaming you.", tc, Color.Orange, 90);
                                break;
                            case 780:
                                DisplayMessage("Let's not lose hope just yet.", tc, Color.Orange, 90);
                                break;
                            case 900:
                                DisplayMessage("I'm gonna have a look around.", tc, Color.Orange, 90);
                                break;
                            case 1020:
                                DisplayMessage("Perhaps there's more here that can help us.", tc, Color.Orange, 90);
                                break;
                            case 1140:
                                DisplayMessage("Let me know if you find anything.", tc, Color.Orange, 90);
                                break;
                            case 1260:
                                tc.As<TravellingCultist>().myBehavior = TravellingCultist.BehaviorState.MoveToTargetLocation;
                                tc.As<TravellingCultist>().TargetLocation = TabletNeighbor.ToWorldCoordinates();
                                //tc.As<TravellingCultist>().CanSpeedUp = false;
                                //tc.As<TravellingCultist>().CanFly = false;
                                tc.As<TravellingCultist>().MoveSpeed = 3f;
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.Finished;
                                break;
                        }
                        break;
                }
        }

        if (CutsceneActive)
            CutsceneTime++;

        if (LeftChain != null)
        {
            AffectVerletObject(LeftChain, 0.125f, 0.8f);
            VerletSimulation(LeftChain, 30, gravity: 0.5f, windAffected: false);
        }

        if (RightChain != null)
        {
            AffectVerletObject(RightChain, 0.125f, 0.8f);
            VerletSimulation(RightChain, 30, gravity: 0.5f, windAffected: false);
        }
    }

    private void DrawTabletVerlets(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);

        if (State != CutsceneState.Arrival && !NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()) && LeftChain != null && RightChain != null)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int k = 0; k < LeftChain.Count - 1; k++)
            {
                Vector2 line = LeftChain[k].Position - LeftChain[k + 1].Position;
                Color lighting = Lighting.GetColor((LeftChain[k + 1].Position + (line / 2f)).ToTileCoordinates());

                Main.spriteBatch.DrawLineBetween(LeftChain[k].Position, LeftChain[k + 1].Position, Color.White.MultiplyRGB(lighting), 3);
            }

            for (int k = 0; k < RightChain.Count - 1; k++)
            {
                Vector2 line = RightChain[k].Position - RightChain[k + 1].Position;
                Color lighting = Lighting.GetColor((RightChain[k + 1].Position + (line / 2f)).ToTileCoordinates());

                Main.spriteBatch.DrawLineBetween(RightChain[k].Position, RightChain[k + 1].Position, Color.White.MultiplyRGB(lighting), 3);
            }

            Main.spriteBatch.End();
        }
    }

    public static void StartCutscene()
    {
        ZoomActive = true;
        CutsceneActive = true;
        CutsceneTime = 0;
        CameraTime = 0;
    }

    private static void DisplayMessage(string key, NPC NPC, Color color, int upTime)
    {
        Rectangle location = new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.width);
        int index = CombatText.NewText(location, color, key, true);//GetWindfallTextValue($"Dialogue.{key}"), true);
        if (index == 100)
            return;
        CombatText MyDialogue = Main.combatText[index];
        MyDialogue.lifeTime = upTime;
        MyDialogue.velocity /= 1.5f;
    }
}

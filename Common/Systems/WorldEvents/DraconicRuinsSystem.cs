using DialogueHelper.UI.Dialogue;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Content.Buffs.Inhibitors;
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
        State = CutsceneState.Arrival;
        ZoomActive = false;
        CutsceneActive = false;
        CutsceneTime = 0;
        CameraTime = 0;
        AccessGranted = false;
        */

        //Main.NewText(CutsceneTime);

        if (DraconicRuinsLocation == new Point(-1, -1))
            return;

        if (State == CutsceneState.Arrival && !NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()))
            NPC.NewNPC(Entity.GetSource_None(), TabletRoom.X * 16 + 8, TabletRoom.Y * 16 - 160, ModContent.NPCType<SealingTablet>());

        if(State == CutsceneState.CultistFumble)
        {
            foreach(Player player in Main.ActivePlayers)
            {
                if (player.dead)
                    continue;

                Rectangle inflatedArea = DraconicRuinsArea;
                inflatedArea.X += 32;
                inflatedArea.Y += 32;
                inflatedArea.Width += 64;
                inflatedArea.Height += 64;
                if (inflatedArea.Contains(player.Center.ToTileCoordinates()))
                    player.AddBuff(ModContent.BuffType<SpacialLock>(), 2);
            }
        }

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
                        CameraPanSystem.PanTowards(RuinsEntrance.ToWorldCoordinates() + new Vector2(0, 48), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CalamityMod.CalamityUtils.CircOutEasing(CameraTime / 60f, 1) / 2f;

                        NPC tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        int offset = 64;
                        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();

                        if (!uiSystem.isDialogueOpen)
                        {
                            switch(CutsceneTime)
                            {
                                case 60:
                                    uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/Arrival", new(Name, [tc.whoAmI]));
                                    CutsceneTime++;
                                    break;
                                case 120:
                                    uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/Arrival", new(Name, [tc.whoAmI]), 2);
                                    CutsceneTime++;
                                    break;
                                case 150:
                                    Vector2 spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(84 + offset) : (84 + offset));
                                    BishopIndex = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarBishop>());
                                    break;
                                case 180:
                                    uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/Arrival", new(Name, [tc.whoAmI]), 3);
                                    CutsceneTime++;
                                    break;
                                case 210:
                                    spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(56 + offset) : (56 + offset)) - Vector2.UnitY * 16;
                                    NPC cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                    cultist.As<LunarCultistDevotee>().TargetPos = TabletRoom.ToWorldCoordinates();
                                    if (!FacingLeft)
                                        cultist.direction *= -1;
                                    break;
                                case 240:
                                    spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(112 + offset) : (112 + offset)) - Vector2.UnitY * 16;
                                    cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                    cultist.As<LunarCultistDevotee>().TargetPos = LeftSideRoom.ToWorldCoordinates();
                                    if (!FacingLeft)
                                        cultist.direction *= -1;
                                    break;
                                case 270:
                                    spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(12 + offset) : (12 + offset)) - Vector2.UnitY * 16;
                                    cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                    cultist.As<LunarCultistDevotee>().TargetPos = TabletRoom.ToWorldCoordinates();
                                    if (!FacingLeft)
                                        cultist.direction *= -1;
                                    break;
                                case 300:
                                    spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(156 + offset) : (156 + offset)) - Vector2.UnitY * 16;
                                    cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                    cultist.As<LunarCultistDevotee>().TargetPos = MiddleSideRoom.ToWorldCoordinates();
                                    if (!FacingLeft)
                                        cultist.direction *= -1;
                                    break;
                                case 330:
                                    spawnPos = RuinsEntrance.ToWorldCoordinates() + Vector2.UnitX * (FacingLeft ? -(34 + offset) : (34 + offset)) - Vector2.UnitY * 16;
                                    cultist = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 7);
                                    cultist.As<LunarCultistDevotee>().TargetPos = UpperSideRoom.ToWorldCoordinates();
                                    if (!FacingLeft)
                                        cultist.direction *= -1;
                                    break;
                                case 490:
                                    DisplayMessage("Go! Claim everything for the cause!", Main.npc[BishopIndex], Color.LimeGreen, 110);
                                    foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                                        npc.ai[2] = 5;
                                    break;
                                case 520:
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
                        }

                        if (CutsceneTime < 60 || !uiSystem.isDialogueOpen)
                            CutsceneTime++;

                        if (CutsceneTime > 90 && !AccessGranted)
                            CutsceneTime--;
                        break;
                    case CutsceneState.CultistFumble:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, -20), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        uiSystem = ModContent.GetInstance<DialogueUISystem>();

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
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/CultistFumble", new(Name, [BishopIndex]));
                                CutsceneTime++;
                                break;
                            case 300:
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
                            case 360:
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
                            case 420:
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.CultistEnd;
                                break;
                        }

                        if (CutsceneTime < 270 || !uiSystem.isDialogueOpen)
                            CutsceneTime++;
                        break;
                    case CutsceneState.PlayerFumble:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, -20), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        uiSystem = ModContent.GetInstance<DialogueUISystem>();

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
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/PlayerFumble", new(Name, [BishopIndex]));
                                CutsceneTime++;
                                break;
                            case 300:
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
                            case 360:
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
                            case 420:
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.PlayerEnd;
                                break;
                        }

                        if (CutsceneTime < 270 || !uiSystem.isDialogueOpen)
                            CutsceneTime++;
                        break;
                    case CutsceneState.CultistEnd:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, -20), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        uiSystem = ModContent.GetInstance<DialogueUISystem>();

                        tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        if (CutsceneTime == 30)
                            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/CultistEnd", new(Name, [tc.whoAmI]));
                        else if (CutsceneTime > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                        {
                            tc.As<TravellingCultist>().myBehavior = TravellingCultist.BehaviorState.MoveToTargetLocation;
                            tc.As<TravellingCultist>().TargetLocation = TabletNeighbor.ToWorldCoordinates();
                            tc.As<TravellingCultist>().MoveSpeed = 3f;
                            ZoomActive = false;
                            CutsceneActive = false;
                            CutsceneTime = 0;
                            CameraTime = 0;
                            State = CutsceneState.Finished;
                        }

                        if (CutsceneTime <= 30 || !uiSystem.isDialogueOpen)
                            CutsceneTime++;
                        
                        break;
                    case CutsceneState.PlayerEnd:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, -20), CircOutEasing(CameraTime / 60f));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f) / 2f;

                        uiSystem = ModContent.GetInstance<DialogueUISystem>();

                        tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        if (CutsceneTime == 30)
                            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/PlayerEnd", new(Name, [tc.whoAmI]));
                        else if (CutsceneTime > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                        {
                            tc.As<TravellingCultist>().myBehavior = TravellingCultist.BehaviorState.MoveToTargetLocation;
                            tc.As<TravellingCultist>().TargetLocation = TabletNeighbor.ToWorldCoordinates();
                            tc.As<TravellingCultist>().MoveSpeed = 3f;
                            ZoomActive = false;
                            CutsceneActive = false;
                            CutsceneTime = 0;
                            CameraTime = 0;
                            State = CutsceneState.Finished;
                        }

                        if (CutsceneTime <= 30 || !uiSystem.isDialogueOpen)
                            CutsceneTime++;
                        break;
                }
        }            

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

using CalamityMod.World;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Utils;
using Windfall.Content.Items.Food;
using Windfall.Content.Items.Quests;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.NPCs.Critters;
using Windfall.Content.Buffs.DoT;
using Windfall.Content.UI;
using Terraria.Enums;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Quests.Cafeteria;
using Windfall.Content.Projectiles.NPCAnimations;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
using Windfall.Content.Items.Quests.SealingRitual;
using Windfall.Content.Buffs.Inhibitors;
using static Windfall.Content.NPCs.WorldEvents.LunarCult.RecruitableLunarCultist;
using Terraria;

namespace Windfall.Common.Systems.WorldEvents;

public class LunarCultBaseSystem : ModSystem
{
    #region Static Variables
    public static Point LunarCultBaseLocation;

    public static Rectangle CultBaseTileArea;

    public static Rectangle CultBaseWorldArea;

    public static Rectangle CultBaseBridgeArea;

    public static bool BaseFacingLeft = false;

    public static List<int> Recruits = [];

    private static List<int> AvailableTopics = [];

    public static bool TutorialComplete = false;

    public static int RecruitmentsSkipped = 0;

    public static bool BetrayalActive = false;

    public static bool FinalMeetingSeen = false;

    private static int spawnChance = 5;

    public static readonly Dictionary<string, Asset<Texture2D>> SkeletonAssets = [];

    public static readonly Dictionary<string, List<VerletObject>> SkeletonVerletGroups = [];

    public static bool DraconicBoneSequenceActive = false;

    public static int DraconicBoneTimer = 0;
    #endregion

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawHangingSkeleton;

        if (!Main.dedServ)
        {
            SkeletonAssets.Add("BackWing", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonBackWing", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("BackWing", []);

            SkeletonAssets.Add("BackLeg", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonBackLeg", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("BackLeg", []);

            SkeletonAssets.Add("Tail", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonTail", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("Tail", []);

            SkeletonAssets.Add("Ribs", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonRibs", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("Ribs", []);

            SkeletonAssets.Add("Skull", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonSkull", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("Skull", []);
            
            SkeletonAssets.Add("FrontWing", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonFrontWing", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("FrontWing", []);

            SkeletonAssets.Add("FrontLeg", ModContent.Request<Texture2D>("Windfall/Assets/NPCs/DragonSkeleton/DragonFrontLeg", AssetRequestMode.AsyncLoad));
            SkeletonVerletGroups.Add("FrontLeg", []);
        }
    }

    public override void ClearWorld()
    {
        LunarCultBaseLocation = new(-1, -1);

        CultBaseTileArea = new(-1, -1, 1, 1);

        CultBaseBridgeArea = new(-1, -1, 1, 1);

        Recruits = [];

        AvailableTopics = [];

        CustomerQueue = [];

        TutorialComplete = false;

        RecruitmentsSkipped = 0;

        BetrayalActive = false;

        spawnChance = 5;

        Active = false;
        State = SystemStates.CheckReqs;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        LunarCultBaseLocation = tag.Get<Point>("LunarCultBaseLocation");

        BaseFacingLeft = tag.GetBool("BaseFacingLeft");

        CultBaseTileArea = new(LunarCultBaseLocation.X - (BaseFacingLeft ? 124 : 0), LunarCultBaseLocation.Y - 74, 124, 148);

        CultBaseWorldArea = new(CultBaseTileArea.X * 16, CultBaseTileArea.Y * 16, CultBaseTileArea.Width * 16, CultBaseTileArea.Height * 16);

        CultBaseBridgeArea = new(BaseFacingLeft ? CultBaseTileArea.Left - 90 : CultBaseTileArea.Right, CultBaseTileArea.Top + 4, 90, CultBaseTileArea.Height - 54);

        Recruits = (List<int>)tag.GetList<int>("Recruits");

        AvailableTopics = (List<int>)tag.GetList<int>("AvailableTopics");

        TutorialComplete = tag.GetBool("TutorialComplete");

        RecruitmentsSkipped = tag.GetInt("RecruitmentsSkipped");

        BetrayalActive = tag.GetBool("BetrayalActive");

        spawnChance = tag.GetInt("spawnChance");

        FinalMeetingSeen = tag.GetBool("FinalActivitySeen");
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["LunarCultBaseLocation"] = LunarCultBaseLocation;

        tag["BaseFacingLeft"] = BaseFacingLeft;

        tag["Recruits"] = Recruits;

        tag["AvailableTopics"] = AvailableTopics;

        tag["TutorialComplete"] = TutorialComplete;

        tag["RecruitmentsSkipped"] = RecruitmentsSkipped;

        tag["BetrayalActive"] = BetrayalActive;

        tag["spawnChance"] = spawnChance;

        tag["FinalActivitySeen"] = FinalMeetingSeen;
    }

    public enum SystemStates
    {
        CheckReqs,
        CheckChance,
        Waiting,
        Yap,
        Ready,
        OratorVisit,
        Meeting,
        Tailor,
        Cafeteria,
        Ritual,
        FinalMeeting,
        End,
    }
    public static SystemStates State = SystemStates.CheckReqs;
    public static SystemStates PlannedActivity = SystemStates.End;

    private static bool OnCooldown = true;
    public static bool Active = false;
    private static int ActivityTimer = -1;
    public static Point ActivityCoords = new(-1, -1);
    private static List<int> NPCIndexs = [];
    private static float zoom = 0;        

    #region Meeting Variables
    public enum MeetingTopic
    {
        None,
        CurrentEvents,
        //DragonCult,
        Goals
    }
    public static MeetingTopic CurrentMeetingTopic = MeetingTopic.None;
    #endregion
    #region Tailor Variables
    public static int[] AssignedClothing = new int[255];
    public static int CompletedClothesCount = 0;
    public static int ClothesGoal = 3;
    #endregion
    #region Cafeteria Variables
    public struct Customer(NPC npc, int orderIDs)
    { 
        public NPC NPC = npc;
        public int OrderID = orderIDs;
    }
    public static List<Customer?> CustomerQueue = [];
    public static readonly List<int> FoodIDs =
    [
        ModContent.ItemType<AbyssalInkPasta>(),
        ModContent.ItemType<AzafurianPallela>(),
        ModContent.ItemType<EbonianCheddarBoard>(),
        ModContent.ItemType<EutrophicClamChowder>(),
        ModContent.ItemType<FriedToxicatfishSandwich>(),
        ModContent.ItemType<GlimmeringNigiri>(),
        ModContent.ItemType<LemonButterHermititanLegs>(),
    ];
    public static List<int> MenuFoodIDs = [];
    public static int SatisfiedCustomers = 0;
    public static int CustomerGoal = 8;
    public static readonly int CustomerLimit = 12;
    public static int AtMaxTimer = 0;
    #endregion
    #region Ritual Variables
    public static int RemainingCultists = 6;
    public static int ActivePortals = 0;
    public static int PortalsDowned = 0;
    public static int RequiredPortalKills = 10;
    #endregion

    private static SoundStyle TeleportSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void OnWorldLoad()
    {
        ActivityTimer = -1;           
    }
    public override void PreUpdateWorld()
    {
        if (NPC.downedAncientCultist || LunarCultBaseLocation == new Point(-1, -1))
            return;

        if (NPC.downedPlantBoss)
        {
            #region Main Character Spawning
            if (!NPC.AnyNPCs(ModContent.NPCType<Seamstress>()))
                NPC.NewNPC(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1400 : 1400), (LunarCultBaseLocation.Y * 16) + 480, ModContent.NPCType<Seamstress>());
            if (!NPC.AnyNPCs(ModContent.NPCType<TheChef>()))
                NPC.NewNPC(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -168 : 168), (LunarCultBaseLocation.Y * 16), ModContent.NPCType<TheChef>());
            if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<Watchman>()))
                NPC.NewNPC(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -2100 : 2100), (LunarCultBaseLocation.Y - 6) * 16 - 5, ModContent.NPCType<Watchman>());
            if (SealingRitualSystem.RitualSequenceSeen)
                return;

            int oratorType = ModContent.NPCType<OratorNPC>();
            if (QuestSystem.Quests["DraconicBone"].Active)
            {
                if (!DraconicBoneSequenceActive && NPC.AnyNPCs(oratorType))
                    foreach (NPC orator in Main.npc.Where(n => n.active && n.type == oratorType))
                        orator.active = false;
            }
            else if (!NPC.AnyNPCs(oratorType))
                NPC.NewNPC(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1858 : 1858), (CultBaseTileArea.Top + 30) * 16, oratorType);

            #endregion

            //Main.NewText(LunarCultBaseLocation.ToWorldCoordinates() - Main.LocalPlayer.Center);

            Player closestPlayer = Main.player[Player.FindClosest(CultBaseWorldArea.TopLeft(), CultBaseWorldArea.Width, CultBaseWorldArea.Height)];
            float PlayerDistFromHideout = (closestPlayer.Center - CultBaseWorldArea.Center()).Length();
            if (PlayerDistFromHideout < 1600 && Main.npc.Where(n => n.active && n.type == ModContent.NPCType<Fingerling>()).Count() < 16)
                SpawnFingerling();

            #region Nearby Enemy Murdering
            foreach (NPC npc in Main.npc.Where(n => n.active))
            {
                if (!npc.dontTakeDamage && npc.lifeMax != 1 && !npc.friendly && !npc.boss && npc.type != ModContent.NPCType<PortalMole>())
                {
                    Rectangle inflatedArea = new(CultBaseWorldArea.X - 512, CultBaseWorldArea.Y + 512, CultBaseWorldArea.Width + 1024, CultBaseWorldArea.Height + 1024);
                    Rectangle inflatedBridge = new(CultBaseBridgeArea.X * 16 - 512, CultBaseBridgeArea.Y * 16 + 512, CultBaseBridgeArea.Width * 16 + 1024, CultBaseBridgeArea.Height * 16 + 1024);
                    if (inflatedArea.Contains((int)npc.Center.X, (int)npc.Center.Y) || inflatedBridge.Contains((int)npc.Center.X, (int)npc.Center.Y))
                        npc.AddBuff(ModContent.BuffType<Entropy>(), 2);
                }                    
            }
            #endregion
        }
        if (Main.player.Any(p => p.active && !p.dead && CultBaseTileArea.Contains(p.Center.ToTileCoordinates())))
            CalamityWorld.ArmoredDiggerSpawnCooldown = 36000;

        bool spawnApostle = false;

        foreach (Player player in Main.ActivePlayers)
        {
            if (QuestSystem.Quests["DraconicBone"].Complete && !DownedNPCSystem.downedOrator)
            {
                Rectangle inflatedArea = new(CultBaseWorldArea.X - 512, CultBaseWorldArea.Y + 512, CultBaseWorldArea.Width + 1024, CultBaseWorldArea.Height + 1024);
                Rectangle inflatedBridge = new(CultBaseBridgeArea.X * 16 - 512, CultBaseBridgeArea.Y * 16 + 512, CultBaseBridgeArea.Width * 16 + 1024, CultBaseBridgeArea.Height * 16 + 1024);
                if (inflatedArea.Contains((int)player.Center.X, (int)player.Center.Y) || inflatedBridge.Contains((int)player.Center.X, (int)player.Center.Y))
                    player.AddBuff(ModContent.BuffType<Entropy>(), 2);
            }

            if (!player.dead && CultBaseTileArea.Contains(player.Center.ToTileCoordinates()))
            {
                #region Basement Teleport
                if (!QuestSystem.Quests["DraconicBone"].Active && player.Center.Y > (LunarCultBaseLocation.Y + 30) * 16)
                {
                    for (int i = 0; i <= 20; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(player.Center, Main.rand.NextVector2Circular(5f, 5f), 30 * Main.rand.NextFloat(1.5f, 2.3f));
                    player.Teleport(new Vector2((LunarCultBaseLocation.X - 106) * 16, (CultBaseTileArea.Top + 27) * 16), TeleportationStyleID.DebugTeleport);
                    SoundEngine.PlaySound(SoundID.Item8, player.Center);
                    for (int i = 0; i <= 20; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(player.Center, Main.rand.NextVector2Circular(5f, 5f), 30 * Main.rand.NextFloat(1.5f, 2.3f));
                    if (NPC.AnyNPCs(ModContent.NPCType<OratorNPC>()))
                    {
                        NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];
                        string path = "Dialogue.LunarCult.TheOrator.WorldText.Basement." + Main.rand.Next(3);
                        DisplayMessage(orator.Hitbox, Color.LimeGreen, path);
                    }
                }
                #endregion

                player.AddBuff(ModContent.BuffType<SpacialLock>(), 2);
            }

            if (player.LunarCult().apostleQuestTracker == 11)
                spawnApostle = true;
        }

        if(DraconicBoneSequenceActive)
            DraconicBoneSequence();

        if (FinalMeetingSeen)
            return;

        if (spawnApostle && !NPC.AnyNPCs(ModContent.NPCType<DisgracedApostleNPC>()))
            NPC.NewNPC(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1800 : 1800), (CultBaseTileArea.Top + 30) * 16, ModContent.NPCType<DisgracedApostleNPC>());

        //for testing
        //State = SystemState.CheckReqs;
        //ActivityTimer = -1;
        //TutorialComplete = true;
        //Main.NewText(State);

        switch (State)
        {
            case SystemStates.CheckReqs:
                if (!NPC.downedPlantBoss || Recruits.Count == 4 || OnCooldown || Main.dayTime || AnyBossNPCS(true))
                {
                    if (Main.dayTime)
                    {
                        OnCooldown = false;
                        Active = false;
                        ActivityCoords = new(-1, -1);
                        ActivityTimer = -1;
                    }
                    break;
                }
                else
                {
                    State = SystemStates.CheckChance;
                    break;
                }
            case SystemStates.CheckChance:
                if (spawnChance < 1)
                    spawnChance = 1;
                if (Main.rand.NextBool(spawnChance))
                {
                    State = SystemStates.Waiting;

                    #region One Time Initializations
                    spawnChance = 5;

                    CurrentMeetingTopic = MeetingTopic.None;

                    Main.npc.First(n => n.active && n.type == ModContent.NPCType<Watchman>()).ai[2] = 0;
                    #endregion

                    #region Assumed upcoming Activity
                    if (!TutorialComplete || RecruitmentsSkipped >= 3) //Orator Visit\
                        PlannedActivity = SystemStates.OratorVisit;
                    else if (QuestSystem.Quests["Recruitment"].Complete && !FinalMeetingSeen)
                        PlannedActivity = SystemStates.FinalMeeting;
                    else
                    {
                        switch ((MoonPhase)Main.moonPhase)
                        {
                            case MoonPhase.Full:
                            case MoonPhase.Empty: //Ritual
                                PlannedActivity = SystemStates.Ritual;
                                break;
                            case MoonPhase.QuarterAtLeft:
                            case MoonPhase.QuarterAtRight: //Meeting
                                PlannedActivity = SystemStates.Meeting;
                                break;
                            case MoonPhase.HalfAtLeft:
                            case MoonPhase.HalfAtRight: //Tailor
                                PlannedActivity = SystemStates.Tailor;
                                break;
                            case MoonPhase.ThreeQuartersAtLeft:
                            case MoonPhase.ThreeQuartersAtRight: //Cafeteria
                                PlannedActivity = SystemStates.Cafeteria;
                                break;
                        }
                    }
                    #endregion
                    
                    #region Activity Alert
                    foreach (Player player in Main.player)
                    {
                        if (player.InventoryHas(ModContent.ItemType<SelenicTablet>()))
                        {
                            Main.NewText("The Selenic Tablet begins to hum...", Color.Cyan);
                            break;
                        }
                    }
                    #endregion

                    ActivityTimer = -1;
                    return;
                }
                else
                {
                    if(spawnChance > 1)
                        spawnChance--;
                    OnCooldown = true;
                    State = SystemStates.CheckReqs;
                }
                break;
            case SystemStates.Waiting:
                if (ActivityTimer == -1)
                {
                    #region Static Character Setup
                    foreach (NPC cultist in Main.npc.Where(n => n.active && (n.type == ModContent.NPCType<LunarCultistDevotee>() || n.type == ModContent.NPCType<LunarCultistArcher>() || n.type == ModContent.NPCType<LunarBishop>())))
                    {
                        if (cultist.ai[2] == 4) //removes all StaticCharacter cultists
                            cultist.active = false;
                    }
                    switch(PlannedActivity)
                    {
                        case SystemStates.Ritual:
                            NPC warn = NPC.NewNPCDirect(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1790 : 1790), (LunarCultBaseLocation.Y - 24) * 16, ModContent.NPCType<LunarCultistArcher>(), ai2: 4);
                            warn.As<LunarCultistArcher>().myCharacter = LunarCultistArcher.Character.RitualWarn;
                            break;
                        case SystemStates.Meeting:               
                            break;
                        case SystemStates.Tailor:
                            break;
                        case SystemStates.Cafeteria:
                            break;
                        case SystemStates.OratorVisit:
                            break;
                    }
                    if(PlannedActivity != SystemStates.Meeting)
                    {
                        NPC speaker = NPC.NewNPCDirect(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -816 : 816), (LunarCultBaseLocation.Y + 24) * 16, ModContent.NPCType<LunarBishop>(), ai2: 4);
                        speaker.As<LunarBishop>().myCharacter = LunarBishop.Character.Speaker;
                        speaker.spriteDirection *= -1;

                        NPC eeper = NPC.NewNPCDirect(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -208 : 208), (LunarCultBaseLocation.Y + 24) * 16, ModContent.NPCType<LunarCultistDevotee>(), ai2: 4);
                        eeper.As<LunarCultistDevotee>().myCharacter = LunarCultistDevotee.Character.Eeper;
                        eeper.spriteDirection *= -1;
                    }
                    if (PlannedActivity != SystemStates.Cafeteria)
                    {
                        Vector2 chefCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center;
                        NPC foodie = NPC.NewNPCDirect(Entity.GetSource_None(), (int)chefCenter.X + 280, (int)chefCenter.Y, ModContent.NPCType<LunarBishop>(), ai2: 4);
                        foodie.As<LunarBishop>().myCharacter = LunarBishop.Character.Foodie;
                        foodie.spriteDirection *= -1;
                    }
                    if (PlannedActivity != SystemStates.Tailor)
                    {
                        Vector2 seamstressCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Seamstress>())].Center;
                        NPC dripped = NPC.NewNPCDirect(Entity.GetSource_None(), (int)seamstressCenter.X - 400, (int)seamstressCenter.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 4);
                        dripped.As<LunarCultistDevotee>().myCharacter = LunarCultistDevotee.Character.NewClothes;
                    }
                    if (PlannedActivity != SystemStates.OratorVisit)
                    {
                        NPC broke = NPC.NewNPCDirect(Entity.GetSource_None(), LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1258 : 1258), (LunarCultBaseLocation.Y - 46) * 16, ModContent.NPCType<LunarCultistArcher>(), ai2: 4);
                        broke.As<LunarCultistArcher>().myCharacter = LunarCultistArcher.Character.Broke;
                    }
                    #endregion
                    
                    ActivityTimer = 0;
                }

                #region Player Proximity
                Vector2 entranceArea = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Watchman>())].Center;
                Player closestPlayer = Main.player[Player.FindClosest(entranceArea, 1, 1)];
                float PlayerDistFromHideout = (entranceArea - closestPlayer.Center).Length();
                if (PlayerDistFromHideout < 160f && closestPlayer.Center.Y < entranceArea.Y + 16)
                    State = SystemStates.Yap;
                #endregion

                #region Moon Phase Change Check
                if (PlannedActivity != SystemStates.OratorVisit && PlannedActivity != SystemStates.FinalMeeting)
                {
                    switch ((MoonPhase)Main.moonPhase)
                    {
                        case MoonPhase.Full: case MoonPhase.Empty: //Ritual
                            if (PlannedActivity != SystemStates.Ritual)
                            {
                                PlannedActivity = SystemStates.Ritual;
                                ActivityTimer = -1;
                            }
                            break;
                        case MoonPhase.QuarterAtLeft: case MoonPhase.QuarterAtRight: //Meeting
                            if (PlannedActivity != SystemStates.Meeting)
                            {
                                PlannedActivity = SystemStates.Meeting;
                                ActivityTimer = -1;
                            }
                            break;
                        case MoonPhase.HalfAtLeft: case MoonPhase.HalfAtRight: //Tailor
                            if (PlannedActivity != SystemStates.Tailor)
                            {
                                PlannedActivity = SystemStates.Tailor;
                                ActivityTimer = -1;
                            }
                            break;
                        case MoonPhase.ThreeQuartersAtLeft: case MoonPhase.ThreeQuartersAtRight: //Cafeteria
                            if (PlannedActivity != SystemStates.Cafeteria)
                            {
                                PlannedActivity = SystemStates.Cafeteria;
                                ActivityTimer = -1;
                            }
                            break;
                    }
                }
                #endregion

                #region Despawn
                if (Main.dayTime)
                {
                    ActivityCoords = new(-1, -1);
                    State = SystemStates.CheckReqs;
                    OnCooldown = true;
                }
                #endregion
                break;
            case SystemStates.Yap:
                NPC watchman = Main.npc.First(n => n.active && n.type == ModContent.NPCType<Watchman>());
                string Activity = !TutorialComplete ? "First" : PlannedActivity.ToString();

                if(ActivityTimer == 30)
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"Watchman/Greetings/{Activity}", new(Name, [watchman.whoAmI]));
                else if(ActivityTimer > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                {
                    watchman.ai[2]++;
                    State = SystemStates.Ready;
                }

                #region Camera Setup
                entranceArea = watchman.Center;
                if (ActivityTimer < 100)
                    zoom = Lerp(zoom, 0.4f, ActivityTimer / 100f);
                else
                    zoom = 0.4f;
                CameraPanSystem.Zoom = zoom;
                CameraPanSystem.PanTowards(new Vector2(entranceArea.X, entranceArea.Y + 60), zoom);
                #endregion

                ActivityTimer++;
                if (State == SystemStates.Ready)
                {
                    ActivityTimer = -1;
                    return;
                }
                break;
            case SystemStates.Ready:
                if (ActivityTimer == -1)
                {
                    #region Activity Specific Setup
                    switch (PlannedActivity)
                    {
                        case SystemStates.Ritual:
                            #region Location Selection
                            ActivityCoords = new Point(LunarCultBaseLocation.X + (BaseFacingLeft ? -32 : 32), LunarCultBaseLocation.Y - 24);
                            ActivityCoords.X *= 16;
                            //ActivityCoords.X += 8;
                            ActivityCoords.Y *= 16;
                            #endregion

                            RemainingCultists = 6;
                            ActivePortals = 0;
                            PortalsDowned = 0;
                            NPCIndexs = [];

                            #region Character Setup   
                            if (NPC.AnyNPCs(ModContent.NPCType<OratorNPC>()))
                                foreach (NPC npc in Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<OratorNPC>()))
                                {
                                    npc.active = false;
                                }
                            NPCIndexs =
                            [
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 364, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 252, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 108, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 108, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 252, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 364, ActivityCoords.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 3),
                                    NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 36, ActivityCoords.Y, ModContent.NPCType<OratorNPC>(), ai0: 2),
                                ];

                            Main.npc[NPCIndexs[0]].spriteDirection = 1;
                            Main.npc[NPCIndexs[0]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X - 364, ActivityCoords.Y);

                            Main.npc[NPCIndexs[1]].spriteDirection = 1;
                            Main.npc[NPCIndexs[1]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X - 252, ActivityCoords.Y);

                            Main.npc[NPCIndexs[2]].spriteDirection = 1;
                            Main.npc[NPCIndexs[2]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X - 108, ActivityCoords.Y);

                            Main.npc[NPCIndexs[3]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X + 108, ActivityCoords.Y);

                            Main.npc[NPCIndexs[4]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X + 252, ActivityCoords.Y);

                            Main.npc[NPCIndexs[5]].As<LunarCultistDevotee>().goalPosition = new(ActivityCoords.X + 364, ActivityCoords.Y);

                            #endregion
                            break;
                        case SystemStates.Meeting:
                            #region Location Selection
                            ActivityCoords = new Point(LunarCultBaseLocation.X + (BaseFacingLeft ? -25 : 23), LunarCultBaseLocation.Y + 18);
                            ActivityCoords.X *= 16;
                            ActivityCoords.X += BaseFacingLeft ? -8 : 8;
                            ActivityCoords.Y *= 16;
                            #endregion

                            #region Topic Selection
                            if (AvailableTopics.Count == 0)
                                for (int h = 0; h < Enum.GetNames(typeof(MeetingTopic)).Length; h++)
                                    AvailableTopics.Add(h);

                            CurrentMeetingTopic = (MeetingTopic)AvailableTopics[Main.rand.Next(AvailableTopics.Count)];
                            AvailableTopics.Remove((int)CurrentMeetingTopic);
                            #endregion

                            #region Character Setup   
                            NPCIndexs =
                            [
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X, ActivityCoords.Y - 2, ModContent.NPCType<LunarBishop>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 280, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 138, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 138, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 280, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            ];
                            int y = 0;
                            foreach (int k in NPCIndexs)
                            {
                                NPC npc = Main.npc[k];
                                if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                                {
                                    switch (CurrentMeetingTopic)
                                    {
                                        case MeetingTopic.CurrentEvents:
                                            switch (y)
                                            {
                                                case 1:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tirith;
                                                    Recruit.canRecruit = true;
                                                    break;
                                                case 2:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                    Recruit.canRecruit = false;
                                                    break;
                                                case 3:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                    Recruit.canRecruit = true;
                                                    break;
                                                case 4:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                    Recruit.canRecruit = false;
                                                    break;
                                            }
                                            break;
                                        case MeetingTopic.Goals:
                                            switch (y)
                                            {
                                                case 1:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                    Recruit.canRecruit = true;
                                                    break;
                                                case 2:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                    Recruit.canRecruit = true;
                                                    break;
                                                case 3:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                    Recruit.canRecruit = false;
                                                    break;
                                                case 4:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tirith;
                                                    Recruit.canRecruit = false;
                                                    break;
                                            }
                                            break;
                                        default:
                                        //case MeetingTopic.DragonCult:
                                            switch (y)
                                            {
                                                case 1:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                    Recruit.canRecruit = false; // Accepts the idea that the Dragon Cult is no good pretty easilly
                                                    break;
                                                case 2:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                    Recruit.canRecruit = true; // Is shocked by how genuine the hate for the Dragon Cult feels
                                                    break;
                                                case 3:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                    Recruit.canRecruit = false; // Already dislikes the Dragon Cult for their association with Yharim
                                                    break;
                                                case 4:
                                                    Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                    Recruit.canRecruit = true; // Is upset of the Order's disregard for the historical significant of Dragons and the history held by the Dragon Cult
                                                    break;
                                            }
                                            break;
                                    }
                                    Recruit.OnSpawn(NPC.GetSource_NaturalSpawn());
                                }
                                y++;
                            }
                            #endregion
                            break;
                        case SystemStates.Tailor:
                            #region Location Selection
                            Vector2 seamstressCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Seamstress>())].Center;
                            ActivityCoords = new((int)seamstressCenter.X, (int)seamstressCenter.Y);
                            #endregion

                            AssignedClothing = new int[255];
                            CompletedClothesCount = 0;
                            ClothesGoal = 5 * (Main.player.Where(p => p.active).Count() + 1);
                            break;
                        case SystemStates.Cafeteria:
                            Vector2 chefCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center;
                            ActivityCoords = new((int)chefCenter.X, (int)chefCenter.Y);

                            MenuFoodIDs = FoodIDs;
                            while (MenuFoodIDs.Count > 5)
                                MenuFoodIDs.RemoveAt(Main.rand.Next(MenuFoodIDs.Count));

                            CustomerQueue = [];
                            SatisfiedCustomers = 0;
                            break;
                        case SystemStates.OratorVisit:
                            ActivityCoords = new Point(LunarCultBaseLocation.X * 16 + (BaseFacingLeft ? -1858 : 1858), (CultBaseTileArea.Top + 30) * 16);

                            if (!NPC.AnyNPCs(ModContent.NPCType<OratorNPC>()))
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X, ActivityCoords.Y, ModContent.NPCType<OratorNPC>(), ai0: 1);
                            else
                                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())].ai[0] = 1;
                            State = SystemStates.OratorVisit;
                            Active = true;
                            break;
                        case SystemStates.FinalMeeting:
                            #region Location Selection
                            ActivityCoords = new Point(LunarCultBaseLocation.X + (BaseFacingLeft ? -25 : 23), LunarCultBaseLocation.Y + 18);
                            ActivityCoords.X *= 16;
                            ActivityCoords.X += BaseFacingLeft ? -8 : 8;
                            ActivityCoords.Y *= 16;
                            #endregion

                            NPCIndexs =
                            [
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X, ActivityCoords.Y - 2, ModContent.NPCType<OratorNPC>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 280, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X - 138, ActivityCoords.Y + 100, ModContent.NPCType<LunarCultistDevotee>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 138, ActivityCoords.Y + 100, ModContent.NPCType<LunarCultistDevotee>()),
                                NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X + 280, ActivityCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            ];

                            List<int> recruits = Recruits;
                            for (int i = 0; i < 6; i++)
                            {
                                if (recruits.Contains(i))
                                    continue;
                                recruits.Add(i);
                                Main.npc[NPCIndexs[1]].As<RecruitableLunarCultist>().MyName = (RecruitNames)i;
                                break;
                            }
                            for (int i = 0; i < 6; i++)
                            {
                                if (recruits.Contains(i))
                                    continue;
                                recruits.Add(i);
                                Main.npc[NPCIndexs[4]].As<RecruitableLunarCultist>().MyName = (RecruitNames)i;
                                break;
                            }

                            break;
                    }
                    #endregion                        
                    ActivityTimer = 0;
                    zoom = 0;


                    foreach (Player player in Main.player.Where(p => p.active))
                    {
                        player.LunarCult().hasRecievedChefMeal = false;
                        if (player.LunarCult().apostleQuestTracker == 2 || player.LunarCult().apostleQuestTracker == 6 || player.LunarCult().apostleQuestTracker == 10)
                            player.LunarCult().apostleQuestTracker++;
                    }
                    
                }

                #region Player Proximity
                closestPlayer = Main.player[Player.FindClosest(new Vector2(ActivityCoords.X, ActivityCoords.Y), 300, 300)];
                PlayerDistFromHideout = new Vector2(closestPlayer.Center.X - ActivityCoords.X, closestPlayer.Center.Y - ActivityCoords.Y).Length();
                if (PlannedActivity == SystemStates.Meeting && PlayerDistFromHideout < 300f)
                {
                    State = SystemStates.Meeting;
                    Active = true;
                }
                #endregion
                #region Despawn  
                else if (Main.dayTime && !Active && PlayerDistFromHideout > 4000f)
                {
                    ActivityCoords = new(-1, -1);
                    State = SystemStates.CheckReqs;
                    OnCooldown = true;
                    foreach (int k in NPCIndexs)
                    {
                        Main.npc[k].active = false;
                    }
                }
                #endregion
                
                break;
            case SystemStates.OratorVisit:
                if ((TutorialComplete && RecruitmentsSkipped < 3) || (BetrayalActive && RecruitmentsSkipped >= 3))
                {
                    Active = false;
                    State = SystemStates.End;
                }
                else
                {
                    if(RecruitmentsSkipped < 3)
                        Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())].ai[0] = 1;
                    else
                        Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())].ai[0] = 3;
                }
                break;
            case SystemStates.Meeting:                     
                if (Active)
                {
                    #region Meeting Scene
                    NPC Bishop = Main.npc[NPCIndexs[0]];
                    NPC Cultist1 = Main.npc[NPCIndexs[1]];
                    NPC Cultist2 = Main.npc[NPCIndexs[2]];
                    NPC Cultist3 = Main.npc[NPCIndexs[3]];
                    NPC Cultist4 = Main.npc[NPCIndexs[4]];

                    Rectangle BishopLocation = new((int)Bishop.Center.X, (int)Bishop.Center.Y, Bishop.width, Bishop.width);
                    Rectangle Cultist1Location = new((int)Cultist1.Center.X, (int)Cultist1.Center.Y, Cultist1.width, Cultist1.width);
                    Rectangle Cultist2Location = new((int)Cultist2.Center.X, (int)Cultist2.Center.Y, Cultist2.width, Cultist2.width);
                    Rectangle Cultist3Location = new((int)Cultist3.Center.X, (int)Cultist3.Center.Y, Cultist3.width, Cultist3.width);
                    Rectangle Cultist4Location = new((int)Cultist4.Center.X, (int)Cultist4.Center.Y, Cultist4.width, Cultist4.width);

                    string key = $"Cutscenes/CultMeetings/{CurrentMeetingTopic}.";

                    if (ActivityTimer == 30)
                    {
                        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, key, new(Name, [Bishop.whoAmI]));
                        ActivityTimer++;
                    }

                    if (ActivityTimer > 40 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                        Active = false;

                    #endregion

                    #region Camera Setup
                    if (ActivityTimer < 100)
                        zoom = Lerp(zoom, 0.4f, 0.075f);
                    else
                        zoom = 0.4f;
                    CameraPanSystem.Zoom = zoom;
                    CameraPanSystem.PanTowards(new Vector2(ActivityCoords.X - 128 * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1), ActivityCoords.Y + 176), zoom * 2.5f);
                    #endregion

                    ActivityTimer++;
                }
                else
                {
                    #region Character Modifying
                    foreach (int i in NPCIndexs)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.type == ModContent.NPCType<LunarBishop>())
                        {
                            Item item = Main.item[Item.NewItem(npc.GetSource_Loot(), npc.Center, new Vector2(8, 4), ModContent.ItemType<LunarCoin>())];
                            item.velocity = Vector2.UnitY * -4;

                            for (int j = 0; j < 50; j++)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                                d.noGravity = true;
                            }
                            SoundEngine.PlaySound(TeleportSound, npc.Center);
                            npc.active = false;
                        }
                        else if (npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                        {
                            if (npc.ModNPC is RecruitableLunarCultist Recruit)
                                Recruit.Chattable = true;
                        }
                    }
                    #endregion

                    State = SystemStates.End;
                }
                break;
            case SystemStates.Tailor:
                if (!Active)
                {
                    foreach (Player player in Main.ActivePlayers)
                    {
                        foreach (Item item in player.inventory.Where(i => i.type == ModContent.ItemType<TailorInstructions>()))
                        {
                            item.stack = 0;
                        }
                    }
                    State = SystemStates.End;
                }
                else
                {
                    if (ActivityTimer == 240)
                        ModContent.GetInstance<TimerUISystem>().TimerStart(2 * 60 * 60);
                    else if (ActivityTimer > 240 && (ModContent.GetInstance<TimerUISystem>().EventTimer == null || ModContent.GetInstance<TimerUISystem>().EventTimer.timer < 0))
                        Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Seamstress>())].As<Seamstress>().EndActivity = true;
                    ActivityTimer++;
                }
                break;
            case SystemStates.Cafeteria:
                if (Active)
                {
                    if (CustomerQueue.Where(c => c.HasValue).Count() >= CustomerLimit || AtMaxTimer >= 10 * 60)
                        AtMaxTimer++;
                    else
                        AtMaxTimer = 0;
                    if (AtMaxTimer < 10 * 60)
                    {
                        if (SatisfiedCustomers < CustomerGoal && CustomerQueue.Count < CustomerLimit && ActivityTimer >= 360 && Main.rand.NextBool(120)) //Spawn New Customer
                        {
                            WeightedRandom<int> customerType = new();
                            customerType.Add(ModContent.NPCType<LunarCultistDevotee>(), 5);
                            customerType.Add(ModContent.NPCType<LunarCultistArcher>(), 3);
                            customerType.Add(ModContent.NPCType<LunarBishop>(), 1);

                            Vector2 chefCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center;
                            ActivityCoords = new((int)chefCenter.X, (int)chefCenter.Y);

                            Point spawnLocation = new(ActivityCoords.X + 1100, ActivityCoords.Y);
                            NPC.NewNPC(NPC.GetSource_NaturalSpawn(), spawnLocation.X, spawnLocation.Y, customerType, ai2: 2);

                            ActivityTimer = 0;
                        }
                        else if (SatisfiedCustomers >= CustomerGoal && CustomerQueue.Count == 0)
                        {
                            NPC chef = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())];
                            DisplayMessage(chef.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheChef.Activity.Finished");
                            Item i = Main.item[Item.NewItem(chef.GetSource_Loot(), chef.Center, new Vector2(8, 4), ModContent.ItemType<LunarCoin>())];
                            i.velocity = Vector2.UnitY * -4;
                            Active = false;
                            return;
                        }
                    }
                    ActivityTimer++;
                }
                else
                {
                    MenuFoodIDs = [];
                    foreach(Player player in Main.ActivePlayers)
                    {
                        foreach (Item item in player.inventory.Where(i => i.type == ModContent.ItemType<ChefMenu>()))
                        {
                            item.stack = 0;
                        }
                    }
                    State = SystemStates.End;
                }
                break;
            case SystemStates.Ritual:
                //ActivePortals = 0;
                //Main.NewText(ActivePortals);
                if (PortalsDowned < RequiredPortalKills && RemainingCultists > 0 && ActivePortals < RemainingCultists && ActivityTimer >= 60 && Main.rand.NextBool(100)) //Spawn New Customer
                {
                    Point spawnLocation = new(ActivityCoords.X + Main.rand.Next(-450, 450), ActivityCoords.Y - Main.rand.Next(120, 300));
                    NPC.NewNPC(NPC.GetSource_NaturalSpawn(), spawnLocation.X, spawnLocation.Y, ModContent.NPCType<PortalMole>(), Start: Main.npc[NPCIndexs[6]].whoAmI + 1);

                    ActivityTimer = 0;
                }
                else if (PortalsDowned >= RequiredPortalKills)
                {
                    NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];

                    if(NPCIndexs.Count != 0)
                    { 
                        if (ActivityTimer == 30)
                            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/RitualSuccess1", new(Name, [orator.whoAmI]));
                        else if (ActivityTimer > 60)
                        {
                            if (ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                                ActivityTimer = 60;
                            else
                                switch (ActivityTimer)
                                {
                                    case 110:
                                        if (Main.npc[NPCIndexs[0]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[0]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[0]].Center);
                                            Main.npc[NPCIndexs[0]].active = false;
                                        }
                                        if (Main.npc[NPCIndexs[5]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[5]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[5]].Center);
                                            Main.npc[NPCIndexs[5]].active = false;
                                        }
                                        break;
                                    case 130:
                                        if (Main.npc[NPCIndexs[1]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[1]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[1]].Center);
                                            Main.npc[NPCIndexs[1]].active = false;
                                        }
                                        if (Main.npc[NPCIndexs[4]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[4]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[4]].Center);
                                            Main.npc[NPCIndexs[4]].active = false;
                                        }
                                        break;
                                    case 150:
                                        if (Main.npc[NPCIndexs[2]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[2]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[2]].Center);
                                            Main.npc[NPCIndexs[2]].active = false;
                                        }
                                        if (Main.npc[NPCIndexs[3]].active)
                                        {
                                            for (int j = 0; j <= 50; j++)
                                            {
                                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                                Dust dust = Dust.NewDustPerfect(Main.npc[NPCIndexs[3]].Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                                dust.noGravity = true;
                                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                            }
                                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, Main.npc[NPCIndexs[3]].Center);
                                            Main.npc[NPCIndexs[3]].active = false;
                                        }
                                        break;
                                    case 210:
                                        NPCIndexs = [];
                                        ActivityTimer = 0;
                                        break;
                                }
                        }
                    }
                    else
                    {
                        if (ActivityTimer == 30)
                            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/RitualSuccess1", new(Name, [orator.whoAmI]), 5);
                        else if (ActivityTimer > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                        {
                            for (int i = 0; i <= 50; i++)
                            {
                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                Vector2 speed = Main.rand.NextVector2Circular(2f, 2.5f);
                                Dust dust = Dust.NewDustPerfect(orator.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                dust.noGravity = true;
                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                            }
                            SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, orator.Center);

                            orator.active = false;

                            Active = false;
                        }
                    }

                    ActivityTimer++;
                }
                else if(RemainingCultists <= 0)
                {
                    NPC orator = Main.npc[NPCIndexs[6]];

                    if (ActivityTimer == 30)
                        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/RitualFailure", new(Name, [orator.whoAmI]));
                    else if (ActivityTimer > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                    {
                        for (int i = 0; i <= 50; i++)
                        {
                            int dustStyle = Main.rand.NextBool() ? 66 : 263;
                            Vector2 speed = Main.rand.NextVector2Circular(2f, 2.5f);
                            Dust dust = Dust.NewDustPerfect(orator.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                            dust.noGravity = true;
                            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                        }
                        SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, orator.Center);

                        orator.active = false;

                        NPCIndexs = [];
                        Active = false;
                    }

                    ActivityTimer++;
                }
                else
                    ActivityTimer++;
                if(!Active)
                    State = SystemStates.End;
                break;
            case SystemStates.FinalMeeting:
                if(Active)
                {
                    #region Camera Setup
                    if (ActivityTimer < 100)
                        zoom = Lerp(zoom, 0.4f, 0.075f);
                    else
                        zoom = 0.4f;
                    CameraPanSystem.Zoom = zoom;
                    CameraPanSystem.PanTowards(new Vector2(ActivityCoords.X, ActivityCoords.Y - 150), zoom);
                    #endregion

                    NPC orator = Main.npc[NPCIndexs[0]];

                    if (ActivityTimer == 30)
                        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/CultMeetings/Final", new(Name, [orator.whoAmI]));
                    else if (ActivityTimer > 60 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                        Active = false;

                }
                else
                {
                    NPC orator = Main.npc[NPCIndexs[0]];

                    for (int i = 0; i < 25; i++)
                    {
                        Item item = Main.item[Item.NewItem(orator.GetSource_Loot(), orator.Center, new Vector2(8, 4), ModContent.ItemType<LunarCoin>())];
                        item.velocity = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-PiOver2, PiOver2)) * -4;
                    }

                    State = SystemStates.End;
                    FinalMeetingSeen = true;
                }
                break;
            case SystemStates.End:
                ActivityCoords = new(-1, -1);
                OnCooldown = true;
                RecruitmentsSkipped++;
                State = SystemStates.CheckReqs;

                Main.npc.First(n => n.active && n.type == ModContent.NPCType<Watchman>()).ai[2]++;

                if (!BetrayalActive)
                {
                    int currentRecruitCount = Main.npc.Where(n => n.active && n.type == ModContent.NPCType<RecruitableLunarCultist>()).Count();
                    if (currentRecruitCount < 4)
                    {
                        List<int> IDs =
                        [
                            0,//"Tirith",
                            1,//"Vivian",
                            2,//"Tania",
                            3,//"Doro",
                            4,//"Skylar",
                            5//"Jamie",
                        ];

                        List<Vector2> AvailablePositions =
                        [
                            new((CultBaseTileArea.Center.X - 4) * 16, (CultBaseTileArea.Center.Y - 8) * 16),
                            new((CultBaseTileArea.Center.X - 4) * 16, (CultBaseTileArea.Center.Y - 8) * 16),
                            new((CultBaseTileArea.Center.X - 4) * 16, CultBaseTileArea.Center.Y * 16),
                            new((CultBaseTileArea.Center.X - 4) * 16, CultBaseTileArea.Center.Y * 16),
                            Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center + (Vector2.UnitX * 256),
                            Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center + (Vector2.UnitX * 256)
                        ];

                        for (int i = 0; i < 4 - currentRecruitCount; i++)
                        {
                            NPC recruit = null;
                            int nameIndex = Main.rand.Next(IDs.Count);
                            int posIndex = Main.rand.Next(AvailablePositions.Count);
                            
                            recruit = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), AvailablePositions[posIndex], ModContent.NPCType<RecruitableLunarCultist>());

                            RecruitableLunarCultist Recruit = recruit.As<RecruitableLunarCultist>();
                            Recruit.MyName = (RecruitableLunarCultist.RecruitNames)IDs[nameIndex];
                            Recruit.Chattable = true;
                            Recruit.canRecruit = true;
                            Recruit.OnSpawn(NPC.GetSource_NaturalSpawn());

                            IDs.RemoveAt(nameIndex);
                            AvailablePositions.RemoveAt(posIndex);
                        }
                    }
                }                    
                break;
        }
    }

    private static void DraconicBoneSequence()
    {
        if (DraconicBoneTimer == 60)
        {
            Point spawnPos = CultBaseWorldArea.Center().ToPoint() + new Point(-16, 891);
            NPC.NewNPC(Entity.GetSource_None(), spawnPos.X, spawnPos.Y, ModContent.NPCType<OratorNPC>(), ai0: 4);
        }

        DraconicBoneTimer++;
    }

    public override void PreUpdateProjectiles()
    {
        if (LunarCultBaseLocation == new Point(-1, -1))
            return;

        // Handle Verlet Sims
        for (int i = 0; i < SkeletonVerletGroups.Count; i++)
        {
            var pair = SkeletonVerletGroups.ElementAt(i);
            //pair.Value.Clear();
            // Instanciate if empty
            if (pair.Value.Count == 0)
            {
                Vector2 spawnCenter;
                int verletCount;

                VerletObject box;
                VerletObject chain1;
                VerletObject chain2;

                switch (pair.Key)
                {
                    case "Skull":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(-160, 512);
                        verletCount = 10;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 64, 32));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-80, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(32, 0), box[1].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[1], 15);
                        break;
                    case "Ribs":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(0, 512);
                        verletCount = 10;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 156, 64));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-72, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(72, 0), box[1].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[1], 15);
                        break;
                    case "FrontWing":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(BaseFacingLeft ? 24 : 72, 512);
                        verletCount = 4;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 156, 96));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-84, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(84, 0), box[1].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[1], 15);
                        break;
                    case "BackWing":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(BaseFacingLeft ? 72 : 24, 512);
                        verletCount = 4;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 128, 96));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-84, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(84, 0), box[1].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[1], 15);
                        break;
                    case "FrontLeg":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(BaseFacingLeft ? 96 : 144, 512);
                        verletCount = 20;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 128, 64));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-32, 0), box[0].Position, verletCount - 4, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(32, 0), box[2].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[2], 15);
                        break;
                    case "BackLeg":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(BaseFacingLeft ? 144 : 96, 512);
                        verletCount = 20;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 128, 64));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-32, 0), box[0].Position, verletCount - 4, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(32, 0), box[2].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[2], 15);
                        break;
                    case "Tail":
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(208, 512);
                        verletCount = 13;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 88, 32));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-32, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(32, 0), box[1].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[1], 15);
                        break;
                    default:
                        spawnCenter = CultBaseWorldArea.Center.ToVector2() + new Vector2(0, 512);
                        verletCount = 9;

                        box = CreateVerletBox(new((int)spawnCenter.X, (int)spawnCenter.Y, 32, 32));
                        chain1 = CreateVerletChain(spawnCenter + new Vector2(-32, 0), box[0].Position, verletCount, 15);
                        chain2 = CreateVerletChain(spawnCenter + new Vector2(32, 0), box[2].Position, verletCount, 15);

                        ConnectVerlets(chain1[^1], box[0], 15);
                        ConnectVerlets(chain2[^1], box[2], 15);
                        break;
                }

                pair.Value.Add(box);
                pair.Value.Add(chain1);
                pair.Value.Add(chain2);

                if(i != 0)
                {
                    switch(pair.Key)
                    {
                        case "Ribs":
                            VerletObject ribBox = pair.Value[0];
                            SetupMidPointConnection(ribBox, 1, SkeletonVerletGroups["Tail"][0].Points[0], 40);
                            SetupMidPointConnection(ribBox, 2, SkeletonVerletGroups["BackLeg"][0].Points[0], 24);
                            SetupMidPointConnection(ribBox, 0, SkeletonVerletGroups["BackWing"][0].Points[3], 24);
                            break;
                        case "Skull":
                            SetupMidPointConnection(pair.Value[0], 1, SkeletonVerletGroups["Ribs"][0].Points[0], 42);
                            SetupMidPointConnection(pair.Value[0], 2, SkeletonVerletGroups["Ribs"][0].Points[3], 42);
                            break;
                        case "FrontWing":
                            SetupMidPointConnection(pair.Value[0], 3, SkeletonVerletGroups["Ribs"][0].Points[0], 48);
                            break;
                        case "FrontLeg":
                            SetupMidPointConnection(pair.Value[0], 0, SkeletonVerletGroups["Ribs"][0].Points[2], 24);
                            break;
                    }
                }
            }

            bool itemDropped = false;
            int boneType = ModContent.ItemType<DraconicBone>();

            for (int j = 0; j < pair.Value.Count; j++)
            {
                VerletObject obj = pair.Value[j];
                bool temp = AffectVerletObject(obj, 0.005f, 0.1f, j != 0);
                if (itemDropped)
                    continue;

                foreach(Player player in Main.ActivePlayers)
                {
                    if (itemDropped = player.HasItem(boneType))
                        break;
                }

                if (QuestSystem.Quests["DraconicBone"].Active && !itemDropped && temp && !Main.item.Any(t => t.active && t.type == boneType) && Main.rand.NextBool(50))
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 spawnPos = obj.Type == ObjectType.Chain ? obj[obj.Count / 2].Position : (obj[0].Position + obj[2].Position) / 2f;
                        Item.NewItem(Item.GetSource_NaturalSpawn(), spawnPos, boneType);
                    }
                    itemDropped = true;
                }
            }

            VerletSimulation(pair.Value, 30, 0.8f, false);
        }
    }

    private void DrawHangingSkeleton(On_Main.orig_DrawProjectiles orig, Main self)
    {
        if (LunarCultBaseLocation == new Point(-1, -1))
        {
            orig(self);
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (KeyValuePair<string, List<VerletObject>> pair in SkeletonVerletGroups)
        {
            /*
            Debug Lines
            foreach(var point in pair.Value[0].Points)
            {
                foreach(var connection in point.Connections)
                {
                    Vector2 line = point.Position - connection.Point.Position;
                    Color lighting = Lighting.GetColor((connection.Point.Position + line / 2f).ToTileCoordinates());
                    Main.spriteBatch.DrawLineBetween(point.Position, connection.Point.Position, Color.Red.MultiplyRGB(lighting), 3);
                }
            }
            */
            for(int j = 1; j < pair.Value.Count; j++)
            {
                for (int i = 0; i < pair.Value[j].Count; i++)
                    new SelenicTwine().DrawRopeSegment(Main.spriteBatch, pair.Value[j].Points, i);
            }

            if (pair.Value.Count > 0)
            {
                Vector2 drawPos = (pair.Value[0].Positions[0] + pair.Value[0].Positions[2]) / 2f;
                var drawRot = (pair.Value[0].Positions[0] - pair.Value[0].Positions[1]).ToRotation() + Pi;
                Main.spriteBatch.Draw(SkeletonAssets[pair.Key].Value, drawPos - Main.screenPosition, null, Lighting.GetColor(drawPos.ToTileCoordinates()), drawRot, SkeletonAssets[pair.Key].Size() * 0.5f, 1f, 0, 0);
            }
        }

        Main.spriteBatch.End();

        orig(self);
    }

    internal static CombatText DisplayMessage(Rectangle location, Color color, string text)
    {
        CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, GetWindfallTextValue(text), true)];
        return MyDialogue;
    }
    public static bool IsTailorActivityActive() => Active && State == SystemStates.Tailor;
    public static bool IsCafeteriaActivityActive() => Active && State == SystemStates.Cafeteria;
    public static bool IsRitualActivityActive() => Active && State == SystemStates.Ritual;
    public static Response[] GetMenuResponses()
    {           
        if(MenuFoodIDs.Count == 0)
        {
            MenuFoodIDs = FoodIDs;
            while (MenuFoodIDs.Count > 5)
                MenuFoodIDs.RemoveAt(Main.rand.Next(MenuFoodIDs.Count));
        }
        Response[] Responses = new Response[MenuFoodIDs.Count];
        for (int i = 0; i < MenuFoodIDs.Count; i++)
        {
            Item item = new(MenuFoodIDs[i]);
            Responses[i] = new Response{Title = item.Name};
        }
        return Responses;
    }
    public static void ResetTimer() => ActivityTimer = 0;
    public static void SpawnFingerling()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < 16; i++)
        {
            Rectangle spawnArea = CultBaseTileArea;
            int checkPositionX = spawnArea.X + Main.rand.Next(spawnArea.Width);
            int checkPositionY = spawnArea.Y + Main.rand.Next(spawnArea.Height);
            Vector2 checkPosition = new(checkPositionX * 16 + 8, checkPositionY * 16);
            
            Tile SpawnTile = ParanoidTileRetrieval(checkPositionX, checkPositionY);
            bool nearPlayer = WindfallUtils.ManhattanDistance(checkPosition, Main.player[Player.FindClosest(checkPosition, 16, 16)].Center) < 800f;
            bool isVaildWall = SpawnTile.WallType == WallID.AncientSilverBrickWall || SpawnTile.WallType == WallID.GreenStainedGlass || SpawnTile.WallType == WallID.EmeraldGemspark;
            isVaildWall |= SpawnTile.WallType == WallID.PlatinumBrick || SpawnTile.WallType == WallID.PearlstoneBrick;
            if (!isVaildWall || nearPlayer || SpawnTile.IsSolid() || Lighting.Brightness(checkPositionX, checkPositionY) <= 0.4f)
                continue;

            int spawnedNPC = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), checkPositionX * 16 + 8, checkPositionY * 16, ModContent.NPCType<Fingerling>());

            if (Main.netMode == NetmodeID.Server && spawnedNPC < Main.maxNPCs)
            {
                Main.npc[spawnedNPC].position.Y -= 8f;

                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedNPC);                
            }
            return;
        }
    }
}

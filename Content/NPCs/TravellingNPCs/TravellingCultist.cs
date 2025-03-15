using CalamityMod.Projectiles.Magic;
using Terraria.GameContent.Bestiary;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Quests.SealingRitual;
using CalamityMod.NPCs.TownNPCs;
using Terraria.ModLoader.IO;
using static Windfall.Common.Systems.PathfindingSystem;

namespace Windfall.Content.NPCs.TravellingNPCs;

public class TravellingCultist : ModNPC, ILocalizedModType
{
    public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
    public override string HeadTexture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist_Head";

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
        BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

        new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(TravellingCultist)}")),
    ]);
    }
    NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
    {
        Velocity = 1f,
        Direction = 1
    };

    public const double despawnTime = 48600.0;
    public static double spawnTime = double.MaxValue;
    private static Profiles.StackedNPCProfile NPCProfile;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 25;
        NPCID.Sets.ExtraFramesCount[Type] = 9;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 60;
        NPCID.Sets.AttackType[Type] = 2;
        NPCID.Sets.AttackTime[Type] = 12;
        NPCID.Sets.AttackAverageChance[Type] = 1;
        NPCID.Sets.HatOffsetY[Type] = 4;
        NPCID.Sets.ShimmerTownTransform[Type] = false;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TravellingCultist>();
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseTree;

        // Influences how the NPC looks in the Bestiary
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = 1
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        NPCProfile = new Profiles.StackedNPCProfile(
            new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture))
        );
    }
    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 24;
        NPC.height = 36;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit55;
        NPC.DeathSound = SoundID.NPCDeath59;
        NPC.knockBackResist = 0.5f;
        NPC.immortal = true;

        myBehavior = BehaviorState.FollowPlayer;
    }

    public static readonly List<Tuple<int, int>> RitualQuestItems =
    [
        new(ModContent.ItemType<TabletFragment>(), 1),
        new (ModContent.ItemType<DraconicBone>(), 1),
        new (ModContent.ItemType<PrimalLightShard>(), 1),
    ];
    public static readonly List<Tuple<int, int>> DungeonQuestItems =
    [
        new (ItemID.Bone, 50),
        new (ItemID.WaterBolt, 1),
        new (ModContent.ItemType<DeificInsignia>(), 5),
    ];
    public static Tuple<int, int> QuestItem = null;

    public enum QuestStatus
    {
        None = 0,
        NotStarted,
        Started,
        Completed
    }
    public static QuestStatus FetchQuestStatus = QuestStatus.None;

    private enum DialogueState
    {
        SearchForHelp,
        DungeonQuest,
        RitualDiscovered,
        RitualQuestWayfinder,
        RitualQuestTablet,
        RitualQuestGap,
        SelenicOrderTalk,
        RitualQuestRecruitmentOnly,
        RitualQuestGap2, //In the event the player recruits everyone before killing Golem
        RitualQuestRecruitmentAndShard,
        RitualQuestShardFinished,
        RitualQuestRecruitmentFinished,
        RitualQuestBone,
        RitualTalk,
        QuestlineFinished,
    }
    private static DialogueState CurrentDialogue = DialogueState.SearchForHelp;

    private enum BehaviorState
    {
        StandStill,
        Wander,
        FollowPlayer,
        MoveToTargetLocation
    }
    private BehaviorState myBehavior = BehaviorState.Wander;

    private enum PriorityTiers
    {
        None,
        MinorEvent,
        MajorEvent,
        QuestUpdate
    }
    private static readonly DialoguePool pool = new(
    [
        new("TravellingCultist/Introductions/Standard", (player) => true, (byte)PriorityTiers.None, true),
        new("TravellingCultist/Introductions/Permafrost", (player) => DownedBossSystem.downedCryogen && NPC.AnyNPCs(ModContent.NPCType<DILF>()), (byte)PriorityTiers.None, true),
        //Quest Updates
        new("TravellingCultist/Introductions/SearchForHelp", (player) => CurrentDialogue == DialogueState.SearchForHelp, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/RitualDiscovered", (player) => CurrentDialogue == DialogueState.RitualDiscovered, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/WayfinderQuest", (player) => CurrentDialogue == DialogueState.RitualQuestWayfinder, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/TabletInterference", (player) => CurrentDialogue == DialogueState.RitualQuestGap, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/SelenicOrderTalk", (player) => CurrentDialogue == DialogueState.SelenicOrderTalk, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/LightShardQuest", (player) => CurrentDialogue == DialogueState.RitualQuestRecruitmentAndShard, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/RecruitmentComplete", (player) => QuestSystem.Quests["Recruitment"].Complete, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/BoneQuest", (player) => CurrentDialogue == DialogueState.RitualQuestBone, (byte)PriorityTiers.QuestUpdate, false),
        new("TravellingCultist/Introductions/RitualTalk", (player) => CurrentDialogue == DialogueState.RitualTalk, (byte)PriorityTiers.QuestUpdate, false),
    ]);
    internal bool introductionDone = false;

    public override void OnSpawn(IEntitySource source)
    {
        if (NPC.ai[3] == 1)
        {
            NPC.aiStyle = -1;
            NPC.direction = 1;
            return;
        }

        if (CurrentDialogue == DialogueState.DungeonQuest)
            FetchQuestStatus = QuestStatus.NotStarted;
        else
            FetchQuestStatus = QuestStatus.None;

        QuestItem = null;
        introductionDone = false;

        bool MilestoneMet = false;

        switch (CurrentDialogue)
        {
            case DialogueState.DungeonQuest:
                if (Main.hardMode)
                    MilestoneMet = true;
                break;
            case DialogueState.RitualDiscovered:
                MilestoneMet = true;
                break;
            case DialogueState.RitualQuestGap:
                if (NPC.downedPlantBoss)
                    MilestoneMet = true;
                break;
            case DialogueState.RitualQuestRecruitmentOnly:
                if (NPC.downedGolemBoss)
                {
                    if (QuestSystem.Quests["Recruitment"].Complete)
                        CurrentDialogue = DialogueState.RitualQuestRecruitmentFinished;
                    else
                        CurrentDialogue = DialogueState.RitualQuestRecruitmentAndShard;
                    
                }
                break;
            case DialogueState.RitualQuestGap2:
                if (NPC.downedGolemBoss)
                {
                    CurrentDialogue = DialogueState.RitualQuestRecruitmentFinished;
                }
                break;
            case DialogueState.RitualQuestRecruitmentAndShard:
                if (QuestSystem.Quests["Recruitment"].Complete)
                {
                    CurrentDialogue = DialogueState.RitualQuestRecruitmentFinished;
                }
                break;
            case DialogueState.RitualQuestRecruitmentFinished:
                if (QuestSystem.Quests["PrimordialLightShard"].Complete)
                    CurrentDialogue = DialogueState.RitualQuestBone;
                break;
            case DialogueState.RitualQuestBone:
                if (QuestSystem.Quests["DraconicBone"].Complete)
                    MilestoneMet = true;
                break;
        }

        if (MilestoneMet)
            CurrentDialogue++;
    }

    public override bool CanChat() => NPC.ai[3] != 1 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();
        myBehavior = BehaviorState.StandStill;
        if (NPC.Center.X < Main.LocalPlayer.Center.X)
            NPC.direction = -1;
        else
            NPC.direction = 1;
        NPC.spriteDirection = -NPC.direction;

        if(introductionDone)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TravellingCultist/Default", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, pool.GetTree(Main.LocalPlayer), new(Name, [NPC.whoAmI]));

        return "";
    }
    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (uiSystem.CurrentDialogueContext.Catagory != "TravellingCultist")
            return;

        NPC cultist = Main.npc[(int)uiSystem.CurrentDialogueContext.Arguments[0]];

        if (QuestItem == null)
        {
            if (CurrentDialogue.ToString().Contains("Ritual"))
            {
                switch(CurrentDialogue)
                {
                    case DialogueState.RitualQuestTablet:
                        QuestItem = RitualQuestItems[0];
                        break;
                    case DialogueState.RitualQuestRecruitmentAndShard:
                    case DialogueState.RitualQuestRecruitmentFinished:
                        QuestItem = RitualQuestItems[1];
                        break;
                    case DialogueState.RitualQuestBone:
                        QuestItem = RitualQuestItems[2];
                        break;
                }
            }
            else
                QuestItem = DungeonQuestItems[Main.rand.Next(3)];
        }

        switch (treeKey)
        {
            case "TravellingCultist/Default":
                #region Random Text
                if (swapped)
                    uiSystem.CurrentTree.Dialogues[0].DialogueText[0].Text = "But anyways, is there anything I can help you with?";
                else
                {
                    WeightedRandom<string> chat = new();

                    chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard1"));
                    chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard2"));
                    chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard3"));
                    if (NPC.AnyNPCs(NPCID.Mechanic) || NPC.AnyNPCs(NPCID.Clothier))
                    {
                        chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.AnyDungeonNPC"));
                        if (NPC.AnyNPCs(NPCID.Mechanic))
                            chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Mechanic"));
                        if (NPC.AnyNPCs(NPCID.Clothier))
                            chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Clothier"));
                    }
                    if (NPC.downedBoss3)
                        if (Main.rand.NextBool(10))
                            chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.SkeletronRare"));
                        else
                            chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.Skeletron"));
                    for (int i = 0; i < LunarCultBaseSystem.Recruits.Count; i++)
                        chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.{(RecruitableLunarCultist.RecruitNames)i}"));

                    uiSystem.CurrentTree.Dialogues[0].DialogueText[0].Text = chat;
                }
                #endregion
                switch (LunarCultBaseSystem.Recruits.Count)
                {
                    case 0:
                        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Recruits.None");
                        break;
                    case 1:
                        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.One").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0]);
                        break;
                    case 2:
                        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.Two").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[1]);
                        break;
                    case 3:
                        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.Three").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[1], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[2]);
                        break;
                    case 4:
                        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Recruits.Four");
                        break;
                }

                if (((FetchQuestStatus == QuestStatus.NotStarted || FetchQuestStatus == QuestStatus.Started) && QuestItem != null) || AnyQuestsInProgress())
                {
                    uiSystem.CurrentTree.Dialogues[0].Responses[0].Requirement = true;
                    if (AnyQuestsInProgress())
                    {
                        string path = "TravellingCultist/QuestProgress/";
                        switch (CurrentDialogue)
                        {
                            case DialogueState.RitualQuestWayfinder:
                                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = path + "QuestWayfinder";
                                break;
                            case DialogueState.RitualQuestTablet:
                                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = path + "QuestTablet";
                                break;
                            case DialogueState.RitualQuestRecruitmentAndShard:
                            case DialogueState.RitualQuestRecruitmentFinished:
                                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = path + "QuestLightShard";
                                break;
                            case DialogueState.RitualQuestBone:
                                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = path + "QuestDragonBone";
                                break;
                        }
                        uiSystem.CurrentTree.Dialogues[0].Responses[0].Heading = 0;
                    }
                    else
                    {
                        uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = "StandardQuestTree";
                        uiSystem.CurrentTree.Dialogues[0].Responses[0].Heading = (FetchQuestStatus == QuestStatus.NotStarted ? 0 : 1);
                    }
                }
                if (CurrentDialogue >= DialogueState.RitualQuestRecruitmentOnly && CurrentDialogue < DialogueState.RitualQuestRecruitmentFinished)
                    uiSystem.CurrentTree.Dialogues[0].Responses[1].Requirement = true;

                break;
            case "StandardQuestTree":
                SetupFetchQuestTree(
                    ref uiSystem,
                    "LunarCult.TravellingCultist.Quests.Dungeon.",
                    "TheCalamity",//"TravellingCultist"
                    QuestItem);
                break;          
            case "TravellingCultist/QuestProgress/QuestTablet":
                AttachCostToResponse(ref uiSystem, new Tuple<int, int>(ModContent.ItemType<TabletFragment>(), 4), 0, 0);
                break;
            case "TravellingCultist/QuestProgress/QuestLightShard":
                AttachCostToResponse(ref uiSystem, new Tuple<int, int>(ModContent.ItemType<PrimalLightShard>(), 1), 0, 0);
                break;
            case "TravellingCultist/QuestProgress/QuestDragonBone":
                AttachCostToResponse(ref uiSystem, new Tuple<int, int>(ModContent.ItemType<DraconicBone>(), 1), 0, 0);
                break;
        }
    }
    private static bool AnyQuestsInProgress() => (QuestSystem.Quests["TabletFragment"].InProgress && (CurrentDialogue == DialogueState.RitualQuestWayfinder || CurrentDialogue == DialogueState.RitualQuestTablet)) ||
                                                 (QuestSystem.Quests["PrimordialLightShard"].InProgress && (CurrentDialogue == DialogueState.RitualQuestRecruitmentAndShard || CurrentDialogue == DialogueState.RitualQuestRecruitmentFinished)) ||
                                                 (QuestSystem.Quests["DraconicBone"].InProgress && CurrentDialogue == DialogueState.RitualQuestBone);

    private static void CloseTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (uiSystem.CurrentDialogueContext.Catagory != "TravellingCultist")
            return;

        NPC cultist = Main.npc[(int)uiSystem.CurrentDialogueContext.Arguments[0]];

        if (treeKey.Contains("Introductions"))
            cultist.As<TravellingCultist>().introductionDone = true;

        switch (treeKey)
        {
            #region Introductions
            case "TravellingCultist/Introductions/SearchForHelp":
                CurrentDialogue = DialogueState.DungeonQuest;
                FetchQuestStatus = QuestStatus.NotStarted;
                break;
            case "TravellingCultist/Introductions/SelenicOrderTalk":
                CurrentDialogue = DialogueState.RitualQuestRecruitmentOnly;
                QuestSystem.Quests["Recruitment"].ResetQuest();
                QuestSystem.Quests["Recruitment"].Active = true;
                break;
            case "TravellingCultist/Introductions/RitualTalk":
                CurrentDialogue = DialogueState.QuestlineFinished;
                QuestSystem.Quests["SealingRitual"].ResetQuest();
                QuestSystem.Quests["SealingRitual"].Active = true;
                break;            
            case "TravellingCultist/Introductions/WayfinderQuest":
                QuestSystem.Quests["TabletFragment"].ResetQuest();
                QuestSystem.Quests["TabletFragment"].Active = true;
                break;
            case "TravellingCultist/Introductions/LightShardQuest":
                CurrentDialogue = DialogueState.RitualQuestRecruitmentAndShard;
                QuestSystem.Quests["PrimordialLightShard"].ResetQuest();
                QuestSystem.Quests["PrimordialLightShard"].Active = true;
                break;
            case "TravellingCultist/Introductions/BoneQuest":
                CurrentDialogue = DialogueState.RitualQuestBone;
                QuestSystem.Quests["DraconicBone"].ResetQuest();
                QuestSystem.Quests["DraconicBone"].Active = true;
                break;
            #endregion
            case "StandardQuestTree":
                if (dialogueID == 2)
                {
                    Item.NewItem(cultist.GetSource_GiftOrReward(), cultist.Center, Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-PiOver4, PiOver4)) * -4, ItemID.DungeonFishingCrateHard);
                    QuestItem = null;
                    FetchQuestStatus = QuestStatus.Completed;
                }
                else
                    FetchQuestStatus = QuestStatus.Started;
                break;
            #region Quest Progress
            case "TravellingCultist/QuestProgress/QuestWayfinder":
                if (dialogueID == 1 && QuestSystem.Quests["TabletFragment"].Progress == 0)
                {
                    QuestSystem.Quests["TabletFragment"].IncrementProgress();
                    CurrentDialogue = DialogueState.RitualQuestTablet;
                }
                break;
            case "TravellingCultist/QuestProgress/QuestTablet":
                if (dialogueID == 1)
                {
                    QuestSystem.Quests["TabletFragment"].IncrementProgress();
                    CurrentDialogue++;
                }
                break;
            case "TravellingCultist/QuestProgress/QuestLightShard":
                if (dialogueID == 1)
                    QuestSystem.Quests["PrimordialLightShard"].IncrementProgress();
                break;
            case "TravellingCultist/QuestProgress/QuestDragonBone":
                if (dialogueID == 1)
                    QuestSystem.Quests["DraconicBone"].IncrementProgress();
                break;
            #endregion
        }
    }

    public override bool PreAI()
    {
        if (NPC.ai[3] == 0 && (!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
        {
            // Here we despawn the NPC and send a message stating that the NPC has despawned
            if (NPC.active)
                DisplayLocalizedText("The " + DisplayName + " has departed!", new(50, 125, 255));
            NPC.netSkip = -1;
            NPC.active = false;
            return false;
        }
        return true;
    }
    private static bool IsNpcOnscreen(Vector2 center)
    {
        int w = NPC.sWidth + NPC.safeRangeX * 2;
        int h = NPC.sHeight + NPC.safeRangeY * 2;
        Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
        foreach (Player player in Main.player)
            if (player.active && player.getRect().Intersects(npcScreenRect))
                return true;
        return false;
    }

    private int Time = 0;
    private PathFinding pathFinding = new();
    private int CurrentWaypoint = 0;
    private float MoveSpeed = 3f;
    private int jumpTimer = 0;

    public override void AI()
    {
        //NPC.velocity.Y += 0.66f;

        switch(myBehavior)
        {
            case BehaviorState.Wander:
                //Change movement
                if(Main.rand.NextBool(NPC.velocity.X == 0 ? 150 : 100))
                {
                    if (NPC.velocity.X != 0)// && Main.rand.NextBool())
                    {
                        NPC.velocity.X *= 0.8f;
                    }
                    else
                    {
                        if (NPC.velocity.X != 0)
                            NPC.velocity.X = -NPC.velocity.X;
                        else
                            NPC.velocity.X = 1f * (Main.rand.NextBool() ? 1 : -1);
                    }
                }

                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                if (NPC.velocity.X != 0)
                {
                    if (Math.Abs(NPC.velocity.X) != 1)
                    {
                        NPC.velocity.X *= 0.8f;
                        if (Math.Abs(NPC.velocity.X) < 0.01f)
                        {
                            NPC.velocity.X = 0;
                            return;
                        }
                    }
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    NPC.spriteDirection = NPC.direction;
                }
                break;
            case BehaviorState.FollowPlayer:
                Vector2 targetPos = Main.LocalPlayer.Center;

                if (jumpTimer != 0 || Time % 10 == 0)
                {
                    pathFinding.FindPath(NPC.Center, targetPos, NPC.IsWalkableThroughDoors, NPC.noGravity ? null : GravityCostFunction, 1300);
                    
                    CurrentWaypoint = 1;
                }
                /*
                for (int i = 0; i < pathFinding.MyPath.Points.Length; i++)
                {
                    Particle p = new GlowOrbParticle(pathFinding.MyPath.Points[i].ToWorldCoordinates(), Vector2.Zero, false, 2, 0.5f, i == CurrentWaypoint ? Color.White : Color.Red);
                    GeneralParticleHandler.SpawnParticle(p);
                }
                */
                Time++;
                if (jumpTimer > 0)
                    jumpTimer--;

                
                bool MovementSuccess = true;
                float distanceToTarget = Vector2.DistanceSquared(NPC.Center, targetPos);
                bool jumpStarted = false;

                if (distanceToTarget < 10000 && Collision.CanHit(NPC, Main.LocalPlayer))
                {
                    if (NPC.noGravity)
                    {
                        Vector2 ground = FindSurfaceBelow(Main.LocalPlayer.Center.ToTileCoordinates()).ToWorldCoordinates();
                        int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, ground);
                        if (distance <= 600)
                            NPC.noGravity = false;
                    }
                    MoveSpeed = 3f;
                    MovementSuccess = false;
                }
                else
                {
                    if (NPC.noGravity)
                        MovementSuccess = AntiGravityPathfindingMovement(NPC, pathFinding, ref CurrentWaypoint, 8, 1f, 0.66f);
                    else
                    {
                        if ((distanceToTarget > 250000 && Time % 120 == 0) || (distanceToTarget > 500000 && Time % 30 == 0))
                        {
                            if (MoveSpeed < 6f)
                                MoveSpeed += 1;
                            else
                                NPC.noGravity = true;
                        }
                        MovementSuccess = GravityAffectedPathfindingMovement(NPC, pathFinding, ref CurrentWaypoint, out NPC.velocity, out jumpStarted, MoveSpeed, 8.5f, 0.25f);
                    }
                }

                if (!MovementSuccess)
                {
                    if (jumpStarted)
                        NPC.noGravity = true;
                    else
                        NPC.velocity.X *= 0.8f;
                }
                else
                {
                    if(jumpStarted)
                    {
                        jumpTimer = 30;
                        Vector2 ground = FindSurfaceBelow(Main.LocalPlayer.Center.ToTileCoordinates()).ToWorldCoordinates();
                        int distance = (int)Vector2.Distance(Main.LocalPlayer.Center, ground);
                        if (distance > 600)
                            NPC.noGravity = true;
                        else if(Main.LocalPlayer.Center.Y < NPC.Center.Y)
                        {
                            distance = (int)Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);
                            if(distance > 800)
                                NPC.noGravity = true;
                        }
                    }
                }

                if (NPC.velocity.X != 0)
                {
                    NPC.direction = Math.Sign(NPC.velocity.X);
                    NPC.spriteDirection = NPC.direction;
                }
                if (NPC.direction == -1)
                {
                    Point p = NPC.Left.ToTileCoordinates();
                    p.X -= 1;
                    bool opened = TryOpenDoor(p, -1);
                }
                else
                {
                    Point p = NPC.Right.ToTileCoordinates();
                    p.X += 1;
                    bool opened = TryOpenDoor(p, -1);

                }
                /*
                if (!MovementSuccess)
                {
                    NPC.velocity.X *= 0.8f;
                    if (Math.Abs(NPC.velocity.X) < 0.01f)
                    {
                        NPC.velocity.X = 0;
                        return;
                    }
                }
                */
                break;
            case BehaviorState.StandStill:
                NPC.velocity.X *= 0.8f;
                break;
        }

        //Debug
        //if (CurrentDialogue == DialogueState.SelenicOrderTalk)
        //    CurrentDialogue++;
        //foreach(var item in pool.CirculatingDialogues)
        //Main.NewText(QuestSystem.Quests["SealingRitual"].Progress);
        //CurrentDialogue = DialogueState.RitualQuestWayfinder;
    }

    public override bool? CanFallThroughPlatforms()
    {
        if (pathFinding.MyPath == null || pathFinding.MyPath.Points.Length == 0 || pathFinding.MyPath.Points.Length <= CurrentWaypoint)
            return false;
        int checkIndex = 3;
        if (pathFinding.MyPath.Points.Length <= CurrentWaypoint + checkIndex)
            checkIndex = pathFinding.MyPath.Points.Length - 1;
        return pathFinding.MyPath.Points[checkIndex].Y > pathFinding.MyPath.Points[CurrentWaypoint].Y;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["DialogueState"] = (int)CurrentDialogue;

        List<string> circulatingTrees = [];
        for (int i = 0; i < pool.CirculatingDialogues.Count; i++)
        {
            circulatingTrees.Add(pool.CirculatingDialogues[i].TreeKey);
        }
        tag["CirculatingDialogue"] = circulatingTrees;
    }

    public override void LoadData(TagCompound tag)
    {
        CurrentDialogue = (DialogueState)tag.GetInt("DialogueState");

        pool.CirculatingDialogues.Clear();
        List<string> circulatingTrees = (List<string>)tag.GetList<string>("CirculatingDialogue");
        for (int i = 0; i < pool.CirculatingDialogues.Count; i++)
        {
            var (TreeKey, Requirement, Priority, Repeatable) = pool.Dialogues.First(d => d.TreeKey == circulatingTrees[i]);
            pool.CirculatingDialogues.Add(new(TreeKey, Requirement, Priority));
        }
    }

    #region Town NPC Stuff
    public override bool CanTownNPCSpawn(int numTownNPCs) => false;
    public override ITownNPCProfile TownNPCProfile() => NPCProfile;
    public override List<string> SetNPCNameList()
    {
        return [
            "Strange Cultist",
            "Poorly Disguised Cultist",
            "Suspicious Looking Cultist",
        ];
    }
    
    public static void UpdateTravelingMerchant()
    {
        bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>()) != -1);

        if (Main.dayTime && Main.time == 10)
        {
            if (!travelerIsThere && (Main.rand.NextBool(4) || (NPC.downedPlantBoss && CurrentDialogue == DialogueState.RitualQuestGap)))
            {
                spawnTime = GetRandomSpawnTime(5400, 8100);
            }
            else
            {
                spawnTime = double.MaxValue;
            }
        }
        if (!travelerIsThere && CanSpawnNow())
        {
            int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<TravellingCultist>(), 1); // Spawning at the world spawn
            NPC traveler = Main.npc[newTraveler];
            traveler.homeless = true;
            traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
            traveler.netUpdate = true;

            // Prevents the traveler from spawning again the same day
            spawnTime = double.MaxValue;

            string key = ("A " + traveler.FullName + " has arrived!");
            Color messageColor = new(50, 125, 255);
            DisplayLocalizedText(key, messageColor);
        }
    }
    private static bool CanSpawnNow()
    {
        //progression locks
        if (!Main.hardMode || !NPC.downedBoss3 || SealingRitualSystem.RitualSequenceSeen)
            return false;
        // can't spawn if any events are running
        if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
            return false;

        // can't spawn if the sundial is active
        if (Main.IsFastForwardingTime())
            return false;
        // can spawn if daytime, and between the spawn and despawn times
        return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
    }
    private static double GetRandomSpawnTime(double minTime, double maxTime) => (maxTime - minTime) * Main.rand.NextDouble() + minTime;

    public override void TownNPCAttackStrength(ref int damage, ref float knockback)
    {
        damage = 20;
        knockback = 4f;
    }
    public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
    {
        cooldown = 15;
        randExtraCooldown = 8;
    }
    public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
    {
        projType = ModContent.ProjectileType<PhantasmalFuryProj>();
        attackDelay = 1;
    }
    #endregion
}

using CalamityMod.World;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Items.Food;
using Windfall.Content.Items.Quest;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using DialogueHelper.Content.UI.Dialogue;
using Windfall.Content.NPCs.Critters;
using Windfall.Content.Buffs.DoT;

namespace Windfall.Common.Systems.WorldEvents
{
    public class LunarCultBaseSystem : ModSystem
    {
        public static Point LunarCultBaseLocation;

        public static Rectangle CultBaseArea;

        public static Rectangle CultBaseBridgeArea;

        public static List<int> Recruits = [];

        private static List<int> AvailableTopics = [];

        public static bool TutorialComplete = false;

        private static int spawnChance = 5;

        public override void ClearWorld()
        {
            LunarCultBaseLocation = new(-1, -1);

            CultBaseArea = new(-1, -1, 1, 1);

            CultBaseBridgeArea = new(-1, -1, 1, 1);

            Recruits = [];

            AvailableTopics = [];

            CustomerQueue = [];

            TutorialComplete = false;

            spawnChance = 5;

            Active = false;
            State = SystemState.CheckReqs;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            LunarCultBaseLocation = tag.Get<Point>("LunarCultBaseLocation");

            CultBaseArea = new((LunarCultBaseLocation.X - 73), (LunarCultBaseLocation.Y - 74), 126, 148);

            CultBaseBridgeArea = new(CultBaseArea.Right, CultBaseArea.Center.Y - 16, 20, 25);

            Recruits = (List<int>)tag.GetList<int>("Recruits");

            AvailableTopics = (List<int>)tag.GetList<int>("AvailableTopics");

            TutorialComplete = tag.GetBool("TutorialComplete");

            spawnChance = tag.GetInt("spawnChance");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["LunarCultBaseLocation"] = LunarCultBaseLocation;

            tag["Recruits"] = Recruits;

            tag["AvailableTopics"] = AvailableTopics;

            tag["TutorialComplete"] = TutorialComplete;

            tag["spawnChance"] = spawnChance;
        }

        public enum SystemState
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
            End,
        }
        public static SystemState State = SystemState.CheckReqs;

        private static bool OnCooldown = true;
        public static bool Active = false;
        private static int ActivityTimer = -1;
        public static Point ActivityCoords = new(-1, -1);
        private static List<int> NPCIndexs = [];
        private static float zoom = 0;        

        #region Meeting Variables
        public enum MeetingTopic
        {
            CurrentEvents,
            Mission,
            Paradise
        }
        public static MeetingTopic CurrentMeetingTopic;
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
        public static List<int> MenuFoodIDs =
        [
            ModContent.ItemType<AbyssalInkPasta>(),
            ModContent.ItemType<AzafurianPallela>(),
            ModContent.ItemType<EbonianCheddarBoard>(),
            ModContent.ItemType<EutrophicClamChowder>(),
            ModContent.ItemType<FriedToxicatfishSandwich>(),
            ModContent.ItemType<GlimmeringNigiri>(),
            ModContent.ItemType<LemonButterHermititanLegs>(),

        ];
        public static int SatisfiedCustomers = 0;
        public static int CustomerGoal = 8;
        private static readonly int CustomerLimit = 12;
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
            //Main.NewText(LunarCultBaseLocation == new Point(-1, -1));
            if (NPC.downedAncientCultist || LunarCultBaseLocation == new Point(-1, -1))
                return;
            //Main.NewText("Active");
            if (NPC.downedPlantBoss)
            {
                if (!NPC.AnyNPCs(ModContent.NPCType<Seamstress>()))
                    NPC.NewNPC(Entity.GetSource_None(), (LunarCultBaseLocation.X * 16) + 242, (LunarCultBaseLocation.Y * 16) + 300, ModContent.NPCType<Seamstress>());
                if (!NPC.AnyNPCs(ModContent.NPCType<TheChef>()))
                    NPC.NewNPC(Entity.GetSource_None(), (LunarCultBaseLocation.X * 16) - 1040, (LunarCultBaseLocation.Y * 16) - 110, ModContent.NPCType<TheChef>());
                if (!NPC.AnyNPCs(ModContent.NPCType<OratorNPC>()))
                    NPC.NewNPC(Entity.GetSource_None(), (CultBaseArea.Right - 11) * 16, (CultBaseArea.Top + 30) * 16, ModContent.NPCType<OratorNPC>());
                if(Main.npc.Where(n => n.active && (n.type == ModContent.NPCType<LunarCultistArcher>() || n.type == ModContent.NPCType<LunarCultistDevotee>() || n.type == ModContent.NPCType<RecruitableLunarCultist>())).Count() < 10)
                    AttemptToCultBaseDenizens();

                foreach (NPC npc in Main.npc.Where(n => n.active))
                {
                    if (!npc.dontTakeDamage && npc.lifeMax != 1 && !npc.friendly && !npc.boss && npc.type != ModContent.NPCType<PortalMole>())
                    {
                        Rectangle inflatedArea = new(CultBaseArea.X - 32, CultBaseArea.Y + 32, CultBaseArea.Width + 64, CultBaseArea.Height + 64);
                        if (inflatedArea.Contains(npc.Center.ToTileCoordinates()))
                            npc.AddBuff(ModContent.BuffType<Entropy>(), 2);
                    }                    
                }
            }
            if (Main.player.Any(p => p.active && !p.dead && CultBaseArea.Contains(p.Center.ToTileCoordinates())))
                CalamityWorld.ArmoredDiggerSpawnCooldown = 36000;
            foreach (Player player in Main.player.Where(p => p.active && !p.dead))
            {                    
                if (CultBaseArea.Contains(player.Center.ToTileCoordinates()) && player.Center.Y > (LunarCultBaseLocation.Y + 30) * 16)
                {
                    for (int i = 0; i <= 20; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(player.Center, Main.rand.NextVector2Circular(5f, 5f), 30 * Main.rand.NextFloat(1.5f, 2.3f));
                    player.Teleport(new Vector2((CultBaseArea.Right - 21) * 16, (CultBaseArea.Top + 27) * 16), TeleportationStyleID.DebugTeleport);
                    SoundEngine.PlaySound(SoundID.Item8, player.Center);
                    for (int i = 0; i <= 20; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(player.Center, Main.rand.NextVector2Circular(5f, 5f), 30 * Main.rand.NextFloat(1.5f, 2.3f));
                    NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];
                    string path = "Dialogue.LunarCult.TheOrator.WorldText.Basement." + Main.rand.Next(3);
                    DisplayMessage(orator.Hitbox, Color.LimeGreen, path);
                }
            }
            //State = SystemState.CheckReqs;
            //ActivityTimer = -1;
            //TutorialComplete = true;
            switch (State)
            {
                case SystemState.CheckReqs:
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
                        State = SystemState.CheckChance;
                        break;
                    }
                case SystemState.CheckChance:
                    if (spawnChance < 1)
                        spawnChance = 1;
                    if (Main.rand.NextBool(spawnChance))
                    {
                        State = SystemState.Waiting;
                        spawnChance = 5;
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
                        State = SystemState.CheckReqs;
                    }
                    break;
                case SystemState.Waiting:
                    if (ActivityTimer == -1)
                    { 
                        ActivityCoords = new Point(LunarCultBaseLocation.X + 63, LunarCultBaseLocation.Y - 6);
                        ActivityCoords.X *= 16;
                        ActivityCoords.Y *= 16;
                        ActivityCoords.Y -= 5;

                        if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<LunarBishop>() && n.ai[2] == 2))
                            NPC.NewNPC(Entity.GetSource_None(), ActivityCoords.X, ActivityCoords.Y, ModContent.NPCType<LunarBishop>(), ai2: 2);
                        ActivityTimer++;
                    }
                    #region Player Proximity
                    Player closestPlayer = Main.player[Player.FindClosest(new Vector2(ActivityCoords.X, ActivityCoords.Y), 300, 300)];
                    float PlayerDistFromHideout = new Vector2(closestPlayer.Center.X - ActivityCoords.X, closestPlayer.Center.Y - ActivityCoords.Y).Length();
                    if (PlayerDistFromHideout < 160f && closestPlayer.Center.Y < ActivityCoords.Y + 16)
                        State = SystemState.Yap;
                    #endregion
                    #region Despawn
                    if (Main.dayTime)
                    {
                        ActivityCoords = new(-1, -1);
                        State = SystemState.CheckReqs;
                        OnCooldown = true;
                    }
                    #endregion
                    break;
                case SystemState.Yap:
                    NPC bishop = Main.npc.First(n => n.active && n.type == ModContent.NPCType<LunarBishop>() && n.ai[2] == 2);
                    string path = "Dialogue.LunarCult.LunarBishop.Greeting.";

                    #region First Time Chat
                    if (!TutorialComplete)
                    {
                        path += "First.";
                        switch(ActivityTimer)
                        {
                            case 30:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 0);
                                break;
                            case 120:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 1);
                                break;
                            case 240:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 2);
                                break;
                            case 360:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 3);
                                break;
                            case 480:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 4);
                                break;
                            case 600:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 5);
                                break;
                            case 720:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 6);
                                break;
                            case 840:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 7);
                                break;
                            case 960:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 8);
                                break;
                            case 1020:
                                bishop.As<LunarBishop>().Despawn();
                                State = SystemState.Ready;
                                break;
                        }
                    }
                    #endregion

                    #region Activity Chats
                    else
                    {
                        if (true)//Main.moonPhase == (int)MoonPhase.Full || Main.moonPhase == (int)MoonPhase.Empty) //Ritual
                            path += "Ritual.";
                        else if (false)//Main.moonPhase == (int)MoonPhase.QuarterAtLeft || Main.moonPhase == (int)MoonPhase.QuarterAtRight) //Meeting
                            path += "Meeting.";
                        else if (false)//Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight) //Tailor
                            path += "Tailor.";
                        else //Cafeteria
                            path += "Cafeteria.";
                        switch (ActivityTimer)
                        {
                            case 30:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 0);
                                break;
                            case 120:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 1);
                                break;
                            case 240:
                                DisplayMessage(bishop.Hitbox, Color.LimeGreen, path + 2);
                                break;
                            case 360:
                                bishop.As<LunarBishop>().Despawn();
                                State = SystemState.Ready;
                                break;
                        }
                    }
                    #endregion

                    #region Camera Setup
                    if (ActivityTimer < 100)
                        zoom = Lerp(zoom, 0.4f, 0.075f);
                    else
                        zoom = 0.4f;
                    CameraPanSystem.Zoom = zoom;
                    CameraPanSystem.PanTowards(new Vector2(ActivityCoords.X, ActivityCoords.Y - 120), zoom);
                    #endregion

                    ActivityTimer++;
                    if (State == SystemState.Ready)
                    {
                        ActivityTimer = -1;
                        return;
                    }
                    break;
                case SystemState.Ready:     
                    if(ActivityTimer == -1)
                    {
                        #region Activity Specific Setup
                        if (!TutorialComplete) //Orator Visit
                        {
                            ActivityCoords = new Point((CultBaseArea.Right - 11) * 16, (CultBaseArea.Top + 30) * 16);

                            if (!NPC.AnyNPCs(ModContent.NPCType<OratorNPC>()))
                                NPC.NewNPC(Entity.GetSource_None(), (CultBaseArea.Right - 11) * 16, (CultBaseArea.Top + 30) * 16, ModContent.NPCType<OratorNPC>(), ai0: 1);
                            else
                                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())].ai[0] = 1;
                            State = SystemState.OratorVisit;
                            Active = true;
                        }
                        else
                        {
                            if (true)//Main.moonPhase == (int)MoonPhase.Full || Main.moonPhase == (int)MoonPhase.Empty) //Ritual
                            {
                                #region Location Selection
                                ActivityCoords = new Point(LunarCultBaseLocation.X - 37, LunarCultBaseLocation.Y - 24);
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
                                    foreach(NPC npc in Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<OratorNPC>()))
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
                            }
                            else if (false)//Main.moonPhase == (int)MoonPhase.QuarterAtLeft || Main.moonPhase == (int)MoonPhase.QuarterAtRight) //Meeting
                            {
                                #region Location Selection
                                ActivityCoords = new Point(LunarCultBaseLocation.X - 48, LunarCultBaseLocation.Y + 18);
                                ActivityCoords.X *= 16;
                                ActivityCoords.X += 8;
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
                                                        Recruit.Recruitable = true;
                                                        break;
                                                    case 2:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                    case 3:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                        Recruit.Recruitable = true;
                                                        break;
                                                    case 4:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                }
                                                break;
                                            case MeetingTopic.Mission:
                                                switch (y)
                                                {
                                                    case 1:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                    case 2:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                        Recruit.Recruitable = true;
                                                        break;
                                                    case 3:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                    case 4:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                        Recruit.Recruitable = true;
                                                        break;
                                                }
                                                break;
                                            case MeetingTopic.Paradise:
                                                switch (y)
                                                {
                                                    case 1:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                        Recruit.Recruitable = true;
                                                        break;
                                                    case 2:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                        Recruit.Recruitable = true;
                                                        break;
                                                    case 3:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                    case 4:
                                                        Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tirith;
                                                        Recruit.Recruitable = false;
                                                        break;
                                                }
                                                break;
                                        }
                                    }
                                    y++;
                                }
                                #endregion
                            }
                            else if (false)//Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight) //Tailor
                            {
                                #region Location Selection
                                Vector2 seamstressCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<Seamstress>())].Center;
                                ActivityCoords = new((int)seamstressCenter.X, (int)seamstressCenter.Y);
                                #endregion

                                AssignedClothing = [];
                                CompletedClothesCount = 0;
                                ClothesGoal = 5 * (Main.player.Where(p => p.active).Count() + 1);
                            }
                            else //Cafeteria
                            {
                                Vector2 chefCenter = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())].Center;
                                ActivityCoords = new((int)chefCenter.X, (int)chefCenter.Y);

                                CustomerQueue = [];
                                SatisfiedCustomers = 0;
                            }
                        }
                        #endregion                        
                        ActivityTimer = 0;
                        zoom = 0;
                    }

                    //Main.NewText( Main.player[0].Center - new Vector2(ActivityCoords.X, ActivityCoords.Y));                  

                    #region Player Proximity
                    closestPlayer = Main.player[Player.FindClosest(new Vector2(ActivityCoords.X, ActivityCoords.Y), 300, 300)];
                    PlayerDistFromHideout = new Vector2(closestPlayer.Center.X - ActivityCoords.X, closestPlayer.Center.Y - ActivityCoords.Y).Length();

                    if (PlayerDistFromHideout < 300f && closestPlayer.Center.Y < ActivityCoords.Y)
                    {
                        /*
                        Actual Code
                        if (Main.moonPhase == (int)MoonPhase.Full || Main.moonPhase == (int)MoonPhase.Empty)
                            State = SystemState.Ritual;
                        else if (Main.moonPhase == (int)MoonPhase.QuarterAtLeft || Main.moonPhase == (int)MoonPhase.QuarterAtRight)
                            State = SystemState.Meeting;
                        else if (Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight)
                            State = SystemState.Tailor;
                        else
                            State = SystemState.Cafeteria;
                        */
                        //State = SystemState.Ritual;

                        //Active = true;
                    }
                    #endregion
                    #region Despawn  
                    else if (Main.dayTime && !Active && PlayerDistFromHideout > 4000f)
                    {
                        ActivityCoords = new(-1, -1);
                        State = SystemState.CheckReqs;
                        OnCooldown = true;
                        foreach (int k in NPCIndexs)
                        {
                            Main.npc[k].active = false;
                        }
                    }
                    #endregion
                    
                    break;
                case SystemState.OratorVisit:
                    if(TutorialComplete)
                    {
                        Active = false;
                        State = SystemState.End;
                    }
                    else
                        Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())].ai[0] = 1;
                    break;
                case SystemState.Meeting:                     
                    if (Active)
                    {
                        #region Meeting Scene
                        CombatText Text;

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

                        string key = $"Dialogue.LunarCult.CultMeetings.{CurrentMeetingTopic}.";

                        switch (CurrentMeetingTopic)
                        {
                            case MeetingTopic.CurrentEvents:
                                switch (ActivityTimer)
                                {
                                    case 1:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 0));
                                        Text.lifeTime = 60;
                                        break;
                                    case 90:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 1));
                                        Text.lifeTime = 60;
                                        break;
                                    case 3 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 2));
                                        break;
                                    case 6 * 60:
                                        Text = DisplayMessage(Cultist1Location, Color.Yellow, (key + 3));
                                        break;
                                    case 8 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 4));
                                        break;
                                    case 10 * 60:
                                        Text = DisplayMessage(Cultist1Location, Color.Yellow, "!?");
                                        DisplayMessage(Cultist2Location, Color.Red, "!?");
                                        DisplayMessage(Cultist3Location, Color.SandyBrown, "?");
                                        DisplayMessage(Cultist4Location, Color.Orange, "!?");
                                        break;
                                    case 12 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 5));
                                        break;
                                    case 14 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 6));
                                        break;
                                    case 16 * 60:
                                        Text = DisplayMessage(Cultist3Location, Color.SandyBrown, (key + 7));
                                        break;
                                    case 18 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 8));
                                        break;
                                    case 20 * 60:
                                        Text = DisplayMessage(Cultist3Location, Color.SandyBrown, (key + 9));
                                        break;
                                    case 22 * 60:
                                        Text = DisplayMessage(Cultist2Location, Color.Red, (key + 10));
                                        break;
                                    case 24 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 11));
                                        break;
                                    case 26 * 60:
                                        Text = DisplayMessage(Cultist2Location, Color.Red, (key + 12));
                                        break;
                                    case 28 * 60:
                                        Text = DisplayMessage(Cultist4Location, Color.Orange, (key + 13));
                                        break;
                                    case 30 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 14));
                                        break;
                                    case 32 * 60:
                                        Text = DisplayMessage(Cultist4Location, Color.Orange, (key + 15));
                                        break;
                                    case 34 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 16));
                                        break;
                                    case 36 * 60:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 17));
                                        break;
                                    case 37 * 60:
                                        Active = false;
                                        break;
                                }
                                break;
                            case MeetingTopic.Mission:
                                switch (ActivityTimer)
                                {
                                    case 1:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 0));
                                        Text.lifeTime = 60;
                                        break;
                                    case 90:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 1));
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 3:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 2));
                                        break;
                                    case 60 * 5:
                                        Active = false;
                                        break;
                                }
                                break;
                            case MeetingTopic.Paradise:
                                switch (ActivityTimer)
                                {
                                    case 1:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 0));
                                        Text.lifeTime = 60;
                                        break;
                                    case 90:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 1));
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 3:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 2));
                                        break;
                                    case 60 * 5:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 3));
                                        break;
                                    case 60 * 7:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 4));
                                        break;
                                    case 60 * 9:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 5));
                                        break;
                                    case 60 * 10:
                                        Text = DisplayMessage(Cultist1Location, Color.Red, (key + 6));
                                        break;
                                    case 60 * 11:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 7));
                                        break;
                                    case 60 * 13:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 8));
                                        break;
                                    case 60 * 15:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 9));
                                        break;
                                    case 60 * 17:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 10));
                                        break;
                                    case 60 * 19:
                                        Text = DisplayMessage(Cultist4Location, Color.Yellow, (key + 11));
                                        break;
                                    case 60 * 21:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 12));
                                        break;
                                    case 60 * 23:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 13));
                                        break;
                                    case 60 * 25:
                                        Text = DisplayMessage(Cultist2Location, Color.SeaGreen, (key + 14));
                                        break;
                                    case 60 * 27:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 15));
                                        break;
                                    case 60 * 29:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 16));
                                        break;
                                    case 60 * 31:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 17));
                                        break;
                                    case 60 * 33:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 18));
                                        break;
                                    case 60 * 35:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 19));
                                        break;
                                    case 60 * 37:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 20));
                                        break;
                                    case 60 * 39:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 21));
                                        break;
                                    case 60 * 41:
                                        Text = DisplayMessage(Cultist2Location, Color.SeaGreen, (key + 22));
                                        break;
                                    case 60 * 43:
                                        DisplayMessage(Cultist2Location, Color.SeaGreen, (key + 23));
                                        DisplayMessage(Cultist3Location, Color.Violet, (key + 24));
                                        break;
                                    case 60 * 45:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 25));
                                        break;
                                    case 60 * 47:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 26));
                                        break;
                                    case 60 * 49:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 27));
                                        break;
                                    case 60 * 51:
                                        Text = DisplayMessage(BishopLocation, Color.Blue, (key + 28));
                                        break;
                                    case 60 * 52:
                                        Active = false;
                                        break;
                                }
                                break;
                        }
                        #endregion

                        #region Camera Setup
                        if (ActivityTimer < 100)
                            zoom = Lerp(zoom, 0.4f, 0.075f);
                        else
                            zoom = 0.4f;
                        CameraPanSystem.Zoom = zoom;
                        CameraPanSystem.PanTowards(new Vector2(ActivityCoords.X, ActivityCoords.Y - 150), zoom);
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

                        State = SystemState.End;
                    }
                    break;
                case SystemState.Tailor:
                    if (!Active)
                        State = SystemState.End;
                    break;
                case SystemState.Cafeteria:
                    if(Active)
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
                            Active = false;
                            return;
                        }
                        ActivityTimer++;
                    }
                    else
                        State = SystemState.End;
                    break;
                case SystemState.Ritual:
                    //ActivePortals = 0;
                    if (PortalsDowned < RequiredPortalKills && RemainingCultists > 0 && ActivePortals < RemainingCultists && ActivityTimer >= 60 && Main.rand.NextBool(100)) //Spawn New Customer
                    {
                        Point spawnLocation = new(ActivityCoords.X + Main.rand.Next(-450, 450), ActivityCoords.Y - Main.rand.Next(120, 300));
                        NPC.NewNPC(NPC.GetSource_NaturalSpawn(), spawnLocation.X, spawnLocation.Y, ModContent.NPCType<PortalMole>(), Start: Main.npc[NPCIndexs[6]].whoAmI + 1);

                        ActivityTimer = 0;
                    }
                    else if (PortalsDowned >= RequiredPortalKills)
                    {
                        NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];                                               

                        switch(ActivityTimer)
                        {
                            case 0:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Success.0");
                                break;
                            case 120:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Success.1");
                                break;
                            case 240:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Success.2");
                                break;
                            case 360:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Success.3");
                                break;
                            case 420:
                                if (Main.npc[NPCIndexs[0]].active)
                                {
                                    for (int i = 0; i <= 50; i++)
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
                                    for (int i = 0; i <= 50; i++)
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
                            case 440:
                                if (Main.npc[NPCIndexs[1]].active)
                                {
                                    for (int i = 0; i <= 50; i++)
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
                                    for (int i = 0; i <= 50; i++)
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
                            case 460:
                                if (Main.npc[NPCIndexs[2]].active)
                                {
                                    for (int i = 0; i <= 50; i++)
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
                                    for (int i = 0; i <= 50; i++)
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
                            case 480:
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(2f, 2.5f);
                                    Dust dust = Dust.NewDustPerfect(orator.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, orator.Center);
                                
                                Main.npc[NPCIndexs[6]].active = false;
                                
                                NPCIndexs = [];
                                Active = false;
                                break;
                        }

                        ActivityTimer++;
                    }
                    else if(RemainingCultists <= 0)
                    {
                        NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];

                        switch (ActivityTimer)
                        {
                            case 0:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Failure.0");
                                break;
                            case 120:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Failure.1");
                                break;
                            case 240:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Failure.2");
                                break;
                            case 360:
                                DisplayMessage(orator.Hitbox, Color.LimeGreen, "Dialogue.LunarCult.TheOrator.WorldText.Ritual.Failure.3");
                                break;
                            case 420:
                                for (int i = 0; i <= 50; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Vector2 speed = Main.rand.NextVector2Circular(2f, 2.5f);
                                    Dust dust = Dust.NewDustPerfect(orator.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                                SoundEngine.PlaySound(LunarCultistDevotee.SpawnSound, orator.Center);

                                Main.npc[NPCIndexs[6]].active = false;

                                NPCIndexs = [];
                                Active = false;
                                break;
                        }
                        ActivityTimer++;
                    }
                    else
                        ActivityTimer++;
                    if(!Active)
                        State = SystemState.End;
                    break;
                case SystemState.End:
                    ActivityCoords = new(-1, -1);
                    OnCooldown = true;
                    int currentRecruitCount = Main.npc.Where(n => n.active && n.type == ModContent.NPCType<RecruitableLunarCultist>()).Count();
                    if (currentRecruitCount < 4)
                    {
                        List<string> names =
                        [
                            "Tirith",
                            "Vivian",
                            "Tania",
                            "Doro",
                            "Skylar",
                            "Jamie",
                        ];
                        int availableNames = 6;
                        for (int i = 0; i < 4 - currentRecruitCount; i++)
                        {
                            NPC recruit;
                            int index = Main.rand.Next(availableNames);
                            if (NPC.FindFirstNPC(ModContent.NPCType<LunarCultistDevotee>()) != -1)
                            {
                                recruit = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarCultistDevotee>())];
                                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarCultistDevotee>())].Transform(ModContent.NPCType<RecruitableLunarCultist>());
                                recruit.As<RecruitableLunarCultist>().MyName = (RecruitableLunarCultist.RecruitNames)index;
                                recruit.ModNPC.OnSpawn(NPC.GetSource_NaturalSpawn());
                            }
                            else
                            {
                                recruit = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarCultistArcher>())];
                                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarCultistArcher>())].Transform(ModContent.NPCType<RecruitableLunarCultist>());
                                recruit.As<RecruitableLunarCultist>().MyName = (RecruitableLunarCultist.RecruitNames)index;
                                recruit.ModNPC.OnSpawn(NPC.GetSource_NaturalSpawn());
                            }
                            names.RemoveAt(index);
                        }
                    }
                    State = SystemState.CheckReqs;
                    break;
            }
        }
        internal static CombatText DisplayMessage(Rectangle location, Color color, string text)
        {
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, GetWindfallTextValue(text), true)];
            return MyDialogue;
        }
        public static bool IsTailorActivityActive() => Active && State == SystemState.Tailor;
        public static bool IsCafeteriaActivityActive() => Active && State == SystemState.Cafeteria;
        public static bool IsRitualActivityActive() => Active && State == SystemState.Ritual;
        public static Response[] GetMenuResponses()
        {
            Response[] Responses = new Response[MenuFoodIDs.Count];
            for (int i = 0; i < MenuFoodIDs.Count; i++)
            {
                Item item = new(MenuFoodIDs[i]);
                Responses[i] = new Response(item.Name, localize: false);
            }
            return Responses;
        }
        public static void ResetTimer()
        {
            ActivityTimer = 0;
        }

        public static void AttemptToCultBaseDenizens()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;          

            for (int i = 0; i < 16; i++)
            {
                Rectangle spawnArea = new((LunarCultBaseLocation.X - 70), (LunarCultBaseLocation.Y - 80), 82, 152);
                int checkPositionX = spawnArea.X + Main.rand.Next(spawnArea.Width);
                int checkPositionY = spawnArea.Y + Main.rand.Next(spawnArea.Height);
                Vector2 checkPosition = new(checkPositionX, checkPositionY);

                Tile aboveSpawnTile = CalamityUtils.ParanoidTileRetrieval(checkPositionX, checkPositionY - 1);
                bool nearCultBase = CalamityUtils.ManhattanDistance(checkPosition, new(LunarCultBaseLocation.X, LunarCultBaseLocation.Y)) < 180f;
                bool isVaildWall = aboveSpawnTile.WallType == WallID.AncientSilverBrickWall || aboveSpawnTile.WallType == WallID.GreenStainedGlass || aboveSpawnTile.WallType == WallID.EmeraldGemspark;
                isVaildWall |= aboveSpawnTile.WallType == WallID.PlatinumBrick || aboveSpawnTile.WallType == WallID.PearlstoneBrick;
                if (!isVaildWall || !nearCultBase || Collision.SolidCollision((checkPosition - new Vector2(2f, 4f)).ToWorldCoordinates(), 4, 8) || aboveSpawnTile.IsTileSolid() || Lighting.Brightness(checkPositionX, checkPositionY - 1) <= 0.4f)
                    continue;

                WeightedRandom<int> pool = new();
                pool.Add(NPCID.None, 0f);
                pool.Add(ModContent.NPCType<Fingerling>(), 0.05f);
                pool.Add(ModContent.NPCType<LunarCultistArcher>(), 0.025f);
                pool.Add(ModContent.NPCType<LunarCultistDevotee>(), 0.05f);

                int typeToSpawn = pool.Get();
                if (typeToSpawn != NPCID.None)
                {
                    int spawnedNPC = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), checkPositionX * 16 + 8, checkPositionY * 16, typeToSpawn, ai2: typeToSpawn == ModContent.NPCType<LunarCultistDevotee>() ? 4 : 3);

                    if (Main.netMode == NetmodeID.Server && spawnedNPC < Main.maxNPCs)
                    {
                        Main.npc[spawnedNPC].position.Y -= 8f;
                        
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedNPC);
                        return;
                    }
                }
            }
        }
    }
}

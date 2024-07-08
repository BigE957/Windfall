using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Content.Items.Quest;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Common.Systems.WorldEvents
{
    public class CultMeetingSystem : ModSystem
    {
        public static Point SolarHideoutLocation;
        public static Point VortexHideoutLocation;
        public static Point NebulaHideoutLocation;
        public static Point StardustHideoutLocation;

        public static List<int> Recruits = [];

        private static List<int> AvailableTopics = [];

        public override void ClearWorld()
        {
            SolarHideoutLocation = new(-1, -1);
            VortexHideoutLocation = new(-1, -1);
            NebulaHideoutLocation = new(-1, -1);
            StardustHideoutLocation = new(-1, -1);

            Recruits = [];

            AvailableTopics = [];
        }
        public override void LoadWorldData(TagCompound tag)
        {
            SolarHideoutLocation = tag.Get<Point>("SolarHideoutLocation");
            VortexHideoutLocation = tag.Get<Point>("VortexHideoutLocation");
            NebulaHideoutLocation = tag.Get<Point>("NebulaHideoutLocation");
            StardustHideoutLocation = tag.Get<Point>("StardustHideoutLocation");

            Recruits = (List<int>)tag.GetList<int>("Recruits");

            AvailableTopics = (List<int>)tag.GetList<int>("AvailableTopics");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["SolarHideoutLocation"] = SolarHideoutLocation;
            tag["VortexHideoutLocation"] = VortexHideoutLocation;
            tag["NebulaHideoutLocation"] = NebulaHideoutLocation;
            tag["StardustHideoutLocation"] = StardustHideoutLocation;

            tag["Recruits"] = Recruits;

            tag["AvailableTopics"] = AvailableTopics;
        }

        private enum SystemState
        {
            CheckReqs,
            CheckChance,
            Spawn,
            End,
        }
        private SystemState State = SystemState.CheckReqs;

        private static bool OnCooldown = true;
        private static bool Active = false;
        public static Point ActiveHideoutCoords = new(-1, -1);
        private static int MeetingTimer = -1;
        public enum MeetingTopic
        {
            CurrentEvents,
            Mission,
            Paradise
        }
        public static MeetingTopic CurrentMeetingTopic;
        private static List<int> NPCIndexs = [];
        private static float zoom = 0;
        private static SoundStyle TeleportSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (!NPC.downedPlantBoss || Recruits.Count == 2 || OnCooldown || Main.dayTime)
                    {
                        if (Main.dayTime)
                        {
                            OnCooldown = false;
                            Active = false;
                            ActiveHideoutCoords = new(-1, -1);
                            MeetingTimer = -1;
                        }
                        break;
                    }
                    else
                    {
                        State = SystemState.CheckChance;
                        break;
                    }
                case SystemState.CheckChance:
                    if (Main.rand.NextBool(5))
                        State = SystemState.Spawn;
                    else
                    {
                        OnCooldown = true;
                        State = SystemState.CheckReqs;
                    }
                    break;
                case SystemState.Spawn:
                    if (MeetingTimer == -1)
                    {
                        #region Location Selection
                        int i = Main.rand.Next(4);
                        switch (i)
                        {
                            case 0:
                                ActiveHideoutCoords = SolarHideoutLocation;
                                break;
                            case 1:
                                ActiveHideoutCoords = VortexHideoutLocation;
                                break;
                            case 2:
                                ActiveHideoutCoords = NebulaHideoutLocation;
                                break;
                            case 3:
                                ActiveHideoutCoords = StardustHideoutLocation;
                                break;
                        }
                        ActiveHideoutCoords.X *= 16;
                        ActiveHideoutCoords.Y *= 16;
                        #endregion

                        foreach (Player player in Main.player)
                        {
                            if (player.InventoryHas(ModContent.ItemType<SelenicTablet>()))
                            {
                                Main.NewText("The Selenic Tablet begins to hum...", Color.Cyan);
                                break;
                            }
                        }
                        zoom = 0;

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
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X, ActiveHideoutCoords.Y - 2, ModContent.NPCType<LunarBishop>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X - 240, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X - 130, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X + 130, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X + 240, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                        ];
                                            
                        foreach (int k in NPCIndexs)
                        {
                            NPC npc = Main.npc[k];
                            if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                            {
                                switch (CurrentMeetingTopic)
                                {
                                    case MeetingTopic.CurrentEvents:
                                        switch (k)
                                        {
                                            case 1:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tirith;
                                                Recruit.Recruitable = true;
                                                npc.direction = 1;
                                                break;
                                            case 2:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                Recruit.Recruitable = false;
                                                npc.direction = 1;
                                                break;
                                            case 3:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                Recruit.Recruitable = true;
                                                npc.direction = -1;
                                                break;
                                            case 4:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                Recruit.Recruitable = false;
                                                npc.direction = -1;
                                                break;
                                        }
                                        break;
                                    case MeetingTopic.Mission:
                                        switch (k)
                                        {
                                            case 1:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Doro;
                                                Recruit.Recruitable = false;
                                                npc.direction = 1;
                                                break;
                                            case 2:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                Recruit.Recruitable = true;
                                                npc.direction = 1;
                                                break;
                                            case 3:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                Recruit.Recruitable = false;
                                                npc.direction = -1;
                                                break;
                                            case 4:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Jamie;
                                                Recruit.Recruitable = true;
                                                npc.direction = -1;
                                                break;
                                        }
                                        break;
                                    case MeetingTopic.Paradise:
                                        switch (k)
                                        {
                                            case 1:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Vivian;
                                                Recruit.Recruitable = true;
                                                npc.direction = 1;
                                                break;
                                            case 2:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tania;
                                                Recruit.Recruitable = true;
                                                npc.direction = 1;
                                                break;
                                            case 3:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Skylar;
                                                Recruit.Recruitable = false;
                                                npc.direction = -1;
                                                break;
                                            case 4:
                                                Recruit.MyName = RecruitableLunarCultist.RecruitNames.Tirith;
                                                Recruit.Recruitable = false;
                                                npc.direction = -1;
                                                break;
                                        }
                                        break;
                                }
                            }
                        }                        
                        #endregion

                        MeetingTimer = 0;
                    }
                    else
                    {
                        #region Despawn
                        if (Main.dayTime)
                        {
                            ActiveHideoutCoords = new(-1, -1);
                            State = SystemState.CheckReqs;
                            OnCooldown = true;
                            foreach (int k in NPCIndexs)
                            {
                                NPC npc = Main.npc[k];
                                if (npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                                    npc.active = false;
                                else if (npc.type == ModContent.NPCType<LunarBishop>())
                                    npc.active = false;
                            }
                        }
                        #endregion

                        float PlayerDistFromHideout = new Vector2(mainPlayer.Center.X - ActiveHideoutCoords.X, mainPlayer.Center.Y - ActiveHideoutCoords.Y).Length();
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
                                    switch (MeetingTimer)
                                    {
                                        case 1:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 0));
                                            Text.lifeTime = 60;
                                            break;
                                        case 90:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 1));
                                            Text.lifeTime = 60;
                                            break;
                                        case 3 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 2));
                                            break;
                                        case 6 * 60:
                                            Text = DisplayMessage(Cultist1Location, Color.Yellow, GetWindfallTextValue(key + 3));
                                            break;
                                        case 8 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 4));
                                            break;
                                        case 10 * 60:
                                            Text = Main.combatText[CombatText.NewText(Cultist1Location, Color.Yellow, "!?", true)];
                                            DisplayMessage(Cultist2Location, Color.Red, "!?");
                                            DisplayMessage(Cultist3Location, Color.SandyBrown, "?");
                                            DisplayMessage(Cultist4Location, Color.Orange, "!?");
                                            break;
                                        case 12 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 5));
                                            break;
                                        case 14 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 6));
                                            break;
                                        case 16 * 60:
                                            Text = DisplayMessage(Cultist3Location, Color.SandyBrown, GetWindfallTextValue(key + 7));
                                            break;
                                        case 18 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 8));
                                            break;
                                        case 20 * 60:
                                            Text = DisplayMessage(Cultist3Location, Color.SandyBrown, GetWindfallTextValue(key + 9));
                                            break;
                                        case 22 * 60:
                                            Text = DisplayMessage(Cultist2Location, Color.Red, GetWindfallTextValue(key + 10));
                                            break;
                                        case 24 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 11));
                                            break;
                                        case 26 * 60:
                                            Text = DisplayMessage(Cultist2Location, Color.Red, GetWindfallTextValue(key + 12));
                                            break;
                                        case 28 * 60:
                                            Text = DisplayMessage(Cultist4Location, Color.Orange, GetWindfallTextValue(key + 13));
                                            break;
                                        case 30 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 14));
                                            break;
                                        case 32 * 60:
                                            Text = DisplayMessage(Cultist4Location, Color.Orange, GetWindfallTextValue(key + 15));
                                            break;
                                        case 34 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 16));
                                            break;
                                        case 36 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 17));
                                            break;
                                        case 37 * 60:
                                            State = SystemState.End;
                                            break;
                                    }
                                    break;
                                case MeetingTopic.Mission:
                                    switch (MeetingTimer)
                                    {
                                        case 1:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 0));
                                            Text.lifeTime = 60;
                                            break;
                                        case 90:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 1));
                                            Text.lifeTime = 60;
                                            break;
                                        case 60 * 3:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 2));
                                            break;
                                    }
                                    break;
                                case MeetingTopic.Paradise:
                                    switch (MeetingTimer)
                                    {
                                        case 1:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 0));
                                            Text.lifeTime = 60;
                                            break;
                                        case 90:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 1));
                                            Text.lifeTime = 60;
                                            break;
                                        case 60 * 3:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 2));
                                            break;
                                        case 60 * 5:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 3));
                                            break;
                                        case 60 * 7:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 4));
                                            break;
                                        case 60 * 9:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 5));
                                            break;
                                        case 60 * 10:
                                            Text = DisplayMessage(Cultist1Location, Color.Red, GetWindfallTextValue(key + 6));
                                            break;
                                        case 60 * 11:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 7));
                                            break;
                                        case 60 * 13:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 8));
                                            break;
                                        case 60 * 15:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 9));
                                            break;
                                        case 60 * 17:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 10));
                                            break;
                                        case 60 * 19:
                                            Text = DisplayMessage(Cultist4Location, Color.Yellow, GetWindfallTextValue(key + 11));
                                            break;
                                        case 60 * 21:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 12));
                                            break;
                                        case 60 * 23:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 13));
                                            break;
                                        case 60 * 25:
                                            Text = DisplayMessage(Cultist2Location, Color.SeaGreen, GetWindfallTextValue(key + 14));
                                            break;
                                        case 60 * 27:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 15));
                                            break;
                                        case 60 * 29:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 16));
                                            break;
                                        case 60 * 31:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 17));
                                            break;
                                        case 60 * 33:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 18));
                                            break;
                                        case 60 * 35:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 19));
                                            break;
                                        case 60 * 37:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 20));
                                            break;
                                        case 60 * 39:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 21));
                                            break;
                                        case 60 * 41:
                                            Text = DisplayMessage(Cultist2Location, Color.SeaGreen, GetWindfallTextValue(key + 22));
                                            break;
                                        case 60 * 43:
                                            DisplayMessage(Cultist2Location, Color.SeaGreen, GetWindfallTextValue(key + 23));
                                            DisplayMessage(Cultist3Location, Color.Violet, GetWindfallTextValue(key + 24));
                                            break;
                                        case 60 * 45:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 25));
                                            break;
                                        case 60 * 47:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 26));
                                            break;
                                        case 60 * 49:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 27));
                                            break;
                                        case 60 * 51:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, GetWindfallTextValue(key + 28));
                                            break;
                                        case 60 * 52:
                                            State = SystemState.End;
                                            break;
                                    }
                                    break;
                            }
                            #endregion

                            #region Camera Setup
                            if (MeetingTimer < 100)
                                zoom = Lerp(zoom, 0.4f, 0.075f);
                            else
                                zoom = 0.4f;
                            CameraPanSystem.Zoom = zoom;
                            CameraPanSystem.PanTowards(new Vector2(ActiveHideoutCoords.X, ActiveHideoutCoords.Y - 150), zoom);
                            #endregion

                            MeetingTimer++;
                        }
                        else if (PlayerDistFromHideout < 300f)
                            Active = true;
                    }
                    break;
                case SystemState.End:
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
                    ActiveHideoutCoords = new(-1, -1);
                    OnCooldown = true;
                    State = SystemState.CheckReqs;
                    break;
            }
        }
        internal static CombatText DisplayMessage(Rectangle location, Color color, string text)
        {
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, text, true)];
            return MyDialogue;
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using Terraria.ModLoader.IO;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Terraria.ID;
using Terraria.Audio;
using CalamityMod.Systems;
using CalamityMod;
using Windfall.Content.Items.Quest;
using System.Diagnostics.Metrics;

namespace Windfall.Common.Systems.WorldEvents
{
    public class CultMeetingSystem : ModSystem
    {
        public static Point SolarHideoutLocation;
        public static Point VortexHideoutLocation;
        public static Point NebulaHideoutLocation;
        public static Point StardustHideoutLocation;

        public static List<int> Recruits = new();

        private static List<int> AvailableTopics = new();

        public override void ClearWorld()
        {
            SolarHideoutLocation = new(-1, -1);
            VortexHideoutLocation = new(-1, -1);
            NebulaHideoutLocation = new(-1, -1);
            StardustHideoutLocation = new(-1, -1);

            Recruits = new List<int>();

            AvailableTopics = new();
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
            Gooning,
            Mewing
        }
        public static MeetingTopic CurrentMeetingTopic;
        private static List<int> NPCIndexs = new();
        private static float zoom = 0;
        private static SoundStyle TeleportSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (!NPC.downedPlantBoss || NPC.downedAncientCultist || OnCooldown || Main.dayTime)
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
                        int i = Main.rand.Next(3);
                        switch(i)
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

                        foreach (Player player in Main.player)
                        {
                            if (player.InventoryHas(ModContent.ItemType<WFEidolonTablet>()))
                            {
                                Main.NewText("The Selenic Tablet begins to hum...", Color.Cyan);
                                break;
                            }
                        }
                        zoom = 0;
                        if (AvailableTopics.Count == 0)
                            for(int h = 0; h < MeetingTopic.GetNames(typeof(MeetingTopic)).Length; h++)
                                AvailableTopics.Add(h);

                        CurrentMeetingTopic = MeetingTopic.CurrentEvents; //(MeetingTopic)AvailableTopics[Main.rand.Next(AvailableTopics.Count)]; Actual Code
                        AvailableTopics.Remove((int)CurrentMeetingTopic);

                        NPCIndexs = new List<int>
                        {
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X, ActiveHideoutCoords.Y - 2, ModContent.NPCType<LunarBishop>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X - 240, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X - 130, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X + 130, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X + 240, ActiveHideoutCoords.Y + 100, ModContent.NPCType<RecruitableLunarCultist>()),
                        };

                        #region Character Setup
                        switch (CurrentMeetingTopic)
                        {
                            case MeetingTopic.CurrentEvents:
                                foreach (int k in NPCIndexs)
                                {
                                    NPC npc = Main.npc[k];
                                    if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                                    {
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
                                    }
                                }
                                break;
                        }
                        #endregion
                        MeetingTimer = 0;
                    }
                    else
                    {
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
                        float PlayerDistFromHideout = new Vector2(mainPlayer.Center.X - ActiveHideoutCoords.X, mainPlayer.Center.Y - ActiveHideoutCoords.Y).Length();
                        if (Active)
                        {
                            #region Meeting Dialogue
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

                            switch (CurrentMeetingTopic)
                            {
                                case MeetingTopic.CurrentEvents:
                                    switch (MeetingTimer)
                                    {
                                        case 1:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Greetings!");
                                            break;
                                        case 90:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Let us begin");
                                            break;
                                        case 3 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "I'm sure you are all aware of some... recent developments.");
                                            break;
                                        case 6 * 60:
                                            Text = DisplayMessage(Cultist1Location, Color.Yellow, "Yeah, what's really going on?!");
                                            break;
                                        case 8 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "The end of our journey together is fast approaching!");
                                            break;
                                        case 10 * 60:
                                            Text = Main.combatText[CombatText.NewText(Cultist1Location, Color.Yellow, "!?", true)];
                                            DisplayMessage(Cultist2Location, Color.Red, "!?");
                                            DisplayMessage(Cultist3Location, Color.Brown, "?");
                                            DisplayMessage(Cultist4Location, Color.Orange, "!?");
                                            break;
                                        case 12 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Yes! Our goals are quickly becoming in reach.");
                                            break;
                                        case 14 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "The time shall come when all of you will be called upon to play your part.");
                                            break;
                                        case 16 * 60:
                                            Text = DisplayMessage(Cultist3Location, Color.Brown, "Uhm... What are our 'parts'?");
                                            break;
                                        case 18 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "That will be revealed to you in due time.");
                                            break;
                                        case 20 * 60:
                                            Text = DisplayMessage(Cultist3Location, Color.Brown, "Oh... Okay.");
                                            break;
                                        case 22 * 60:
                                            Text = DisplayMessage(Cultist2Location, Color.Red, "Does this mean we've learned all there is to learn?");
                                            break;
                                        case 24 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Not at all. In the coming days, much more shall be revealed to you.");
                                            break;
                                        case 26 * 60:
                                            Text = DisplayMessage(Cultist2Location, Color.Red, "I see...");
                                            break;
                                        case 28 * 60:
                                            Text = DisplayMessage(Cultist4Location, Color.Orange, "Will we see the Orator?");
                                            break;
                                        case 30 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "It is very possible he might grace us with his prescence as the time nears.");
                                            break;
                                        case 32 * 60:
                                            Text = DisplayMessage(Cultist4Location, Color.Orange, "Awesome!");
                                            break;
                                        case 34 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Hold steady your faith. It shall very soon be rewarded.");
                                            break;
                                        case 36 * 60:
                                            Text = DisplayMessage(BishopLocation, Color.Blue, "Until we next meet.");
                                            break;
                                        case 37 * 60:
                                            State = SystemState.End;
                                            break;
                                    }
                                    break;
                                case MeetingTopic.Gooning:
                                    break;
                                case MeetingTopic.Mewing:
                                    break;
                            }
                            #endregion

                            Vector2 LerpLocation = Vector2.Zero;
                            if (MeetingTimer < 100)
                                zoom = MathHelper.Lerp(zoom, 0.4f, 0.075f);
                            else
                                zoom = 0.4f;
                            ZoomSystem.SetZoomEffect(zoom);
                            Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = new(ActiveHideoutCoords.X, ActiveHideoutCoords.Y - 150);
                            Main.LocalPlayer.Windfall_Camera().ScreenFocusInterpolant = zoom;

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
                                Recruit.chattable = true;
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

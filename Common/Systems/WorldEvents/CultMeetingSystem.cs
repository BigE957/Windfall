using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using Windfall.Content.Projectiles.NPCAnimations;
using Terraria.ModLoader.IO;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;
using Terraria.ID;
using Terraria.Audio;
using CalamityMod.NPCs.SunkenSea;

namespace Windfall.Common.Systems.WorldEvents
{
    public class CultMeetingSystem : ModSystem
    {
        public static Point SolarHideoutLocation;
        public static Point VortexHideoutLocation;
        public static Point NebulaHideoutLocation;
        public static Point StardustHideoutLocation;

        public static List<int> Recruits = new();

        public override void ClearWorld()
        {
            SolarHideoutLocation = new(-1, -1);
            VortexHideoutLocation = new(-1, -1);
            NebulaHideoutLocation = new(-1, -1);
            StardustHideoutLocation = new(-1, -1);

            Recruits = new List<int>();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            SolarHideoutLocation = tag.Get<Point>("SolarHideoutLocation");
            VortexHideoutLocation = tag.Get<Point>("VortexHideoutLocation");
            NebulaHideoutLocation = tag.Get<Point>("NebulaHideoutLocation");
            StardustHideoutLocation = tag.Get<Point>("StardustHideoutLocation");

            Recruits = (List<int>)tag.GetList<int>("Recruits");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["SolarHideoutLocation"] = SolarHideoutLocation;
            tag["VortexHideoutLocation"] = VortexHideoutLocation;
            tag["NebulaHideoutLocation"] = NebulaHideoutLocation;
            tag["StardustHideoutLocation"] = StardustHideoutLocation;

            tag["Recruits"] = Recruits;
        }

        private enum SystemState
        {
            CheckReqs,
            CheckChance,
            Spawn,
            End,
        }
        private SystemState State = SystemState.CheckReqs;

        private bool OnCooldown = true;
        private bool Active = false;
        private Point ActiveHideoutCoords = new(-1, -1);
        private int MeetingTimer = -1;
        private enum MeetingTopic
        {
            Jelqing,
            Gooning,
            Mewing
        }
        private MeetingTopic CurrentMeetingTopic;
        private List<int> NPCIndexs = new();
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
                    {
                        State = SystemState.Spawn;
                    }
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
                                Main.NewText("A Lunar Cult Meeting has begun at the Solar Hideout!", Color.Blue);
                                ActiveHideoutCoords = SolarHideoutLocation;
                                break;
                            case 1:
                                Main.NewText("A Lunar Cult Meeting has begun at the Vortex Hideout!", Color.Blue);
                                ActiveHideoutCoords = VortexHideoutLocation;
                                break;
                            case 2:
                                Main.NewText("A Lunar Cult Meeting has begun at the Nebula Hideout!", Color.Blue);
                                ActiveHideoutCoords = NebulaHideoutLocation;
                                break;
                            case 3:
                                Main.NewText("A Lunar Cult Meeting has begun at the Stardust Hideout!", Color.Blue);
                                ActiveHideoutCoords = StardustHideoutLocation;
                                break;
                        }
                        ActiveHideoutCoords.X *= 16;
                        ActiveHideoutCoords.Y *= 16;
                        CurrentMeetingTopic = MeetingTopic.Jelqing; //(MeetingTopic)Main.rand.Next(MeetingTopic.GetNames(typeof(MeetingTopic)).Length);
                        NPCIndexs = new List<int>
                        {
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X, ActiveHideoutCoords.Y, ModContent.NPCType<LunarBishop>()),
                            NPC.NewNPC(Entity.GetSource_None(), ActiveHideoutCoords.X - 120, ActiveHideoutCoords.Y, ModContent.NPCType<RecruitableLunarCultist>())
                        };
                        MeetingTimer = 0;
                    }
                    else
                    {
                        if (Main.dayTime)
                        {
                            Main.NewText("The Lunar Cult Meeting has ended...", Color.Blue);
                            State = SystemState.CheckReqs;
                            OnCooldown = true;
                            break;
                        }
                        float PlayerDistFromHideout = new Vector2(mainPlayer.Center.X - ActiveHideoutCoords.X, mainPlayer.Center.Y - ActiveHideoutCoords.Y).Length();
                        if (Active)
                        {
                            //Cult Meeting Code goes here:
                            CombatText Text;
                            NPC Bishop = Main.npc[NPCIndexs[0]];
                            NPC Cultist1 = Main.npc[NPCIndexs[1]];
                            //NPC Cultist2 = Main.npc[NPCIndexs[2]];
                            //NPC Cultist3 = Main.npc[NPCIndexs[3]];
                            //NPC Cultist4 = Main.npc[NPCIndexs[4]];
                            switch (CurrentMeetingTopic)
                            {
                                case (MeetingTopic.Jelqing):
                                    Rectangle BishopLocation = new((int)Bishop.Center.X, (int)Bishop.Center.Y, Bishop.width, Bishop.width);
                                    Rectangle Cultist1Location = new((int)Cultist1.Center.X, (int)Cultist1.Center.Y, Cultist1.width, Cultist1.width);
                                    switch (MeetingTimer)
                                    {
                                        case 1:
                                            Text = Main.combatText[CombatText.NewText(BishopLocation, Color.Blue, "Howdy!", true)];
                                            break;
                                        case 1 * 60:
                                            Text = Main.combatText[CombatText.NewText(BishopLocation, Color.Blue, "Jelqing is very poggers!", true)];
                                            break;
                                        case 3 * 60:
                                            Text = Main.combatText[CombatText.NewText(BishopLocation, Color.Blue, "Jelq often!", true)];
                                            break;
                                        case 6 * 60:
                                            Text = Main.combatText[CombatText.NewText(Cultist1Location, Color.Blue, "Uh... What if my dick gets too big?", true)];
                                            break;
                                        case 8 * 60:
                                            Text = Main.combatText[CombatText.NewText(BishopLocation, Color.Blue, "NOT POSSIBLE.", true)];
                                            break;
                                        case 10 * 60:
                                            Text = Main.combatText[CombatText.NewText(BishopLocation, Color.Blue, "Meeting over.", true)];
                                            State = SystemState.End;
                                            break;
                                    }
                                    break;
                                case (MeetingTopic.Gooning):
                                    break;
                                case (MeetingTopic.Mewing):

                                    break;
                            }

                            MeetingTimer++;
                        }
                        else if (PlayerDistFromHideout < 300f)
                        {
                            Main.NewText("You've arrived!", Color.Blue);
                            Active = true;
                        }
                        else
                            Main.NewText("Too far!", Color.Blue);
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
                            {
                                Recruit.chattable = true;
                            }
                        }
                    }
                    OnCooldown = true;
                    State = SystemState.CheckReqs;
                    break;
            }
        }
    }
}

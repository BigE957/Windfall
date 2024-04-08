using Terraria;
using Windfall.Content.NPCs.Bosses.TheOrator;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using static System.Net.Mime.MediaTypeNames;
using static Windfall.Common.Systems.WorldEvents.CultMeetingSystem;

namespace Windfall.Common.Systems.WorldEvents
{
    public class SealingRitualSystem : ModSystem
    {
        private enum SystemState
        {
            CheckReqs,
            CheckChance,
            Spawn,
            End,
        }
        private SystemState State = SystemState.CheckReqs;

        private static bool OnCooldown = false;
        private static bool Active = false;
        private static int RitualTimer = -1;
        private static List<int> NPCIndexs = new();
        private static float zoom = 0;
        private static SoundStyle TeleportSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private static Vector2 DungeonCoords = new Vector2(Main.dungeonX - 5, Main.dungeonY).ToWorldCoordinates();
        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            //State = SystemState.CheckReqs;
            //RitualTimer = -1;
            //OnCooldown = false;
            //DisplayLocalizedText($"{Active}");
            //DungeonCoords = new Vector2(Main.dungeonX - 4, Main.dungeonY).ToWorldCoordinates();
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (TravellingCultist.RitualQuestProgress != 4 || NPC.downedAncientCultist || OnCooldown)
                        return;
                    else
                    {
                        RitualTimer = -1;
                        State = SystemState.Spawn;
                        break;
                    }
                case SystemState.Spawn:
                    if (RitualTimer == -1)
                    {
                        Active = false;
                        zoom = 0;
                        NPCIndexs = new List<int>
                        {
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X - 240), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X - 130), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            //NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X + 130), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            //NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X + 240), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y, ModContent.NPCType<TravellingCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 112, ModContent.NPCType<SealingTablet>()),
                        };

                        #region Character Setup
                        for (int k = 0; k < 2; k++)
                        {
                            NPC npc = Main.npc[k];
                            if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>() && k != 0)
                            {
                                Recruit.MyName = (RecruitableLunarCultist.RecruitNames) Recruits[k];
                            }
                        }
                        zoom = 0;
                        #endregion                       
                    }
                    else
                    {
                        if ((DungeonCoords - mainPlayer.Center).Length() < 150f && !Active)
                        {
                            Active = true;
                            RitualTimer = 0;
                            zoom = 0;
                        }
                        if (Active)
                        {
                            NPC Recruit1 = Main.npc[NPCIndexs[0]];
                            NPC Recruit2 = Main.npc[NPCIndexs[1]];
                            //NPC Recruit3 = Main.npc[NPCIndexs[2]];
                            //NPC Recruit4 = Main.npc[NPCIndexs[3]];
                            NPC LunaticCultist = Main.npc[NPCIndexs[2]];

                            Rectangle Recruit1Location = new((int)Recruit1.Center.X, (int)Recruit1.Center.Y, Recruit1.width, Recruit1.width);
                            Rectangle Recruit2Location = new((int)Recruit2.Center.X, (int)Recruit2.Center.Y, Recruit2.width, Recruit2.width);
                            //Rectangle Cultist3Location = new((int)Recruit3.Center.X, (int)Recruit3.Center.Y, Recruit3.width, Recruit3.width);
                            //Rectangle Cultist4Location = new((int)Recruit4.Center.X, (int)Recruit4.Center.Y, Recruit4.width, Recruit4.width);
                            Rectangle LunaticLocation = new((int)LunaticCultist.Center.X, (int)LunaticCultist.Center.Y, LunaticCultist.width, LunaticCultist.width);

                            CombatText Text;
                            switch (RitualTimer)
                            {
                                case 1:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "Ah! You're finally here!");
                                    break;
                                case 90:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "We can finally get this thing started!");
                                    break;
                                case 60 * 3:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "...");
                                    break;
                                case 60 * 5:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "Uh, the animation budget's a bit low at the moment...");
                                    break;
                                case 60 * 7:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "Just imagine smth cool happening right now.");
                                    break;
                                case 60 * 10:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "Uh oh, guys! Who's that over there?!");
                                    break;
                                case 60 * 12:
                                    Text = DisplayMessage(LunaticLocation, Color.Blue, "It's the Orator!!!!");
                                    SoundEngine.PlaySound(SoundID.Roar, mainPlayer.Center);
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        NPC.SpawnOnPlayer(mainPlayer.whoAmI, ModContent.NPCType<TheOrator>());
                                    else
                                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, mainPlayer.whoAmI, ModContent.NPCType<TheOrator>());
                                    State = SystemState.End;
                                    break;
                            }                            
                            if (RitualTimer < 30)
                                zoom = Lerp(zoom, 0.4f, 0.075f);
                            else
                                zoom = 0.4f;
                            ZoomSystem.SetZoomEffect(zoom);
                            Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = new(LunaticLocation.X, LunaticLocation.Y - 150);
                            Main.LocalPlayer.Windfall_Camera().ScreenFocusInterpolant = zoom;
                        }
                    }
                    RitualTimer++;
                    break;
                case SystemState.End:
                    Active = false;
                    /*
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
                    */
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

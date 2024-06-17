using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Items.Quest;
using Windfall.Content.NPCs.Bosses.TheOrator;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Boss.Orator;
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

        private static bool Active = false;
        private static int RitualTimer = -1;
        private static List<int> NPCIndexs = new();
        private static float zoom = 0;
        private static Vector2 DungeonCoords = new Vector2(Main.dungeonX - 4, Main.dungeonY).ToWorldCoordinates();
        private static bool CultistFacePlayer = true;

        public static bool RitualSequenceSeen = false;
        public override void ClearWorld()
        {
            RitualSequenceSeen = false;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            RitualSequenceSeen = tag.GetBool("RitualSequenceSeen");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["RitualSequenceSeen"] = RitualSequenceSeen;
        }
        public override void OnWorldLoad()
        {
            DungeonCoords = new Vector2(Main.dungeonX - 4, Main.dungeonY).ToWorldCoordinates();
        }
        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];
            #region Debugging Stuffs
            //State = SystemState.CheckReqs; RitualTimer = -2; RitualSequenceSeen = false; Active = false;
            //RitualTimer = 60 * 51;
            //DisplayLocalizedText($"{RitualTimer}, {State}, {(DungeonCoords - mainPlayer.Center).Length()}, {RitualSequenceSeen}");
            #endregion
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (TravellingCultist.RitualQuestProgress != 4 || RitualSequenceSeen)
                       return;
                    else
                    {
                        RitualTimer = -10;
                        State = SystemState.Spawn;
                    }
                    break;
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
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 128, ModContent.NPCType<SealingTablet>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<TravellingCultist>(), 0, 1),
                            
                        };

                        #region Character Setup
                        for (int k = 0; k < 2; k++)
                        {
                            NPC npc = Main.npc[k];
                            if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>() && k != 0)
                            {
                                Recruit.MyName = (RecruitableLunarCultist.RecruitNames)Recruits[k];
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
                            NPC Recruit3 = Main.npc[NPCIndexs[2]];
                            NPC Recruit4 = Main.npc[NPCIndexs[3]];
                            NPC SealingTablet = Main.npc[NPCIndexs[2]]; //[4]
                            NPC LunaticCultist = null;
                            if (NPCIndexs.Count == 4)
                                LunaticCultist = Main.npc[NPCIndexs[3]]; //[5]

                            NPC Orator = null;
                            if (NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>()) != -1)
                            {
                                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];
                            }

                            Rectangle Recruit1Location = new((int)Recruit1.Center.X, (int)Recruit1.Center.Y, Recruit1.width, Recruit1.width);
                            Rectangle Recruit2Location = new((int)Recruit2.Center.X, (int)Recruit2.Center.Y, Recruit2.width, Recruit2.width);
                            Rectangle Recruit3Location = new((int)Recruit3.Center.X, (int)Recruit3.Center.Y, Recruit3.width, Recruit3.width);
                            Rectangle Recruit4Location = new((int)Recruit4.Center.X, (int)Recruit4.Center.Y, Recruit4.width, Recruit4.width);
                            Rectangle LunaticLocation = new();
                            if (LunaticCultist != null)
                                LunaticLocation = new((int)LunaticCultist.Center.X, (int)LunaticCultist.Center.Y, LunaticCultist.width, LunaticCultist.width);
                            Rectangle OratorLocation = new();
                            if (Orator != null)
                                OratorLocation = new((int)Orator.Center.X, (int)Orator.Center.Y, Orator.width, Orator.width);

                            CombatText Text;
                            Color TextColor = Color.Cyan;
                            string key = "LunarCult.TravellingCultist.SealingRitual.";
                            if (RitualTimer > 60 * 65)
                                key = "LunarCult.TheOrator.WorldText.SealingRitual.Initial.";
                            if (Recruit1.ModNPC is RecruitableLunarCultist Recruitable1 && Recruit2.ModNPC is RecruitableLunarCultist Recruitable2 && Recruit3.ModNPC is RecruitableLunarCultist Recruitable3 && Recruit4.ModNPC is RecruitableLunarCultist Recruitable4)
                            {
                                switch (RitualTimer)
                                {
                                    case 1:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '0');
                                        Text.lifeTime = 60;
                                        break;
                                    case 60:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '1');
                                        break;
                                    case 60 * 3:
                                        GetRecruitValues(Recruitable3.MyName.ToString(), ref TextColor, ref key);
                                        Text = DisplayMessage(Recruit3Location, TextColor, key + '0'); //So this is gonna seal Moon Lord?
                                        CultistFacePlayer = false;
                                        LunaticCultist.direction = -1;
                                        break;
                                    case 60 * 5:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '2');
                                        break;
                                    case 60 * 7:
                                        GetRecruitValues(Recruitable1.MyName.ToString(), ref TextColor, ref key);
                                        Text = DisplayMessage(Recruit1Location, TextColor, key + '1'); //Can't believe we were helping with something so disasterous...
                                        break;
                                    case 60 * 10:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '3');
                                        Text.lifeTime = 90;
                                        break;
                                    case 60 * 12:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '4');
                                        break;
                                    case 60 * 14:
                                        GetRecruitValues(Recruitable2.MyName.ToString(), ref TextColor, ref key);
                                        Text = DisplayMessage(Recruit2Location, TextColor, key + '2'); //What do we do?
                                        break;
                                    case 60 * 16:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '5');
                                        break;
                                    case 60 * 18:
                                        SoundEngine.PlaySound(SoundID.Item8, Recruit1.Center);
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '6');
                                        Text.lifeTime = 60;
                                        Recruit2.noGravity = true;
                                        Recruit2.velocity = Vector2.Zero;
                                        GetRecruitValues(Recruitable2.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit2Location, TextColor, "Emoticons.Shock");
                                        break;
                                    case 60 * 19:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '7');
                                        Text.lifeTime = 60;
                                        Recruit3.noGravity = true;
                                        Recruit3.velocity = Vector2.Zero;
                                        GetRecruitValues(Recruitable3.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit3Location, TextColor, "Emoticons.Shock");
                                        break;
                                    case 60 * 20:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '8');
                                        SoundEngine.PlaySound(SoundID.Item8, Recruit2.Center);
                                        Text.lifeTime = 60;
                                        Recruit1.noGravity = true;
                                        Recruit1.velocity = Vector2.Zero;
                                        GetRecruitValues(Recruitable1.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit1Location, TextColor, "Emoticons.Shock");
                                        break;
                                    case 60 * 21:
                                        Recruit4.noGravity = true;
                                        Recruit4.velocity = Vector2.Zero;
                                        GetRecruitValues(Recruitable4.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit4Location, TextColor, "Emoticons.Shock");
                                        break;
                                    case 60 * 22:
                                        SealingTablet.ai[0] = 1;
                                        break;
                                    case 60 * 23:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + '9');
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 25:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "10");
                                        CultistFacePlayer = true;
                                        break;
                                    case 60 * 27:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "11");
                                        CultistFacePlayer = false;
                                        LunaticCultist.direction = -1;
                                        break;
                                    case 60 * 28:
                                        LunaticCultist.direction = 1;
                                        break;
                                    case 60 * 29:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "12");
                                        CultistFacePlayer = true;
                                        break;
                                    case 60 * 31:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "13");
                                        break;
                                    case 60 * 34:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "14");
                                        break;
                                    case 60 * 36:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "15");
                                        break;
                                    case 60 * 38:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "16");
                                        CultistFacePlayer = false;
                                        LunaticCultist.direction = -1;
                                        Text.lifeTime = 180;
                                        break;
                                    case 60 * 41:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "17");
                                        Text.lifeTime = 180;
                                        LunaticCultist.direction = 1;
                                        break;
                                    case 60 * 44:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "18");
                                        Text.lifeTime = 180;
                                        CultistFacePlayer = true;
                                        break;
                                    case 60 * 47:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "19");
                                        Text.lifeTime = 180;
                                        break;
                                    case 60 * 50:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "20");
                                        break;
                                    case 60 * 52:
                                        SoundEngine.PlaySound(SoundID.Item71, DungeonCoords);
                                        Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), DungeonCoords, new Vector2(0, -20), ModContent.ProjectileType<EmpyreanThorn>(), 200, 0f);
                                        CultistFacePlayer = false;
                                        LunaticCultist.noGravity = true;
                                        LunaticCultist.position.Y -= 64;
                                        LunaticCultist.velocity = Vector2.Zero;
                                        LunaticCultist.rotation = -Pi / 2;
                                        break;
                                    case 60 * 55:
                                        Text = DisplayMessage(LunaticLocation, Color.Cyan, key + "21");
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 58:
                                        NPCIndexs.Remove(2);
                                        LunaticCultist.immortal = false;
                                        LunaticCultist.StrikeInstantKill();
                                        Recruit1.noGravity = false; Recruit2.noGravity = false; Recruit3.noGravity = false; Recruit4.noGravity = false;
                                        SealingTablet.ai[0] = 0;
                                        break;
                                    case 60 * 65:
                                        if (mainPlayer.InventoryHas(ModContent.ItemType<SelenicTablet>()))
                                        {
                                            Main.NewText("The Selenic Tablet hums violently...", Color.Cyan);
                                            break;
                                        }
                                        break;
                                    case 60 * 72:
                                        Recruit1.velocity.Y = Recruit2.velocity.Y = Recruit3.velocity.Y = Recruit4.velocity.Y = -5;

                                        GetRecruitValues(Recruitable1.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit1Location, TextColor, "Emoticons.Shock");

                                        GetRecruitValues(Recruitable2.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit2Location, TextColor, "Emoticons.Shock");

                                        GetRecruitValues(Recruitable3.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit3Location, TextColor, "Emoticons.Shock");

                                        GetRecruitValues(Recruitable4.MyName.ToString(), ref TextColor, ref key);
                                        DisplayMessage(Recruit4Location, TextColor, "Emoticons.Shock");

                                        break;
                                    case 60 * 74:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "0");
                                        break;
                                    case 60 * 76:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "1");
                                        break;
                                    case 60 * 78:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "2");
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 79:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "3");
                                        break;
                                    case 60 * 82:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "4");
                                        break;
                                    case 60 * 85:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "5");
                                        break;
                                    case 60 * 88:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "6");
                                        break;

                                    case 60 * 90:
                                        SoundEngine.PlaySound(SoundID.Roar, mainPlayer.Center);
                                        Orator.Transform(ModContent.NPCType<TheOrator>());
                                        State = SystemState.End;
                                        break;
                                }
                            }
                            if (RitualTimer < 30)
                                zoom = Lerp(zoom, 0.4f, 0.075f);
                            else
                                zoom = 0.4f;
                            CameraPanSystem.Zoom = zoom;
                            CameraPanSystem.PanTowards(new Vector2(DungeonCoords.X, DungeonCoords.Y - 150), zoom);

                            if (RitualTimer >= 60 * 62 && RitualTimer <= 60 * 72)
                            {
                                if ((RitualTimer < 60 * 68 && Main.rand.NextBool(3)) || RitualTimer > 60 * 68)
                                {
                                    for (int i = 0; i < RitualTimer / (60 * 62); i++)
                                        EmpyreanMetaball.SpawnDefaultParticle(new Vector2(DungeonCoords.X + Main.rand.NextFloat(-16, 16), DungeonCoords.Y), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-10, -5)), Main.rand.NextFloat(25f, 50f));
                                }
                            }
                            if (RitualTimer == 60 * 70)
                                NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<OratorNPC>());

                            if (Recruit1.Center.Y <= SealingTablet.Center.Y)
                                Recruit1.velocity = Vector2.Zero;
                            if (RitualTimer >= 60 * 18 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit1.Center + new Vector2(Main.rand.Next(Recruit1.width * -1, Recruit1.width), Main.rand.Next(Recruit1.height * -1, Recruit1.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 19)
                                {
                                    Recruit1.velocity.Y = (float)(1 * Math.Sin(RitualTimer / 10));
                                }
                                else if (Recruit2.velocity.Length() < 2)
                                    Recruit1.velocity.Y -= 0.1f;
                            }

                            if (RitualTimer >= 60 * 20 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit2.Center + new Vector2(Main.rand.Next(Recruit2.width * -1, Recruit2.width), Main.rand.Next(Recruit2.height * -1, Recruit2.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 21)
                                {
                                    Recruit2.velocity.Y = (float)(1 * Math.Sin((RitualTimer + 10) / 10));
                                }
                                else if (Recruit2.velocity.Length() < 2)
                                    Recruit2.velocity.Y -= 0.1f;
                            }

                            if (RitualTimer < 60 * 58)
                                if (CultistFacePlayer)
                                    LunaticCultist.aiStyle = 0;
                                else
                                    LunaticCultist.aiStyle = -1;
                        }
                    }
                    RitualTimer++;
                    break;
                case SystemState.End:
                    Active = false;
                    if (!RitualSequenceSeen)
                    {
                        RitualSequenceSeen = true;
                        foreach (int i in NPCIndexs)
                        {
                            NPC npc = Main.npc[i];                        
                            if (npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                                if (npc.ModNPC is RecruitableLunarCultist Recruit)
                                    Recruit.Chattable = true;
                        }
                    }
                    State = SystemState.CheckReqs;
                    break;
            }
        }
        internal static CombatText DisplayMessage(Rectangle location, Color color, string key)
        {
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, GetWindfallTextValue($"Dialogue.{key}"), true)];
            return MyDialogue;
        }
        private static void GetRecruitValues(string name, ref Color TextColor, ref string Key)
        {
            switch (name)
            {
                case "Tirith":
                    TextColor = Color.Yellow;
                    break;
                case "Vivian":
                    TextColor = Color.Red;
                    break;
                case "Tania":
                    TextColor = Color.Green;
                    break;
                case "Doro":
                    TextColor = Color.SandyBrown;
                    break;
                case "Skylar":
                    TextColor = Color.Purple;
                    break;
                case "Jamie":
                    TextColor = Color.Orange;
                    break;
            }
            Key = $"LunarCult.Recruits.{name}.SealingRitual.";

        }
    }
}

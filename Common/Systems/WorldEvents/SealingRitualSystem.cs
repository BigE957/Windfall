using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.Other.RitualFurnature;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

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
        private static List<int> NPCIndexs = [];
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
        public override void OnWorldUnload()
        {
            State = SystemState.CheckReqs;
            zoom = 0;
            CameraPanSystem.Zoom = 0;
        }
        public override void PreUpdateWorld()
        {
            #region Debugging Stuffs
            RitualSequenceSeen = true;
            Recruits = [0, 1, 3, 4];
            //State = SystemState.CheckReqs; RitualTimer = -2; RitualSequenceSeen = false; Active = false;
            //Main.NewText($"{RitualTimer}, {State}, {(DungeonCoords - Main.LocalPlayer.Center).Length()}, {RitualSequenceSeen}");
            //TravellingCultist.RitualQuestProgress = 4;
            #endregion

            if (RitualSequenceSeen)
            {
                if (!Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()))
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X, DungeonCoords.Y - 52), Vector2.Zero, ModContent.ProjectileType<BurningAltar>(), 0, 0f, ai0: 1);
                else if (Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()).ai[0] == 0)
                    Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()).ai[0] = 1;
                if (!NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()))
                    NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 128, ModContent.NPCType<SealingTablet>(), Start: 150, ai0: 2);
                else if (Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())].ai[0] != 2)
                    Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())].ai[0] = 2;
                
                return;
            }
            
            switch (State)
            {
                case SystemState.CheckReqs:
                    if (TravellingCultist.RitualQuestProgress != 4)
                       return;
                    else
                    {
                        RitualTimer = -2;
                        State = SystemState.Spawn;
                    }
                    break;
                case SystemState.Spawn:
                    if (RitualTimer == -2)
                    {
                        Active = false;
                        zoom = 0;
                        NPCIndexs =
                        [
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X - 220), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X - 150), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X + 150), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)(DungeonCoords.X + 220), (int)DungeonCoords.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 128, ModContent.NPCType<SealingTablet>()),
                            NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<TravellingCultist>(), 0, 1),
                            
                        ];
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X, DungeonCoords.Y - 52), Vector2.Zero, ModContent.ProjectileType<BurningAltar>(), 0, 0f, ai0: 0);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X + 64, DungeonCoords.Y - 32), Vector2.Zero, ModContent.ProjectileType<RitualTorch>(), 0, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X - 64, DungeonCoords.Y - 32), Vector2.Zero, ModContent.ProjectileType<RitualTorch>(), 0, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X + 185, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X - 185, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X + 255, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X - 255, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X + 115, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(DungeonCoords.X - 115, DungeonCoords.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);

                        #region Character Setup
                        for (int k = 0; k < 4; k++)
                        {                           
                            NPC npc = Main.npc[NPCIndexs[k]];
                            if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                            {
                                Recruit.MyName = (RecruitableLunarCultist.RecruitNames)Recruits[k];
                                npc.GivenName = ((RecruitableLunarCultist.RecruitNames)Recruits[k]).ToString();
                            }
                            if (Main.dungeonX > Main.spawnTileX)
                                npc.direction = -1;
                            else
                                npc.direction = 1;
                            if (k > 1)
                                npc.direction *= -1;
                        }
                        RitualTimer = -1;
                        #endregion
                    }
                    else
                    {
                        if ((DungeonCoords - Main.player[Player.FindClosest(DungeonCoords, 16, 16)].Center).Length() < 150f && !Active)
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
                            NPC SealingTablet = Main.npc[NPCIndexs[4]];
                            NPC LunaticCultist = null;
                            if (NPCIndexs.Count == 6)
                                LunaticCultist = Main.npc[NPCIndexs[5]];

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
                            if (RitualTimer > 60 * 61)
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
                                        for(int i = 0; i < 30; i++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                            Dust d = Dust.NewDustPerfect(SealingTablet.Center + Main.rand.NextVector2CircularEdge(24, 24), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                            d.noGravity = true;
                                        }
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
                                        NPCIndexs.Remove(4);
                                        LunaticCultist.immortal = false;
                                        LunaticCultist.StrikeInstantKill();
                                        Recruit1.noGravity = false; Recruit2.noGravity = false; Recruit3.noGravity = false; Recruit4.noGravity = false;
                                        SealingTablet.ai[0] = 0;
                                        break;
                                    case 60 * 63:
                                        NPC orator = NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<OratorNPC>());
                                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, orator.Center);
                                        for (int i = 0; i < 32; i++)
                                            EmpyreanMetaball.SpawnDefaultParticle(orator.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(4f, 24f) * -1, Main.rand.NextFloat(110f, 130f));
                                        break;
                                    case 60 * 64:
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
                                    case 60 * 66:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "0");
                                        break;
                                    case 60 * 68:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "1");
                                        break;
                                    case 60 * 70:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "2");
                                        Text.lifeTime = 60;
                                        break;
                                    case 60 * 71:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "3");
                                        break;
                                    case 60 * 74:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "4");
                                        break;
                                    case 60 * 77:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "5");
                                        break;
                                    case 60 * 80:
                                        Text = DisplayMessage(OratorLocation, Color.LightGreen, key + "6");
                                        break;

                                    default:
                                        if (RitualTimer > 60 * 82)
                                        {
                                            SoundEngine.PlaySound(SoundID.Roar, Orator.Center);
                                            Orator.Transform(ModContent.NPCType<TheOrator>());
                                            Orator.ModNPC.OnSpawn(NPC.GetSource_NaturalSpawn());
                                            State = SystemState.End;
                                        }
                                        break;
                                }
                            }
                            if (RitualTimer < 30)
                                zoom = Lerp(zoom, 0.4f, 0.075f);
                            else
                                zoom = 0.4f;
                            CameraPanSystem.Zoom = zoom;
                            CameraPanSystem.PanTowards(new Vector2(DungeonCoords.X, DungeonCoords.Y - 150), zoom);
                            //Main.NewText(RitualTimer / 60);
                            #region Additional Visuals 
                            #region Recruit Hover
                            if (RitualTimer >= 60 * 20 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit1.Center + new Vector2(Main.rand.Next(Recruit1.width * -1, Recruit1.width), Main.rand.Next(Recruit1.height * -1, Recruit1.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 21)
                                {
                                    Recruit1.velocity.Y = (float)(1 * Math.Sin(RitualTimer / 10));
                                }
                                else if (Recruit1.velocity.Length() < 2)
                                    Recruit1.velocity.Y -= 0.1f;
                            }

                            if (RitualTimer >= 60 * 18 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit2.Center + new Vector2(Main.rand.Next(Recruit2.width * -1, Recruit2.width), Main.rand.Next(Recruit2.height * -1, Recruit2.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 19)
                                {
                                    Recruit2.velocity.Y = (float)(1 * Math.Sin((RitualTimer + 10) / 10));
                                }
                                else if (Recruit2.velocity.Length() < 2)
                                    Recruit2.velocity.Y -= 0.1f;
                            }

                            if (RitualTimer >= 60 * 19 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit3.Center + new Vector2(Main.rand.Next(Recruit3.width * -1, Recruit3.width), Main.rand.Next(Recruit3.height * -1, Recruit3.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 20)
                                {
                                    Recruit3.velocity.Y = (float)(1 * Math.Sin(RitualTimer / 10));
                                }
                                else if (Recruit3.velocity.Length() < 2)
                                    Recruit3.velocity.Y -= 0.1f;
                            }

                            if (RitualTimer >= 60 * 21 && RitualTimer <= 60 * 58)
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                                Dust d = Dust.NewDustPerfect(Recruit4.Center + new Vector2(Main.rand.Next(Recruit4.width * -1, Recruit4.width), Main.rand.Next(Recruit4.height * -1, Recruit4.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                                d.noGravity = true;
                                if (RitualTimer >= 60 * 22)
                                {
                                    Recruit4.velocity.Y = (float)(1 * Math.Sin((RitualTimer + 10) / 10));
                                }
                                else if (Recruit4.velocity.Length() < 2)
                                    Recruit4.velocity.Y -= 0.1f;
                            }
                            #endregion
                            if (RitualTimer >= 60 * 50 && RitualTimer <= 60 * 63)
                            {
                                float ratio = Clamp((RitualTimer - (60 * 50)) / 60f, 0f, 1f);
                                Main.NewText(ratio);
                                float width = 64f * ExpInEasing(ratio, 1);
                                width = Clamp(width, 0f, 72f);
                                for (int i = 0; i < 18; i++)
                                    EmpyreanMetaball.SpawnDefaultParticle(new Vector2(DungeonCoords.X + Main.rand.NextFloat(-width, width), DungeonCoords.Y + Main.rand.NextFloat(0, 24f)), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-3, -1) * SineInEasing(ratio, 1)), Main.rand.NextFloat(14f, 28f) * ratio);
                            }

                            if (Recruit1.Center.Y <= SealingTablet.Center.Y)
                                Recruit1.velocity = Vector2.Zero;
                            #endregion
                            if (LunaticCultist != null && RitualTimer < 60 * 51)
                                if (CultistFacePlayer)
                                    LunaticCultist.aiStyle = 0;
                                else
                                    LunaticCultist.aiStyle = -1;
                            RitualTimer++;
                        }
                    }                    
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

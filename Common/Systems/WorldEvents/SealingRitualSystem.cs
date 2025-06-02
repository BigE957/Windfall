using DialogueHelper.UI.Dialogue;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.Props.RitualFurnature;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Common.Systems.WorldEvents;

public class SealingRitualSystem : ModSystem
{
    private enum SystemState
    {
        CheckReqs,
        CheckChance,
        Spawn,
        End,
    }
    private static SystemState State = SystemState.CheckReqs;

    public static bool Active = false;
    private static int RitualTimer = -1;
    private static List<int> NPCIndexs = [];
    private static float zoom = 0;
    private static Point RitualTile;
    private static Vector2 RitualWorld => RitualTile.ToWorldCoordinates();



    private enum CultistFacing
    {
        Left = -1,
        Player,
        Right,
        UhmHeavenIg
    }
    private static CultistFacing CultistDir = CultistFacing.Player;

    private static readonly int[] RecruitsHover = [-1, -1, -1, -1];

    private static bool StopTimer = false;

    public static bool RitualSequenceSeen = false;

    public static int ThornIndex = -1;

    public override void OnModLoad()
    {
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;
    }

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
        RitualTile = new Point(Main.dungeonX + (Main.dungeonX > Main.spawnTileX ? 4 : -4), Main.dungeonY);
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
        /*
        QuestSystem.Quests["SealingRitual"].ResetQuest();
        QuestSystem.Quests["SealingRitual"].Active = true;
        Recruits = [1, 2, 3, 4];
        QuestSystem.Quests["Recruitment"].Progress = 4;
        State = SystemState.CheckReqs; RitualTimer = -2; RitualSequenceSeen = false; Active = false;
        */
        //Recruits = [];
        //Main.NewText($"{RitualTimer}, {State}, {(RitualTile - new Point(Main.dungeonX, Main.dungeonY))}, {RitualSequenceSeen}");
        #endregion

        if (RitualSequenceSeen)
        {
            if (!Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()))
                Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X, RitualWorld.Y - 52), Vector2.Zero, ModContent.ProjectileType<BurningAltar>(), 0, 0f, ai0: 1);
            else if (Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()).ai[0] == 0)
                Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<BurningAltar>()).ai[0] = 1;
            if (!NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()))
            {
                float y;
                for (y = 1; y < 32; y++)
                {
                    if (IsSolidNotDoor(RitualTile - new Point(0, (int)y)))
                        break;
                }
                y -= 0.5f;

                NPC.NewNPC(Entity.GetSource_None(), (int)RitualWorld.X, (int)RitualWorld.Y - (int)(y * 16), ModContent.NPCType<SealingTablet>(), 150, ai1: ((y - 7) * 1.1f) + 2, ai0: 2);
            }
            else if (Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())].ai[0] != 2)
                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SealingTablet>())].ai[0] = 2;
            
            return;
        }
        switch (State)
        {
            case SystemState.CheckReqs:
                if (!QuestSystem.Quests["SealingRitual"].InProgress)
                   return;
                RitualTimer = -2;
                State = SystemState.Spawn;
                break;
            case SystemState.Spawn:
                if (RitualTimer == -2)
                {
                    StopTimer = false;
                    Active = false;
                    zoom = 0;
                    RecruitsHover[0] = -1;
                    RecruitsHover[1] = -1;
                    RecruitsHover[2] = -1;
                    RecruitsHover[3] = -1;

                    float y;
                    for (y = 1; y < 32; y++)
                    {
                        if (IsSolidNotDoor(RitualTile - new Point(0, (int)y)))
                            break;
                    }

                    NPCIndexs =
                    [
                        NPC.NewNPC(Entity.GetSource_None(), (int)(RitualWorld.X - 220), (int)RitualWorld.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                        NPC.NewNPC(Entity.GetSource_None(), (int)(RitualWorld.X - 150), (int)RitualWorld.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                        NPC.NewNPC(Entity.GetSource_None(), (int)(RitualWorld.X + 150), (int)RitualWorld.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                        NPC.NewNPC(Entity.GetSource_None(), (int)(RitualWorld.X + 220), (int)RitualWorld.Y - 8, ModContent.NPCType<RecruitableLunarCultist>()),
                        NPC.NewNPC(Entity.GetSource_None(), (int)RitualWorld.X, (int)RitualWorld.Y - (int)(y * 16), ModContent.NPCType<SealingTablet>(), 150, ai1: ((y - 7) * 1.1f) + 2),
                        NPC.NewNPC(Entity.GetSource_None(), (int)RitualWorld.X, (int)RitualWorld.Y - 8, ModContent.NPCType<TravellingCultist>(), ai3: 1),
                        
                    ];
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X, RitualWorld.Y - 52), Vector2.Zero, ModContent.ProjectileType<BurningAltar>(), 0, 0f, ai0: 0);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X + 64, RitualWorld.Y - 32), Vector2.Zero, ModContent.ProjectileType<RitualTorch>(), 0, 0f);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X - 64, RitualWorld.Y - 32), Vector2.Zero, ModContent.ProjectileType<RitualTorch>(), 0, 0f);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X + 185, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X - 185, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X + 255, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X - 255, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X + 115, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f);
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(RitualWorld.X - 115, RitualWorld.Y - 16), Vector2.Zero, ModContent.ProjectileType<BookPile>(), 0, 0f, ai0: 1);

                    #region Character Setup
                    for (int k = 0; k < 4; k++)
                    {                           
                        NPC npc = Main.npc[NPCIndexs[k]];
                        if (npc.ModNPC is RecruitableLunarCultist Recruit && npc.type == ModContent.NPCType<RecruitableLunarCultist>())
                        {
                            Recruit.MyName = (RecruitableLunarCultist.RecruitNames)Recruits[k];
                            npc.GivenName = ((RecruitableLunarCultist.RecruitNames)Recruits[k]).ToString();
                        }
                        npc.direction = 1;
                        if (k > 1)
                            npc.direction *= -1;
                    }
                    RitualTimer = -1;
                    #endregion
                }
                else
                {
                    if ((RitualWorld - Main.player[Player.FindClosest(RitualWorld, 16, 16)].Center).Length() < 150f && !Active)
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

                        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();

                        switch (RitualTimer)
                        {
                            case 30:
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/SealingRitual/MainRitual", new(Name, [LunaticCultist.whoAmI]));
                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 160: 
                                RecruitsHover[1] = 0;
                                Recruit2.velocity.Y = -3;
                                break;

                            case 250:
                                RecruitsHover[2] = 0;
                                Recruit3.velocity.Y = -3;
                                break;

                            case 340:
                                RecruitsHover[0] = 0;
                                Recruit1.velocity.Y = -3;
                                break;

                            case 430:
                                RecruitsHover[3] = 0;
                                Recruit4.velocity.Y = -3;
                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 500:
                                SoundEngine.PlaySound(SoundID.Item71, RitualWorld);
                                ThornIndex = Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), RitualWorld, new Vector2(0, -20), ModContent.ProjectileType<EmpyreanThorn>(), 200, 0f, ai0: -1);
                                CultistDir = CultistFacing.UhmHeavenIg;
                                LunaticCultist.noGravity = true;
                                LunaticCultist.position.Y -= 32;
                                LunaticCultist.velocity = Vector2.Zero;
                                LunaticCultist.rotation = -Pi / 2;

                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 680:
                                NPC orator = NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), (int)RitualWorld.X, (int)RitualWorld.Y - 8, ModContent.NPCType<OratorNPC>());
                                orator.direction = Math.Sign(Main.player[0].Center.X - orator.Center.X);
                                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, orator.Center);
                                for (int i = 0; i < 32; i++)
                                    EmpyreanMetaball.SpawnDefaultParticle(orator.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(4f, 24f) * -1, Main.rand.NextFloat(110f, 130f));
                                break;

                            case 710:
                                if (Recruit1.ModNPC is RecruitableLunarCultist Recruitable1 && Recruit2.ModNPC is RecruitableLunarCultist Recruitable2 && Recruit3.ModNPC is RecruitableLunarCultist Recruitable3 && Recruit4.ModNPC is RecruitableLunarCultist Recruitable4)
                                {
                                    Recruit1.velocity.Y = Recruit2.velocity.Y = Recruit3.velocity.Y = Recruit4.velocity.Y = -5;

                                    GetRecruitValues(Recruitable1.MyName.ToString(), out Color TextColor, out string key);
                                    DisplayMessage(Recruit1.Hitbox, TextColor, "Emoticons.Shock");

                                    GetRecruitValues(Recruitable2.MyName.ToString(), out TextColor, out key);
                                    DisplayMessage(Recruit2.Hitbox, TextColor, "Emoticons.Shock");

                                    GetRecruitValues(Recruitable3.MyName.ToString(), out TextColor, out key);
                                    DisplayMessage(Recruit3.Hitbox, TextColor, "Emoticons.Shock");

                                    GetRecruitValues(Recruitable4.MyName.ToString(), out TextColor, out key);
                                    DisplayMessage(Recruit4.Hitbox, TextColor, "Emoticons.Shock");
                                }
                                break;

                            case 800:
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/SealingRitual/OratorIntro", new(Name, [NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())]));
                                RitualTimer++;
                                StopTimer = true;
                                break;
                        }

                        if (!StopTimer)
                            RitualTimer++;
                        
                        if (RitualTimer < 30)
                            zoom = Lerp(zoom, 0.4f, 0.075f);
                        else
                            zoom = 0.4f;
                        CameraPanSystem.Zoom = zoom;
                        CameraPanSystem.PanTowards(new Vector2(RitualWorld.X + 120, RitualWorld.Y), zoom * 2.5f);
                        
                        #region Additional Visuals 

                        #region Recruit Hover
                        for(int i = 0; i < 4; i++)
                        {
                            NPC recruit = Main.npc[NPCIndexs[i]];

                            if (RecruitsHover[i] == -1)
                            {
                                recruit.noGravity = false;
                                continue;
                            }

                            if (Main.rand.NextBool())
                            {
                                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f) * 2f;
                                Dust d = Dust.NewDustPerfect(recruit.position + new Vector2(Main.rand.Next(0, recruit.width), Main.rand.Next(0, recruit.height)), DustID.GoldFlame, speed, Scale: Main.rand.NextFloat(1f, 1.5f));
                                d.noGravity = true;
                            }

                            recruit.noGravity = true;

                            if (recruit.velocity.Length() > 1)
                                recruit.velocity.Y += 0.1f;
                            else
                            {
                                recruit.velocity.Y = (float)(-Math.Cos(RecruitsHover[i] / 10));
                                RecruitsHover[i]++;
                            }
                                
                        }
                        #endregion

                        #region Orator Goop
                        if (RitualTimer >= 432 && RitualTimer <= 680)
                        {
                            float ratio = Clamp((RitualTimer - 432) / 60f, 0f, 1f);
                            //Main.NewText(ratio);
                            float width = 64f * ExpInEasing(ratio);
                            width = Clamp(width, 0f, 72f);
                            for (int i = 0; i < 18; i++)
                                EmpyreanMetaball.SpawnDefaultParticle(new Vector2(RitualWorld.X + Main.rand.NextFloat(-width, width), RitualWorld.Y + Main.rand.NextFloat(0, 24f)), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-3, -1) * SineInEasing(ratio)), Main.rand.NextFloat(14f, 28f) * ratio);
                        }

                        if (Recruit1.Center.Y <= SealingTablet.Center.Y)
                            Recruit1.velocity = Vector2.Zero;
                        #endregion

                        #endregion

                        if (LunaticCultist != null && CultistDir != CultistFacing.UhmHeavenIg)
                            switch(CultistDir)
                            {
                                case CultistFacing.Player:
                                    LunaticCultist.aiStyle = 0;
                                    break;
                                default:
                                    LunaticCultist.aiStyle = -1;
                                    LunaticCultist.direction = (int)CultistDir * Main.dungeonX > Main.spawnTileX ? 1 : -1;
                                    break;
                            }
                    }
                }                    
                break;
            case SystemState.End:
                Active = false;
                if (!RitualSequenceSeen)
                {
                    Main.npc[NPCIndexs[4]].ai[0] = 2;
                    RitualSequenceSeen = true;
                    QuestSystem.Quests["SealingRitual"].IncrementProgress();
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
    private static void GetRecruitValues(string name, out Color TextColor, out string Key)
    {
        TextColor = name switch
        {
            "Tirith" => Color.Yellow,
            "Vivian" => Color.Red,
            "Tania" => Color.Green,
            "Doro" => Color.SandyBrown,
            "Skylar" => Color.Purple,
            "Jamie" => Color.Orange,
            _ => Color.White,
        };
        Key = $"LunarCult.Recruits.{name}.SealingRitual.";

    }

    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey != "Cutscenes/SealingRitual/MainRitual")
            return;

        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();

        uiSystem.CurrentTree.Characters[1] = "Recruits/" + ((RecruitableLunarCultist.RecruitNames)Recruits[0]).ToString();
        uiSystem.CurrentTree.Characters[2] = "Recruits/" + ((RecruitableLunarCultist.RecruitNames)Recruits[1]).ToString();
        uiSystem.CurrentTree.Characters[3] = "Recruits/" + ((RecruitableLunarCultist.RecruitNames)Recruits[2]).ToString();
        uiSystem.CurrentTree.Characters[4] = "Recruits/" + ((RecruitableLunarCultist.RecruitNames)Recruits[3]).ToString();

        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{(RecruitableLunarCultist.RecruitNames)Recruits[0]}.SealingRitual.0");
        uiSystem.CurrentTree.Dialogues[3].DialogueText[0].Text = GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{(RecruitableLunarCultist.RecruitNames)Recruits[2]}.SealingRitual.1");
        uiSystem.CurrentTree.Dialogues[5].DialogueText[0].Text = GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{(RecruitableLunarCultist.RecruitNames)Recruits[3]}.SealingRitual.2");
    }

    private static void ClickEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey != "Cutscenes/SealingRitual/MainRitual")
            return;
        switch (dialogueID)
        {
            case 0:
                CultistDir = CultistFacing.Left;
                break;
            case 2:
                CultistDir = CultistFacing.Right;
                break;
            case 3:
                CultistDir = CultistFacing.Player;
                break;
            case 4:
                CultistDir = CultistFacing.Right;
                break;
            case 5:
                StopTimer = false;
                break;
            case 6:
                CultistDir = CultistFacing.Player;
                for (int i = 0; i < 30; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust d = Dust.NewDustPerfect(Main.npc[NPCIndexs[4]].Center + Main.rand.NextVector2CircularEdge(24, 24), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                    d.noGravity = true;
                }
                Main.npc[NPCIndexs[4]].ai[0] = 1;
                break;
            case 9:
                StopTimer = false;
                break;
            case 11:
                NPC LunaticCultist = Main.npc[NPCIndexs[5]];
                NPC SealingTablet = Main.npc[NPCIndexs[4]];
                LunaticCultist.immortal = false;
                LunaticCultist.StrikeInstantKill();
                NPCIndexs.RemoveAt(5);

                if (ThornIndex == -1)
                    ThornIndex = FindFirstProjectile(ModContent.ProjectileType<EmpyreanThorn>());

                Main.projectile[ThornIndex].As<EmpyreanThorn>().canDespawn = true;

                for(int i = 0; i < 4; i++)
                    RecruitsHover[i] = -1;

                SealingTablet.ai[0] = 0;
                StopTimer = false;
                break;
        }
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey == "Cutscenes/SealingRitual/OratorIntro")
        {
            NPC Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];
            SoundEngine.PlaySound(SoundID.Roar, Orator.Center);
            Orator.Transform(ModContent.NPCType<TheOrator>());
            Orator.ModNPC.OnSpawn(NPC.GetSource_NaturalSpawn());
            State = SystemState.End;
        }
    }

}

using DialogueHelper.UI.Dialogue;
using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Items.Lore;
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

    private static bool Active = false;
    private static int RitualTimer = -1;
    private static List<int> NPCIndexs = [];
    private static float zoom = 0;
    private static Vector2 DungeonCoords = new Vector2(Main.dungeonX - 4, Main.dungeonY).ToWorldCoordinates();
    
    private enum CultistFacing
    {
        Left = -1,
        Player,
        Right,
        UhmHeavenIg
    }
    private static CultistFacing CultistDir = CultistFacing.Player;

    private static readonly bool[] RecruitsHover = [false, false, false, false];

    private static bool StopTimer = false;

    public static bool RitualSequenceSeen = false;

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
        //QuestSystem.Quests["SealingRitual"].Active = false;
        //State = SystemState.CheckReqs;

        //RitualSequenceSeen = false;
        //Recruits = [1, 2, 3, 4];
        //Recruits = [];
        //QuestSystem.Quests["Recruitment"].Progress = 4;
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
                if (!QuestSystem.Quests["SealingRitual"].InProgress)
                   return;
                RitualTimer = -2;
                State = SystemState.Spawn;
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
                        NPC.NewNPC(Entity.GetSource_None(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<TravellingCultist>(), ai3: 1),
                        
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

                        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
                        switch (RitualTimer)
                        {
                            case 30:
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/Arrival", new(Name, [LunaticCultist.whoAmI]));
                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 60:
                                RecruitsHover[1] = true;
                                break;

                            case 90:
                                RecruitsHover[2] = true;
                                break;

                            case 120:
                                RecruitsHover[0] = true;
                                break;

                            case 150:
                                RecruitsHover[3] = true;
                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 270:
                                SoundEngine.PlaySound(SoundID.Item71, DungeonCoords);
                                Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), DungeonCoords, new Vector2(0, -20), ModContent.ProjectileType<EmpyreanThorn>(), 200, 0f, ai0: -1);
                                CultistDir = CultistFacing.UhmHeavenIg;
                                LunaticCultist.noGravity = true;
                                LunaticCultist.position.Y -= 64;
                                LunaticCultist.velocity = Vector2.Zero;
                                LunaticCultist.rotation = -Pi / 2;

                                RitualTimer++;
                                StopTimer = true;
                                break;

                            case 870:
                                NPC orator = NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), (int)DungeonCoords.X, (int)DungeonCoords.Y - 8, ModContent.NPCType<OratorNPC>());
                                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, orator.Center);
                                for (int i = 0; i < 32; i++)
                                    EmpyreanMetaball.SpawnDefaultParticle(orator.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(4f, 24f) * -1, Main.rand.NextFloat(110f, 130f));
                                break;

                            case 930:
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

                            case 1050:
                                uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/DraconicRuins/OratorIntro", new(Name, [LunaticCultist.whoAmI]));
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
                        CameraPanSystem.PanTowards(new Vector2(DungeonCoords.X, DungeonCoords.Y - 150), zoom);
                        
                        #region Additional Visuals 

                        #region Recruit Hover
                        for(int i = 0; i < 4; i++)
                        {
                            if (!RecruitsHover[i])
                                continue;

                            NPC recruit = Main.npc[NPCIndexs[i]];

                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust d = Dust.NewDustPerfect(recruit.Center + new Vector2(Main.rand.Next(recruit.width * -1, recruit.width), Main.rand.Next(recruit.height * -1, recruit.height)), DustID.GoldFlame, speed * 2, Scale: 1.5f);
                            d.noGravity = true;
                            if (RitualTimer >= 60 * 21)
                            {
                                recruit.velocity.Y = (float)(1 * Math.Sin(RitualTimer / 10));
                            }
                            else if (recruit.velocity.Length() < 2)
                                recruit.velocity.Y -= 0.1f;
                        }
                        #endregion

                        #region Orator Goop
                        if (RitualTimer >= 152 && RitualTimer <= 870)
                        {
                            float ratio = Clamp((RitualTimer - 152) / 60f, 0f, 1f);
                            //Main.NewText(ratio);
                            float width = 64f * ExpInEasing(ratio);
                            width = Clamp(width, 0f, 72f);
                            for (int i = 0; i < 18; i++)
                                EmpyreanMetaball.SpawnDefaultParticle(new Vector2(DungeonCoords.X + Main.rand.NextFloat(-width, width), DungeonCoords.Y + Main.rand.NextFloat(0, 24f)), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-3, -1) * SineInEasing(ratio)), Main.rand.NextFloat(14f, 28f) * ratio);
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
                        RitualTimer++;
                    }
                }                    
                break;
            case SystemState.End:
                Active = false;
                if (!RitualSequenceSeen)
                {
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
            default:
                TextColor = Color.White;
                break;
        }
        Key = $"LunarCult.Recruits.{name}.SealingRitual.";

    }

    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        throw new NotImplementedException();
    }

    private static void ClickEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey != "Cutscenes/SealingRitual/RitualMain")
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

                for(int i = 0; i < 4; i++)
                    RecruitsHover[i] = false;

                SealingTablet.ai[0] = 0;
                StopTimer = false;
                break;
        }
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey != "Cutscenes/SealingRitual/OratorIntro")
        {
            NPC Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorNPC>())];
            SoundEngine.PlaySound(SoundID.Roar, Orator.Center);
            Orator.Transform(ModContent.NPCType<TheOrator>());
            Orator.ModNPC.OnSpawn(NPC.GetSource_NaturalSpawn());
            State = SystemState.End;
        }
    }
}

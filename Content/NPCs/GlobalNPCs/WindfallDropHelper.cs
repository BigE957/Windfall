using Terraria.GameContent.ItemDropRules;
using Windfall.Common.Systems;
using Windfall.Content.Items.Essences;
using Windfall.Content.Items.Journals;
using Windfall.Content.Items.Lore;
using Windfall.Content.Items.Quest.SealingRitual;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Content.NPCs.GlobalNPCs
{
    public class WindfallDropHelper : GlobalNPC
    {
        [JITWhenModsEnabled("CalamityMod")]
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            #region Vanilla Enemy Drops
            if (npc.type == NPCID.DarkCaster)
            {
                npcLoot.Add(ItemDropRule.Common(ItemID.WaterBolt, 10));
            }
            if (npc.type == NPCID.AngryBones || npc.type == NPCID.AngryBonesBig || npc.type == NPCID.AngryBonesBigHelmet || npc.type == NPCID.AngryBonesBigMuscle)
            {
                npcLoot.AddIf(() => TravellingCultist.QuestArtifact.Type == ModContent.ItemType<DeificInsignia>() && NPC.AnyNPCs(ModContent.NPCType<TravellingCultist>()), ModContent.ItemType<DeificInsignia>(), 5);
            }
            #endregion
            #region Vanilla Boss Drops
            if (npc.type == NPCID.EaterofWorldsHead)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedBoss2, ModContent.ItemType<EoWEssence>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == NPCID.BrainofCthulhu)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedBoss2, ModContent.ItemType<BoCEssence>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == NPCID.QueenSlimeBoss)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedQueenSlime, ModContent.ItemType<HallowLore>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == NPCID.Golem)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedGolemBoss, ModContent.ItemType<TabletFragment>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == NPCID.DukeFishron)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedFishron, ModContent.ItemType<DraconicBone>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == NPCID.HallowBoss)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedEmpressOfLight, ModContent.ItemType<PrimalLightShard>(), desc: DropHelper.FirstKillText);
            }
            #endregion
            #region Calamity Drops
            Mod calamity = ModLoader.GetMod("CalamityMod");
            #region Calamity Enemy Drops
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedCnidrion, ModContent.ItemType<JournalDesert>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("GiantClam").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedCLAM, ModContent.ItemType<JournalIlmeris>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedEvil2Summon, ModContent.ItemType<JournalCrimson>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedEvil2Summon, ModContent.ItemType<JournalCorruption>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("CragmawMire").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedCragmawMire, ModContent.ItemType<JournalSulphur>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedSirenLure, ModContent.ItemType<JournalOcean>(), desc: DropHelper.FirstKillText);
            }
            #endregion
            #region Calamity Boss Drops
            if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDesertScourge, ModContent.ItemType<IlmerisLore>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("HiveMind").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator, ModContent.ItemType<HiveEssence>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("PerforatorHive").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedPerforator && !DownedBossSystem.downedHiveMind, ModContent.ItemType<PerfEssence>(), desc: DropHelper.FirstKillText);
            }
                if (npc.type == calamity.Find<ModNPC>("SlimeGodCore").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedSlimeGod, ModContent.ItemType<StatisLore>(), desc: DropHelper.FirstKillText);
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedSlimeGod, ModContent.ItemType<SlimeEssence>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("ProfanedGuardianCommander").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedGuardians, ModContent.ItemType<ProfanedWastesLore>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("DevourerofGodsHead").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDoG, ModContent.ItemType<DistortionLore>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("Providence").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedProvidence, ModContent.ItemType<BraeLore>(), desc: DropHelper.FirstKillText);
            }
            if (npc.type == calamity.Find<ModNPC>("Yharon").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedYharon, ModContent.ItemType<DragonsAerieLore>(), desc: DropHelper.FirstKillText);
            }
            #endregion
            #endregion
        }
    }
}
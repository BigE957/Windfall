﻿using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using CalamityMod;
using Windfall.Content.Items.Journals;
using Windfall.Content.Items.Lore;
using Windfall.Common.Systems;

namespace Windfall.Content.NPCs
{
    public class DropHelper : GlobalNPC
    {
        [JITWhenModsEnabled("CalamityMod")]
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedCnidrion, ModContent.ItemType<JournalDesert>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("GiantClam").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedCLAM, ModContent.ItemType<JournalIlmeris>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedEvil2Summon, ModContent.ItemType<JournalCrimson>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedEvil2Summon, ModContent.ItemType<JournalCorruption>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("CragmawMire").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedCragmawMire, ModContent.ItemType<JournalSulphur>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedSirenLure, ModContent.ItemType<JournalOcean>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDesertScourge, ModContent.ItemType<IlmerisLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("SlimeGodCore").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedSlimeGod, ModContent.ItemType<StatisLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == NPCID.QueenSlimeBoss)
            {
                npcLoot.AddConditionalPerPlayer(() => !NPC.downedQueenSlime, ModContent.ItemType<HallowLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("ProfanedGuardianCommander").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedGuardians, ModContent.ItemType<ProfanedWastesLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("DevourerofGodsHead").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDoG, ModContent.ItemType<DistortionLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("Providence").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedProvidence, ModContent.ItemType<BraeLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }
            if (npc.type == calamity.Find<ModNPC>("Yharon").Type)
            {
                npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedYharon, ModContent.ItemType<DragonsAerieLore>(), desc: CalamityMod.DropHelper.FirstKillText);

            }

        }

    }

}
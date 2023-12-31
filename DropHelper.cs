using Terraria.ModLoader;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;
using CalamityMod.Systems;
using CalamityMod;
using WindfallAttempt1.Items.Lore;


namespace WindfallAttempt1
{
	public class DropHelper : GlobalNPC
	{
		[JITWhenModsEnabled("CalamityMod")]
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
			Mod calamity = ModLoader.GetMod("CalamityMod");
			if (npc.type == calamity.Find<ModNPC>("DesertScourgeHead").Type)
			{
				npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDesertScourge, ModContent.ItemType<IllmerisLore>(), desc: CalamityMod.DropHelper.FirstKillText);

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
				npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedGSS, ModContent.ItemType<ProfanedWastesLore>(), desc: CalamityMod.DropHelper.FirstKillText);

			}
			if (npc.type == calamity.Find<ModNPC>("DevourerofGodsHead").Type)
			{
				npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDoG, ModContent.ItemType<DistortionLore>(), desc: CalamityMod.DropHelper.FirstKillText);

			}
			if (npc.type == calamity.Find<ModNPC>("DevourerofGodsHead").Type)
			{
				npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedDesertScourge, ModContent.ItemType<BraeLore>(), desc: CalamityMod.DropHelper.FirstKillText);

			}
			if (npc.type == calamity.Find<ModNPC>("Yharon").Type)
			{
				npcLoot.AddConditionalPerPlayer(() => !DownedBossSystem.downedYharon, ModContent.ItemType<DragonsAerieLore>(), desc: CalamityMod.DropHelper.FirstKillText);

			}

		}

	}

}
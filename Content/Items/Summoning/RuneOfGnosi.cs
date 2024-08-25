using CalamityMod.Events;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Items.Summoning
{
    public class RuneOfGnosi : ModItem, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Items/SummonItems/RuneofKos";

        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 18;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<TheOrator>());
            else
                NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<TheOrator>());

            return true;
        }
    }
}

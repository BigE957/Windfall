using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Windfall.Systems;
using Windfall.UI.WanderersJournals;


namespace Windfall.Items.Journals
{
    public class JournalCorruption : ModItem, ILocalizedModType
    {
        public static readonly SoundStyle UseSound = new("Windfall/Sounds/Items/JournalPageTurn");

        public new string LocalizationCategory => "Items.Journals";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = UseSound;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            WindfallGlobalItem.InsertJournalTooltop(tooltips);
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
                WorldSaveSystem.JournalsCollected[4] = true;
                JournalUISystem.whichEvilJournal = "Corruption";
            }
            return true;
        }
    }
}
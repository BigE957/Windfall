using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Windfall.Content.Systems;
using Windfall.Content.UI.WanderersJournals;


namespace Windfall.Content.Items.Journals
{
    public class JournalDesert : ModItem, ILocalizedModType
    {
        public static readonly SoundStyle UseSound = new("Windfall/Sounds/Items/JournalPageTurn");
        public new string LocalizationCategory => "Items.Journals";
        public override string Texture => "Windfall/Assets/Items/Journals/JournalDesert";

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
                WorldSaveSystem.JournalsCollected[2] = true;
            }
            return true;
        }
    }
}
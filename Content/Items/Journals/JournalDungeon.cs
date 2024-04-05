using Windfall.Common.Systems;


namespace Windfall.Content.Items.Journals
{
    public class JournalDungeon : ModItem, ILocalizedModType
    {
        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/JournalPageTurn");

        public new string LocalizationCategory => "Items.Journals";
        public override string Texture => "Windfall/Assets/Items/Journals/JournalDungeon";

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
                WorldSaveSystem.JournalsCollected[7] = true;
            }
            return true;
        }
    }
}
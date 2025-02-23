using Windfall.Content.UI.Activities;

namespace Windfall.Content.Items.Quests.Cafeteria;
internal class TailorInstructions : ModItem
{
    public override string Texture => "Windfall/Assets/Items/Journals/JournalForest";
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.scale = 0.75f;
        Item.rare = ItemRarityID.Blue;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            if (!TailorInstructionsUISystem.IsTailorInstructionsOpen)
                ModContent.GetInstance<TailorInstructionsUISystem>().ShowTailorInstructionsUI();
            else
                ModContent.GetInstance<TailorInstructionsUISystem>().HideTailorInstructionsUI();
        }
        return true;
    }
}
namespace Windfall.Content.Items.Quests.Tailor;

public class DyedHood : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => $"Windfall/Assets/Items/Quest/Tailor/{nameof(DyedHood)}";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 1;
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Hood>().
            AddIngredient(ItemID.GreenandBlackDye).
            AddTile(TileID.DyeVat).
            Register();
    }
}

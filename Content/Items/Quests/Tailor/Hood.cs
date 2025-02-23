namespace Windfall.Content.Items.Quests.Tailor;

public class Hood : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => $"Windfall/Assets/Items/Quest/Tailor/{nameof(Hood)}";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.rare = ItemRarityID.White;
        Item.maxStack = 1;
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Silk, 10).
            AddTile(TileID.Loom).
            Register();
    }
}

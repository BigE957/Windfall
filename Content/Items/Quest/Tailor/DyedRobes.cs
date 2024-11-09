namespace Windfall.Content.Items.Quest.Tailor;

public class DyedRobes : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => $"Windfall/Assets/Items/Quest/Tailor/{nameof(DyedRobes)}";
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
            AddIngredient(ItemID.Robe).
            AddIngredient(ItemID.GreenandBlackDye).
            AddTile(TileID.DyeVat).
            Register();
    }
}

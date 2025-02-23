namespace Windfall.Content.Items.Quests.Tailor;

public class LunarCultistHood : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => $"Windfall/Assets/Items/Quest/Tailor/{nameof(LunarCultistHood)}";
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
            AddIngredient<DyedHood>().
            AddIngredient(ItemID.Silk, 2).
            AddTile(TileID.Loom).
            Register();
    }
}

using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Hangers;
public class Hanger : ModItem, ILocalizedModType, IModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<HangerTile>();
        Item.rare = ItemRarityID.Lime;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddTile(TileID.Sawmill)
            .Register();
    }
}

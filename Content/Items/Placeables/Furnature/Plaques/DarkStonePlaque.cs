using Windfall.Content.Tiles.Furnature.Plaques;

namespace Windfall.Content.Items.Placeables.Furnature.Plaques;
public class DarkStonePlaque : ModItem, ILocalizedModType, IModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 28;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<DarkStonePlaqueTile>();
        Item.rare = ItemRarityID.Lime;
    }
}

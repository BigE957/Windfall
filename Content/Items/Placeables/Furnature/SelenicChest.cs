using Windfall.Content.Tiles.Furnature;

namespace Windfall.Content.Items.Placeables.Furnature;
public class SelenicChest : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 22;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.value = 500;
        Item.createTile = ModContent.TileType<SelenicChestTile>();
    }
}
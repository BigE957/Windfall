using Windfall.Content.Tiles.Furnature;

namespace Windfall.Content.Items.Placeables.Furnature;
public class HandStatue : ModItem, ILocalizedModType, IModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 84;
        Item.height = 90;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<HandStatueTile>();
        Item.rare = ItemRarityID.Lime;
    }
}

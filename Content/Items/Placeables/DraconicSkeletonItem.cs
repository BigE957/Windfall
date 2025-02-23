using Windfall.Content.Tiles;

namespace Windfall.Content.Items.Placeables;
public class DraconicSkeletonItem : ModItem, ILocalizedModType, IModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override string Texture => "Terraria/Images/Item_5100";

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
        Item.createTile = ModContent.TileType<DraconicSkeleton>();
        Item.rare = ItemRarityID.Lime;
    }
}

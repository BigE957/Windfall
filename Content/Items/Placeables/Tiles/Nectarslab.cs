using Windfall.Content.Tiles.Blocks;

namespace Windfall.Content.Items.Placeables.Tiles;
public class Nectarslab : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 12;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<NectarslabTile>();
    }

    public override void AddRecipes()
    {
        CreateRecipe(100).
            AddIngredient(ItemID.StoneSlab, 100).
            AddIngredient(ItemID.CrispyHoneyBlock, 10).
            AddTile(TileID.HeavyWorkBench).
            Register();
    }
}

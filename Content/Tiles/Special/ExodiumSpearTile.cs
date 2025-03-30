using Windfall.Content.Items.Weapons.Rogue;
using CalamityMod.Tiles.Ores;

namespace Windfall.Content.Tiles.Special;
public class ExodiumSpearTile : ModTile
{
    public override void SetStaticDefaults()
    {
        MergeWithSet(Type, ModContent.TileType<ExodiumOre>());

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.Height = 9;
        TileObjectData.newTile.Origin = new Point16(1, 5);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16, 16, 16];

        AddMapEntry(Color.DarkGray, GetItemName<ExodiumSpear>());

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;

        RegisterItemDrop(ModContent.ItemType<ExodiumSpear>());

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<ExodiumSpear>(), Type, 0);

        TileObjectData.addTile(Type);
    }

    public override bool CanExplode(int i, int j) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 4;
}

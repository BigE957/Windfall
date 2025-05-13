using CalamityMod;
using Windfall.Content.Items.Placeables;

namespace Windfall.Content.Tiles.Blocks;
public class DriedStones : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = false;
        Main.tileCut[Type] = false;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;

        HitSound = SoundID.Dig;

        AddMapEntry(new Color(127, 111, 144));

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<DriedSeeds>(), Type, 0, 1, 2);

        base.SetStaticDefaults();
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = 0;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
        Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];
        if (nearestPlayer.active)
        {
            if (nearestPlayer.ActiveItem().type == ItemID.Sickle)
                yield return new Item(ItemID.Hay, Main.rand.Next(1, 2 + 1));

            if (Main.rand.NextBool(20))
                yield return new Item(ModContent.ItemType<DriedSeeds>());
        }
    }
}

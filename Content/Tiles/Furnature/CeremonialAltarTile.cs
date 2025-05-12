using CalamityMod;

namespace Windfall.Content.Tiles.Furnature;
public class CeremonialAltarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;

        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Width = 6;
        int height = TileObjectData.newTile.Height = 2;

        TileObjectData.newTile.Origin = new Point16(3, 1);
        TileObjectData.newTile.CoordinateHeights = new int[height];
        for (int i = 0; i < height; i++)
            TileObjectData.newTile.CoordinateHeights[i] = 16;

        //TileObjectData.newTile.CoordinateHeights[22] = 18;
        TileObjectData.newTile.DrawYOffset = 2;
        /*
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        */
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(240, 190, 60));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX == 36 || tile.TileFrameX == 54)
        {
            r = 0.5f;
            g = 0.25f;
            b = 0.25f;
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Tile tile = Main.tile[i, j];
        //0, 18, 36, 54, 72, 90
        if (tile.TileFrameY == 0 && tile.TileFrameX != 0 && tile.TileFrameX != 90)
        {
            CalamityUtils.DrawFlameSparks(DustID.OrangeTorch, 12, i, j);
        }
    }
}

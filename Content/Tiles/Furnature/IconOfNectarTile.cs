using CalamityMod;

namespace Windfall.Content.Tiles.Furnature;
public class IconOfNectarTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;

        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Width = 11;
        int height = TileObjectData.newTile.Height = 13;

        TileObjectData.newTile.Origin = new Point16(6, 11);
        TileObjectData.newTile.CoordinateHeights = new int[height];
        for (int i = 0; i < height; i++)
            TileObjectData.newTile.CoordinateHeights[i] = 16;

        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(80, 190, 60));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameY >= 198 && tile.TileFrameX >= 72 && tile.TileFrameX <= 108)
        {
            r = 0.5f;
            g = 0.25f;
            b = 0.25f;
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        Tile tile = Main.tile[i, j];
        //0, 18, 36, 54, 72, 90, 108, 126, 144, 162, 180
        if (tile.TileFrameY == 198 && tile.TileFrameX >= 54 && tile.TileFrameX <= 126)
        {
            CalamityUtils.DrawFlameSparks(DustID.OrangeTorch, 12, i, j);
        }
    }
}

using Terraria.Enums;
using Windfall.Content.Items.Placeables.Furnature;

namespace Windfall.Content.Tiles.Furnature;
public class HandStatueTile : ModTile
{
    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<HandStatue>());
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Width = 21;
        TileObjectData.newTile.Height = 23;

        TileObjectData.newTile.Origin = new Point16(10, 22);
        TileObjectData.newTile.CoordinateHeights = new int[23];
        for(int i = 0; i < 23; i++)
        {
            TileObjectData.newTile.CoordinateHeights[i] = 16;
        }
        //TileObjectData.newTile.CoordinateHeights[22] = 18;
        TileObjectData.newTile.DrawYOffset = 2;

        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(128, 128, 128));
    }
}

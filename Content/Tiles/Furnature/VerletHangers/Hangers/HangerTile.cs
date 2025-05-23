using CalamityMod.TileEntities;
using Terraria.Enums;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Hangers;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
public class HangerTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        Main.tileSolid[Type] = false;

        RegisterItemDrop(ModContent.ItemType<Hanger>());
        TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.WaterDeath = false;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = [16];

        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<HangerEntity>().Hook_AfterPlacement, -1, 0, true);
        TileObjectData.newTile.UsesCustomCanPlace = true;

        TileObjectData.newTile.StyleHorizontal = false;
        TileObjectData.newTile.StyleWrapLimit = 4;
        TileObjectData.newTile.StyleMultiplier = 4;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorAlternateTiles = [
            TileID.WoodenBeam,
            561,
            574,
            575,
            576,
            577,
            578
        ];
        TileObjectData.addAlternate(2);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
        TileObjectData.newAlternate.AnchorAlternateTiles = [
            TileID.WoodenBeam,
			561,
			574,
			575,
			576,
			577,
			578

        ];
        TileObjectData.addAlternate(3);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addAlternate(1);

        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(Color.Gray);

        DustType = DustID.Silver;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (!fail)
            FindTileEntity<HangerEntity>(i, j, 1, 1)?.Kill(i, j);
    }
}

using CalamityMod;
using CalamityMod.Items.Placeables.Furniture;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Windfall.Content.Items.Placeables.Furnature;
using Windfall.Content.Items.Utility;

namespace Windfall.Content.Tiles.Furnature;
public class SelenicChestTile : ModTile
{
    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<SelenicChest>());

        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Containers];

        AddMapEntry(new Color(174, 129, 92), this.GetLocalization("MapEntry0"), MapChestName);
        AddMapEntry(new Color(174, 129, 92), this.GetLocalization("MapEntry1"), MapChestName);
        DustType = DustID.Gold;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

    public string MapChestName(string name, int i, int j) => CalamityUtils.GetMapChestName(name, i, j);
    public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameX / 36);
    public override LocalizedText DefaultContainerName(int frameX, int frameY)
    {
        int option = frameX / 36;
        return this.GetLocalization("MapEntry" + option);
    }
    public override void MouseOver(int i, int j) => CalamityUtils.ChestMouseOver<AstralChest>(i, j);
    public override void MouseOverFar(int i, int j) => CalamityUtils.ChestMouseFar<AstralChest>(i, j);
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    // Locked Chest stuff
    public override bool IsLockedChest(int i, int j) => Main.tile[i, j].TileFrameX / 36 == 1;
    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
    {
        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<CrescentKey>())
            return false;

        Main.LocalPlayer.HeldItem.stack--; // Uses the key

        dustType = DustType;
        return true;
    }
    public override bool RightClick(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        int left = i;
        int top = j;

        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }
        if (tile.TileFrameY != 0)
        {
            top--;
        }
        return CalamityUtils.LockedChestRightClick(IsLockedChest(left, top), left, top, i, j);
    }
}

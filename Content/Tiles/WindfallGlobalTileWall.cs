using Windfall.Content.Items.Tools;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Content.Tiles;

public class WindfallGlobalTileWall : GlobalWall
{
    public override bool CanExplode(int i, int j, int type)
    {
        if (CultBaseTileArea.Intersects(new(i, j, 16, 16)) || CultBaseBridgeArea.Intersects(new(i, j, 16, 16)))
            return false;

        return base.CanExplode(i, j, type);
    }

    public override bool CanPlace(int i, int j, int type)
    {
        if (CultBaseTileArea.Intersects(new(i, j, 1, 1)) || CultBaseBridgeArea.Intersects(new(i, j, 1, 1)))
            return false;

        return base.CanPlace(i, j, type);
    }

    public override void KillWall(int i, int j, int type, ref bool fail)
    {
        if (CultBaseTileArea.Intersects(new(i, j, 16, 16)) || CultBaseBridgeArea.Intersects(new(i, j, 16, 16)))
            fail = true;
        Rectangle wallArea = new(i - 1, j - 1, 3, 3);
        if ((wallArea.Contains(Main.MouseWorld.ToTileCoordinates()) || wallArea.Contains(new Point(Main.SmartCursorX, Main.SmartCursorY))) && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<HammerChisel>())
            fail = true;
    }
}

using Windfall.Common.Systems;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Content.Tiles;

public class WindfallGlobalTileWall : GlobalWall
{
    public override bool CanExplode(int i, int j, int type)
    {
        if (CultBaseArea.Intersects(new(i, j, 16, 16)) || CultBaseBridgeArea.Intersects(new(i, j, 16, 16)))
            return false;

        return base.CanExplode(i, j, type);
    }

    public override bool CanPlace(int i, int j, int type)
    {
        if (CultBaseArea.Intersects(new(i, j, 1, 1)) || CultBaseBridgeArea.Intersects(new(i, j, 1, 1)))
            return false;

        return base.CanPlace(i, j, type);
    }

    public override void KillWall(int i, int j, int type, ref bool fail)
    {
        if (CultBaseArea.Intersects(new(i, j, 16, 16)) || CultBaseBridgeArea.Intersects(new(i, j, 16, 16)))
            fail = true;
    }
}

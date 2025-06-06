﻿using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Tools;

namespace Windfall.Content.Tiles;

public class WindfallGlobalTileWall : GlobalWall
{
    public override bool CanExplode(int i, int j, int type)
    {
        Point p = new(i, j);
        if (LunarCultBaseSystem.ShouldBlockTerrainModification(p) || DraconicRuinsSystem.ShouldBlockTerrainModification(p))
            return false;

        return base.CanExplode(i, j, type);
    }

    public override bool CanPlace(int i, int j, int type)
    {
        Point p = new(i, j);
        if (LunarCultBaseSystem.ShouldBlockTerrainModification(p) || DraconicRuinsSystem.ShouldBlockTerrainModification(p))
            return false;

        return base.CanPlace(i, j, type);
    }

    public override void KillWall(int i, int j, int type, ref bool fail)
    {
        Point p = new(i, j);
        if (LunarCultBaseSystem.ShouldBlockTerrainModification(p) || DraconicRuinsSystem.ShouldBlockTerrainModification(p))
            fail = true;
        Rectangle wallArea = new(i - 1, j - 1, 3, 3);
        if ((wallArea.Contains(Main.MouseWorld.ToTileCoordinates()) || wallArea.Contains(new Point(Main.SmartCursorX, Main.SmartCursorY))) && Main.LocalPlayer.HeldItem.type == ModContent.ItemType<HammerChisel>())
            fail = true;
    }
}

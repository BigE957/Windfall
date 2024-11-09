namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    /// <summary>
    /// Checks if a chest contains a given ItemType and returns that items index if it finds it. Otherwise, returns -1.
    /// </summary>
    public static int ChestContains(Chest chest, int type)
    {
        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
            if (chest.item[inventoryIndex].type == type)
                return inventoryIndex;
        return -1;
    }
    public static Point FindSurfaceBelow(Point p)
    {
        
        if (WorldGen.SolidTile(p))
            while (WorldGen.SolidTile(p.X, p.Y - 1) && p.Y >= 1)
                p.Y--;
        else
        {
            
            while (!WorldGen.SolidTile(p.X, p.Y + 1) && !TileID.Sets.Platforms[Framing.GetTileSafely(p.X, p.Y).TileType] && p.Y < Main.maxTilesY)
                p.Y++;
            if (!TileID.Sets.Platforms[Framing.GetTileSafely(p.X, p.Y).TileType] && !Main.tile[p.X, p.Y].IsHalfBlock)
                p.Y++;               
        }

        return p;
    }
}

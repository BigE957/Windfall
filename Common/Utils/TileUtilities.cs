using Terraria;
using Terraria.ID;

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

    public static bool IsSolidOrPlatform(Point p) => WorldGen.SolidTile(p) || TileID.Sets.Platforms[Framing.GetTileSafely(p.X, p.Y).TileType];

    public static bool IsSolidNotDoor(Point p)
    {
        Tile tile = Framing.GetTileSafely(p.X, p.Y);

        if (!tile.IsTileSolid())
            return false;
        return !TileLoader.IsClosedDoor(tile) && tile.TileType != TileID.TallGateClosed;
    }

    public static bool TryOpenDoor(Point p, int direction)
    {
        Tile tile = Framing.GetTileSafely(p.X, p.Y);
        if (tile.IsTileSolid())
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (TileLoader.IsClosedDoor(tile))
                    return WorldGen.OpenDoor(p.X, p.Y, 1);
                else if (tile.TileType == TileID.TallGateClosed)
                    return WorldGen.ShiftTallGate(p.X, p.Y, false);
            }
        }
        return false;
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
    /// <summary>
    /// Slopes the tile at the given Point as if it were hit by a Hammer.
    /// </summary>
    public static void VanillaHammerSlopingLogic(Point p)
    {
        int x = p.X;
        int y = p.Y;
        Tile tile = Main.tile[p];

        if (TileID.Sets.Platforms[Main.tile[x, y].TileType])
        {
            if (tile.IsHalfBlock)
            {
                WorldGen.PoundTile(x, y);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, x, y, 1f);
            }
            else
            {
                SlopeType SlopeA = SlopeType.SlopeDownLeft;
                SlopeType SlopeB = SlopeType.SlopeDownRight;
                if (TileID.Sets.Platforms[Main.tile[x + 1, y - 1].TileType] || TileID.Sets.Platforms[Main.tile[x - 1, y + 1].TileType] || (WorldGen.SolidTile(x + 1, y) && !WorldGen.SolidTile(x - 1, y)))
                {
                    SlopeA = SlopeType.SlopeDownRight;
                    SlopeB = SlopeType.SlopeDownLeft;
                }
                if (Main.tile[x, y].Slope == 0)
                {
                    WorldGen.SlopeTile(x, y, (int)SlopeA);
                    SlopeType mySlope = Main.tile[x, y].Slope;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, x, y, (int)mySlope);
                }
                else if (Main.tile[x, y].Slope == SlopeA)
                {
                    WorldGen.SlopeTile(x, y, (int)SlopeB);
                    SlopeType mySlope = Main.tile[x, y].Slope;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, x, y, (int)mySlope);
                }
                else
                {
                    WorldGen.SlopeTile(x, y);
                    SlopeType mySlope = Main.tile[x, y].Slope;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, x, y, (int)mySlope);
                    WorldGen.PoundTile(x, y);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, x, y, 1f);
                }
            }
        }
        else
        {
            switch (Main.tile[x, y].TileType)
            {
                case 314:
                    if (Minecart.FrameTrack(x, y, pound: true) && Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 15, x, y, 1f);
                    break;
                case 137:
                    int frameNum = 0;
                    switch (Main.tile[x, y].TileFrameY / 18)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 5:
                            switch (Main.tile[x, y].TileFrameX / 18)
                            {
                                case 0:
                                    frameNum = 2;
                                    break;
                                case 1:
                                    frameNum = 3;
                                    break;
                                case 2:
                                    frameNum = 4;
                                    break;
                                case 3:
                                    frameNum = 5;
                                    break;
                                case 4:
                                    frameNum = 1;
                                    break;
                                case 5:
                                    frameNum = 0;
                                    break;
                            }
                            break;
                        case 3:
                        case 4:
                            switch (Main.tile[x, y].TileFrameX / 18)
                            {
                                case 0:
                                case 1:
                                    frameNum = 3;
                                    break;
                                case 3:
                                    frameNum = 2;
                                    break;
                                case 2:
                                    frameNum = 4;
                                    break;
                                case 4:
                                    frameNum = 0;
                                    break;
                            }
                            break;
                    }
                    Main.tile[x, y].TileFrameX = (short)(frameNum * 18);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendTileSquare(-1, x, y);
                    break;
                case 424:
                    if (Main.tile[x, y].TileFrameX == 0)
                    Main.tile[x, y].TileFrameX = 18;
                    else if (Main.tile[x, y].TileFrameX == 18)
                    Main.tile[x, y].TileFrameX = 36;
                    else
                    {
                        Main.tile[x, y].TileFrameX = 0;
                    }
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendTileSquare(-1, x, y);
                    break;
                case 442:
                    Tile aboveTile = Main.tile[x, y - 1];
                    Tile belowTile = Main.tile[x, y + 1];
                    Tile leftTIle = Main.tile[x - 1, y];
                    Tile rightTile = Main.tile[x + 1, y];
                    Tile bottomLeft = Main.tile[x - 1, y + 1];
                    Tile bottomRight = Main.tile[x + 1, y + 1];
                    Tile topRight = Main.tile[x - 1, y - 1];
                    Tile topLeft = Main.tile[x + 1, y - 1];
                    int num6 = -1;
                    int num7 = -1;
                    int num8 = -1;
                    int num9 = -1;
                    int tree = -1;
                    int tree2 = -1;
                    int tree3 = -1;
                    int tree4 = -1;
                    if (aboveTile != null && aboveTile.HasTile && !aboveTile.BottomSlope)
                    num7 = aboveTile.TileType;
                    if (belowTile != null && belowTile.HasTile && !belowTile.IsHalfBlock && !belowTile.TopSlope)
                    num6 = belowTile.TileType;
                    if (leftTIle != null && leftTIle.HasTile && (leftTIle.Slope == 0 || (int)leftTIle.Slope % 2 != 1))
                    num8 = leftTIle.TileType;
                    if (rightTile != null && rightTile.HasTile && (rightTile.Slope == 0 || (int)rightTile.Slope % 2 != 0))
                    num9 = rightTile.TileType;
                    if (bottomLeft != null && bottomLeft.HasTile)
                    tree = bottomLeft.TileType;
                    if (bottomRight != null && bottomRight.HasTile)
                    tree2 = bottomRight.TileType;
                    if (topRight != null && topRight.HasTile)
                    tree3 = topRight.TileType;
                    if (topLeft != null && topLeft.HasTile)
                    tree4 = topLeft.TileType;
                    bool flag = (num7 >= 0 && Main.tileSolid[num7] && (!Main.tileNoAttach[num7] || (TileID.Sets.Platforms[num7] && aboveTile.IsHalfBlock)) && (aboveTile.TopSlope || aboveTile.Slope == 0 || aboveTile.IsHalfBlock));
                    bool flag2 = ((num8 >= 0 && Main.tileSolid[num8] && !Main.tileNoAttach[num8] && (leftTIle.LeftSlope || leftTIle.Slope == 0) && !leftTIle.IsHalfBlock) || (num8 >= 0 && TileID.Sets.IsBeam[num8]) || (WorldGen.IsTreeType(num8) && WorldGen.IsTreeType(tree3) && WorldGen.IsTreeType(tree)));
                    bool flag3 = ((num9 >= 0 && Main.tileSolid[num9] && !Main.tileNoAttach[num9] && (rightTile.RightSlope || rightTile.Slope == 0) && !rightTile.IsHalfBlock) || (num9 >= 0 && TileID.Sets.IsBeam[num9]) || (WorldGen.IsTreeType(num9) && WorldGen.IsTreeType(tree4) && WorldGen.IsTreeType(tree2)));
                    bool flag4 = (num6 >= 0 && Main.tileSolid[num6] && (!Main.tileNoAttach[num6] || TileID.Sets.Platforms[num6]) && (belowTile.BottomSlope || belowTile.Slope == 0) && !belowTile.IsHalfBlock);
                    
                    int num10 = Main.tile[x, y].TileFrameX / 22;
                    short num11 = -2;
                    switch (num10)
                    {
                        case 0:
                            num11 = (short)((!flag2) ? (flag ? 1 : ((!flag3) ? (-1) : 3)) : 2);
                            break;
                        case 2:
                            num11 = (short)(flag ? 1 : ((!flag3) ? ((!flag4) ? (-1) : 0) : 3));
                            break;
                        case 1:
                            num11 = (short)((!flag3) ? ((!flag4) ? ((!flag2) ? (-1) : 2) : 0) : 3);
                            break;
                        case 3:
                            num11 = (short)((!flag4) ? ((!flag2) ? (flag ? 1 : (-1)) : 2) : 0);
                            break;
                    }
                    if (num11 != -1)
                    {
                        if (num11 == -2)
                            num11 = 0;
                        Main.tile[x, y].TileFrameX = (short)(22 * num11);
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendTileSquare(-1, x, y);
                    }
                    break;
            default:
                if ((tile.IsHalfBlock || (int)tile.Slope != 0) && !Main.tileSolidTop[tile.TileType])
                {
                    int num12 = 1;
                    int num13 = 1;
                    int num14 = 2;
                    if ((WorldGen.SolidTile(x + 1, y) || (int)Main.tile[x + 1, y].Slope == 1 || (int)Main.tile[x + 1, y].Slope == 3) && !WorldGen.SolidTile(x - 1, y))
                    {
                        num13 = 2;
                        num14 = 1;
                    }
                    if (WorldGen.SolidTile(x, y - 1) && !WorldGen.SolidTile(x, y + 1))
                        num12 = -1;
                    if (num12 == 1)
                    {
                        if (tile.Slope == SlopeType.Solid)
                            WorldGen.SlopeTile(x, y, num13);
                        else if ((int)tile.Slope == num13)
                            WorldGen.SlopeTile(x, y, num14);
                        else if ((int)tile.Slope == num14)
                            WorldGen.SlopeTile(x, y, num13 + 2);
                        else if ((int)tile.Slope == num13 + 2)
                            WorldGen.SlopeTile(x, y, num14 + 2);
                        else
                            WorldGen.SlopeTile(x, y);
                    }
                    else if ((int)tile.Slope == 0)
                        WorldGen.SlopeTile(x, y, num13 + 2);
                    else if ((int)tile.Slope == num13 + 2)
                        WorldGen.SlopeTile(x, y, num14 + 2);
                    else if ((int)tile.Slope == num14 + 2)
                        WorldGen.SlopeTile(x, y, num13);
                    else if ((int)tile.Slope == num13)
                        WorldGen.SlopeTile(x, y, num14);
                    else
                        WorldGen.SlopeTile(x, y);
                    int num15 = (int)tile.Slope;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, x, y, num15);
                }
                else
                {
                    WorldGen.PoundTile(x, y);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 7, x, y, 1f);
                }
                break;
        }
        }
    }
}

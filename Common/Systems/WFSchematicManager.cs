using CalamityMod.Schematics;
using CalamityMod.TileEntities;
using CalamityMod.Tiles.DraedonStructures;
using System.Reflection;
using static CalamityMod.Schematics.SchematicManager;

namespace Windfall.Common.Systems;

public class WFSchematicManager : ModSystem
{
    private const string StructureFilePath = "Content/World/Schematics/";

    internal const string WanderersCabinKey = "Wanderers Cabin";
    internal const string WanderersCabinKeyFilename = StructureFilePath + "WanderersCabin.csch";

    internal const string LunarCultKey = "Lunar Cult Base";
    internal const string LunarCultFileName = StructureFilePath + "LunarCultBase.csch";

    internal const string TileEntityTestKey = "Tile Entity Test";
    internal const string TileEntityTestFileName = StructureFilePath + "TileEntityTest.csch";

    internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
        typeof(SchematicManager).GetField("TileMaps", Utilities.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;

    internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.UniversalBindingFlags);


    public override void OnModLoad()
    {
        TileMaps[WanderersCabinKey] = LoadWindfallSchematic(WanderersCabinKeyFilename);
        //TileMaps[SummoningGroundsKey] = LoadWindfallSchematic(SummoningGroundsKeyFilename);
        TileMaps[LunarCultKey] = LoadWindfallSchematic(LunarCultFileName);
        TileMaps[TileEntityTestKey] = LoadWindfallSchematic(TileEntityTestFileName);
    }

    public static SchematicMetaTile[,] LoadWindfallSchematic(string filename)
    {
        SchematicMetaTile[,] ret = null;
        using (Stream st = Windfall.Instance.GetFileStream(filename, true))
            ret = (SchematicMetaTile[,])ImportSchematicMethod.Invoke(null, [st]);
        return ret;
    }

    public static void PlaceFlippableSchematic<T>(string name, Point pos, SchematicAnchor anchorType, ref bool specialCondition, T chestDelegate = null, bool flipHorizontal = false) where T : Delegate
    {
        // If no schematic exists with this name, cancel with a helpful log message.
        if (!TileMaps.TryGetValue(name, out SchematicMetaTile[,] schematic))
        {
            Windfall.Instance.Logger.Warn($"Tried to place a schematic with name \"{name}\". No matching schematic file found.");
            return;
        }

        // Invalid chest interaction delegates need to throw an error.
        if (chestDelegate != null &&
            chestDelegate is not Action<Chest> &&
            chestDelegate is not Action<Chest, int, bool>)
        {
            throw new ArgumentException("The chest interaction function has invalid parameters.", nameof(chestDelegate));
        };

        ((Dictionary<string, PilePlacementFunction>)typeof(SchematicManager).GetField("PilePlacementMaps", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).TryGetValue(name, out PilePlacementFunction pilePlacementFunction);

        // Grab the schematic itself from the dictionary of loaded schematics.
        int width = schematic.GetLength(0);
        int height = schematic.GetLength(1);

        // Calculate the appropriate location to start laying down schematic tiles.
        int cornerX = pos.X;
        int cornerY = pos.Y;
        switch (anchorType)
        {
            case SchematicAnchor.TopLeft: // Provided point is top-left corner = No change
            case SchematicAnchor.Default: // This is also default behavior
            default:
                break;
            case SchematicAnchor.TopCenter: // Provided point is top center = Top-left corner is 1/2 width to the left
                cornerX -= width / 2;
                break;
            case SchematicAnchor.TopRight: // Provided point is top-right corner = Top-left corner is 1 width to the left
                cornerX -= width;
                break;
            case SchematicAnchor.CenterLeft: // Provided point is left center: Top-left corner is 1/2 height above
                cornerY -= height / 2;
                break;
            case SchematicAnchor.Center: // Provided point is center = Top-left corner is 1/2 width and 1/2 height up-left
                cornerX -= width / 2;
                cornerY -= height / 2;
                break;
            case SchematicAnchor.CenterRight: // Provided point is right center: Top-left corner is 1 width and 1/2 height up-left
                cornerX -= width;
                cornerY -= height / 2;
                break;
            case SchematicAnchor.BottomLeft: // Provided point is bottom-left corner = Top-left corner is 1 height above
                cornerY -= height;
                break;
            case SchematicAnchor.BottomCenter: // Provided point is bottom center: Top-left corner is 1/2 width and 1 height up-left
                cornerX -= width / 2;
                cornerY -= height;
                break;
            case SchematicAnchor.BottomRight: // Provided point is bottom-right corner = Top-left corner is 1 width to the left and 1 height above
                cornerX -= width;
                cornerY -= height;
                break;
        }

        // Make sure that all four corners of the target area are actually in the world.
        if (!WorldGen.InWorld(cornerX, cornerY) || !WorldGen.InWorld(cornerX + width, cornerY + height))
        {
            Windfall.Instance.Logger.Warn("Schematic failed to place: Part of the target location is outside the game world.");
            return;
        }

        // Make an array for the tiles that used to be where this schematic will be pasted.
        SchematicMetaTile[,] originalTiles = new SchematicMetaTile[width, height];

        // Schematic area pre-processing has three steps.
        // Step 1: Kill all trees and cacti specifically. This prevents ugly tree/cactus pieces from being restored later.
        // Step 2: Fill the original tiles array with everything that was originally in the target rectangle.
        // Step 3: Destroy everything in the target rectangle (except chests -- that'll cause infinite recursion).
        // The third step is necessary so that multi tiles on the edge of the region are properly destroyed (e.g. Life Crystals).

        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y)
            {
                Tile t = Main.tile[x + cornerX, y + cornerY];
                if (t.TileType == TileID.Trees || t.TileType == TileID.PineTree || t.TileType == TileID.Cactus)
                    WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);
            }

        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y)
            {
                Tile t = Main.tile[x + cornerX, y + cornerY];
                originalTiles[x, y] = new SchematicMetaTile(t);
            }

        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y)
            {
                ushort TileType = (ushort)typeof(SchematicMetaTile).GetField("TileType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(originalTiles[x, y]);
                if (TileType != TileID.Containers)
                    WorldGen.KillTile(x + cornerX, y + cornerY, noItem: true);
            }

        // Lay down the schematic. If the schematic calls for it, bring back tiles that are stored in the old tiles array.               
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y)
            {
                //Uses the tile opposite where its placing when the schematic is flipped horizontally
                SchematicMetaTile smt = schematic[flipHorizontal ? width - 1 - x : x, y];
                smt.ApplyTo(x + cornerX, y + cornerY, originalTiles[x, y]);
                Tile worldTile = Main.tile[x + cornerX, y + cornerY];

                //Handle any additional fixes that need to be made as a result of this structure being flipped horizontally
                if (flipHorizontal && !smt.keepTile)
                {
                    //Turns Left Slopes into Right Slopes and vice versa.
                    if(worldTile.Slope != SlopeType.Solid)
                        worldTile.Slope += (int)worldTile.Slope % 2 == 0 ? -1 : 1;

                    //Correct Multi-Tile TileFrames
                    int style = 0, alt = 0;
                    TileObjectData.GetTileInfo(worldTile, ref style, ref alt);
                    TileObjectData data = TileObjectData.GetTileData(worldTile.TileType, style, alt);
                    if (data != null && !TileID.Sets.Platforms[worldTile.TileType])
                    {
                        int sheetSquare = 16 + data.CoordinatePadding;

                        if (data.Width > 1)
                        {
                            int frameNum = worldTile.TileFrameX / sheetSquare % data.Width;
                            //This equation gets us the amount we need to move TileFrameX to correct any issues MultiTiles that occurred during the flip process
                            worldTile.TileFrameX += (short)((-frameNum + (data.Width - (frameNum + 1))) * (16 + data.CoordinatePadding));
                        }

                        //Flips Tiles that place directionally (Chairs, Beds, ect.)                        
                        if (data.Direction != Terraria.Enums.TileObjectDirection.None)// && !ValidTileEntityTypes.Contains(worldTile.TileType))
                        {
                            int range = 1;
                            if (data.RandomStyleRange > range)
                                range = data.RandomStyleRange;
                            if (worldTile.TileFrameX / sheetSquare % (data.Width * data.StyleMultiplier * range) < data.Width)
                                worldTile.TileFrameX += (short)(sheetSquare * data.Width);
                            else
                                worldTile.TileFrameX -= (short)(sheetSquare * data.Width);
                        }
                    }
                    //Fix Platform TileFrames
                    else if (TileID.Sets.Platforms[worldTile.TileType])
                        switch (worldTile.TileFrameX / 18)
                        {
                            case 1:
                                worldTile.TileFrameX += 18;
                                break;
                            case 2:
                                worldTile.TileFrameX -= 18;
                                break;
                            case 3:
                                worldTile.TileFrameX += 18;
                                break;
                            case 4:
                                worldTile.TileFrameX -= 18;
                                break;
                            case 8:
                                worldTile.TileFrameX += 36;
                                break;
                            case 10:
                                worldTile.TileFrameX -= 36;
                                break;
                            case 12:
                                worldTile.TileFrameX += 18;
                                break;
                            case 13:
                                worldTile.TileFrameX -= 18;
                                break;
                            case 15:
                                worldTile.TileFrameX += 18;
                                break;
                            case 16:
                                worldTile.TileFrameX -= 18;
                                break;
                            case 19:
                                worldTile.TileFrameX += 18;
                                break;
                            case 20:
                                worldTile.TileFrameX -= 18;
                                break;
                            case 25:
                                worldTile.TileFrameX += 18;
                                break;
                            case 26:
                                worldTile.TileFrameX -= 18;
                                break;
                        }
                    //A handful of tiles do not have any TileObjectData and/or are unqiuely sheeted. Because of this we need to correct their frames manually
                    //Note that this is not a comprehensive list. So far only tiles which appear in existing Calamity Structures are here. There may be other tiles which need to be added in the future.
                    else
                    {
                        switch (worldTile.TileType)
                        {
                            case TileID.Pots: //Vanilla pots do not have any TileObjectData, however Modded pots which do will be caught by the above conditions.
                                if (worldTile.TileFrameX / 18 == 0)
                                    worldTile.TileFrameX += 18;
                                else
                                    worldTile.TileFrameX -= 18;
                                break;
                            case TileID.HolidayLights: //Christmas Lights must be fixed when facing Left or Right
                                if (worldTile.TileFrameY / 18 == 3)
                                    worldTile.TileFrameY -= 18;
                                else if (worldTile.TileFrameY / 18 == 2)
                                    worldTile.TileFrameY += 18;
                                break;
                            case TileID.MinecartTrack: //Minecart Tracks are very similar to Platforms, and must be dealt with accordingly. Strangely, their TileFrameX and Y are handled very differently when compared to most other tiles.
                                switch (worldTile.TileFrameX)
                                {
                                    case 2:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 3:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 4:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 5:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 6:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 7:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 8:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 9:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 14:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 15:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 18:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 19:
                                        worldTile.TileFrameX--;
                                        break;
                                    case 24:
                                        worldTile.TileFrameX++;
                                        break;
                                    case 25:
                                        worldTile.TileFrameX--;
                                        break;
                                }
                                if (worldTile.TileFrameY == 8)
                                    worldTile.TileFrameY++;
                                else if (worldTile.TileFrameY == 9)
                                    worldTile.TileFrameY--;
                                break;
                            case TileID.ExposedGems: //Similarly to Holiday Lights, we need to determine if Exposed Gems are facing to the left or right and then flip them. However unlike holiday lights, Exposed Gems have 3 variants we must also account for. We do this by multiplying the standard size of 18 by three when dividing the TileFrameY
                                if (worldTile.TileFrameY / 54 == 3)
                                    worldTile.TileFrameY -= 54;
                                else if (worldTile.TileFrameY / 54 == 2)
                                    worldTile.TileFrameY += 54;
                                break;
                            case TileID.Trees: //Trees need to be corrected manually as their sheets are pretty much wholely unique.
                                if (worldTile.TileFrameY / 22 >= 9)
                                {
                                    if (worldTile.TileFrameX / 22 == 1)
                                        break;
                                    else if (worldTile.TileFrameX / 22 == 2)
                                        worldTile.TileFrameX += 22;
                                    else if (worldTile.TileFrameX / 22 == 3)
                                        worldTile.TileFrameX -= 22;
                                }
                                else
                                {
                                    switch (worldTile.TileFrameX / 22)
                                    {
                                        case 0:
                                            if (worldTile.TileFrameY / 22 > 5)
                                                worldTile.TileFrameX += 66;
                                            break;
                                        case 1:
                                            worldTile.TileFrameX += 22;
                                            break;
                                        case 2:
                                            worldTile.TileFrameX -= 22;
                                            break;
                                        case 3:
                                            worldTile.TileFrameX += 22;
                                            if (worldTile.TileFrameY / 22 < 3)
                                                worldTile.TileFrameY += 66;
                                            else if (worldTile.TileFrameY / 18 < 6)
                                                worldTile.TileFrameY -= 66;
                                            break;
                                        case 4:
                                            worldTile.TileFrameX -= 22;
                                            if (worldTile.TileFrameY / 22 < 3)
                                                worldTile.TileFrameY += 66;
                                            else if (worldTile.TileFrameY / 22 < 6)
                                                worldTile.TileFrameY -= 66;
                                            else if (worldTile.TileFrameY / 22 >= 6)
                                                worldTile.TileFrameX -= 66;
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }

                // If the determined tile type is a chest and this is its top left corner, define it appropriately.
                // Skip this step if this schematic position preserves tiles.
                bool isChest = worldTile.TileType == TileID.Containers || TileID.Sets.BasicChest[worldTile.TileType];
                if (!smt.keepTile && isChest && worldTile.TileFrameX % 36 == 0 && worldTile.TileFrameY == 0)
                {
                    // If a chest already exists "near" this position, then the corner was likely already defined.
                    // Do not do anything if a chest was already defined.
                    // FindChestByGuessing checks a 2x2 space starting in the given position, so nudge it up and left by 1.
                    int chestIndex = Chest.FindChestByGuessing(x + cornerX - 1, y + cornerY - 1);
                    if (chestIndex == -1)
                    {
                        chestIndex = Chest.CreateChest(x + cornerX, y + cornerY, -1);
                        Chest chest = Main.chest[chestIndex];
                        // Use the appropriate chest delegate function to fill the chest.
                        if (chestDelegate is Action<Chest, int, bool>)
                        {
                            (chestDelegate as Action<Chest, int, bool>)?.Invoke(chest, worldTile.TileType, specialCondition);
                            specialCondition = true;
                        }
                        else if (chestDelegate is Action<Chest>)
                            (chestDelegate as Action<Chest>)?.Invoke(chest);
                    }
                }

                // Now that the tile data is correctly set, place appropriate tile entities.
                TryToPlaceTileEntities(x + cornerX, y + cornerY, worldTile);

                // Activate the pile placement function if defined.
                Rectangle placeInArea = new(x, y, width, height);
                pilePlacementFunction?.Invoke(x + cornerX, y + cornerY, placeInArea);
            }
    }
    private static readonly List<int> ValidTileEntityTypes =
    [
        ModContent.TileType<ChargingStation>(),
        ModContent.TileType<DraedonLabTurret>(),
        ModContent.TileType<LabHologramProjector>(),
        ModContent.TileType<HostileFireTurret>(),
        ModContent.TileType<HostileIceTurret>(),
        ModContent.TileType<HostileLaserTurret>(),
        ModContent.TileType<HostileOnyxTurret>(),
        ModContent.TileType<HostilePlagueTurret>(),
        ModContent.TileType<HostileWaterTurret>()
    ];
    private static void TryToPlaceTileEntities(int x, int y, Tile t)
    {
        int tileType = t.TileType;

        if (!ValidTileEntityTypes.Contains(tileType))
            return;
        
        int index = ValidTileEntityTypes.IndexOf(tileType);

        int FrameX, FrameY;
        int style = 0, alt = 0;
        TileObjectData.GetTileInfo(t, ref style, ref alt);
        TileObjectData data = TileObjectData.GetTileData(t.TileType, style, alt);
        if (data != null)
        {
            int sheetSquare = 16 + data.CoordinatePadding;
            FrameX = t.TileFrameX / sheetSquare % data.Width;
            FrameY = t.TileFrameY / sheetSquare % data.Height;
        }
        else
        {
            FrameX = t.TileFrameX;
            FrameY = t.TileFrameY;
        }

        if (t.HasTile && FrameX == 0 && FrameY == 0)
        {
            switch (index)
            {
                case 0:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEChargingStation>());                   
                    break;
                case 1:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLabTurret>());
                    break;
                case 2:
                    Windfall.Instance.Logger.Debug($"Attempting to place Projector with frame data: {t.TileFrameX}, {t.TileFrameY}");
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TELabHologramProjector>());                    
                    break;
                case 3:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileFireTurret>());
                    break;
                case 4:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileIceTurret>());
                    break;
                case 5:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLaserTurret>());
                    break;
                case 6:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileOnyxTurret>());
                    break;
                case 7:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostilePlagueTurret>());
                    break;
                case 8:
                    TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileWaterTurret>());
                    break;
            }
        }
    }
}


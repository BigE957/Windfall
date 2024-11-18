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
    internal const string LunarCultKeyFileName = StructureFilePath + "LunarCultBase.csch";
    internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
        typeof(SchematicManager).GetField("TileMaps", Utilities.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;

    internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.UniversalBindingFlags);


    public override void OnModLoad()
    {
        TileMaps[WanderersCabinKey] = LoadWindfallSchematic(WanderersCabinKeyFilename);
        //TileMaps[SummoningGroundsKey] = LoadWindfallSchematic(SummoningGroundsKeyFilename);
        TileMaps[LunarCultKey] = LoadWindfallSchematic(LunarCultKeyFileName);
    }

    public static SchematicMetaTile[,] LoadWindfallSchematic(string filename)
    {
        SchematicMetaTile[,] ret = null;
        using (Stream st = Windfall.Instance.GetFileStream(filename, true))
            ret = (SchematicMetaTile[,])ImportSchematicMethod.Invoke(null, new object[] { st });
        return ret;
    }

    public static void PlaceFlippableSchematic<T>(string name, Point pos, SchematicAnchor anchorType, ref bool specialCondition, T chestDelegate = null, bool flipHorizontal = false) where T : Delegate
    {
        // If no schematic exists with this name, cancel with a helpful log message.
        if (!TileMaps.TryGetValue(name, out SchematicMetaTile[,] value))
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
        SchematicMetaTile[,] schematic = value;
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

        //Flip schematic as needed
        /*
        if (flipHorizontal)
        {
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width / 2; ++x)               
                {
                    //tuple swap the two tiles
                    (schematic[x, y], schematic[width - 1 - x, y]) = (schematic[width - 1 - x, y], schematic[x, y]);
                }
        }
        */

        // Lay down the schematic. If the schematic calls for it, bring back tiles that are stored in the old tiles array.               
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < height; ++y)
            {
                SchematicMetaTile smt = schematic[flipHorizontal ? width - 1 - x : x, y];
                smt.ApplyTo(x + cornerX, y + cornerY, originalTiles[x, y]);
                Tile worldTile = Main.tile[x + cornerX, y + cornerY];

                if (flipHorizontal && !smt.keepTile)
                {
                    //Flip any slopes when placing blocks as needed
                    switch (worldTile.Slope)
                    {
                        case SlopeType.SlopeUpLeft:
                            worldTile.Slope = SlopeType.SlopeUpRight;
                            break;
                        case SlopeType.SlopeDownLeft:
                            worldTile.Slope = SlopeType.SlopeDownRight;
                            break;
                        case SlopeType.SlopeUpRight:
                            worldTile.Slope = SlopeType.SlopeUpLeft;
                            break;
                        case SlopeType.SlopeDownRight:
                            worldTile.Slope = SlopeType.SlopeDownLeft;
                            break;
                    }

                    //Correct Multi-Tile TileFrames
                    int style = 0, alt = 0;
                    TileObjectData.GetTileInfo(worldTile, ref style, ref alt);
                    TileObjectData data = TileObjectData.GetTileData(worldTile.TileType, style, alt);
                    if (data != null && !TileID.Sets.Platforms[worldTile.TileType])
                    {
                        switch(data.Width)
                        {
                            case 1:
                                if(TileID.Sets.CanBeSatOnForNPCs[worldTile.TileType])
                                {
                                    if(worldTile.TileFrameX / 18 == 1)
                                        worldTile.TileFrameX -= 18;
                                    else
                                        worldTile.TileFrameX += 18;
                                }
                                break;
                            case 2:
                                if (worldTile.TileFrameX / 18 % data.Width == 1)
                                    worldTile.TileFrameX -= 18;
                                else
                                    worldTile.TileFrameX += 18;
                                break;
                            case 3:                                
                                if (worldTile.TileFrameX / 18 % data.Width == 2)
                                    worldTile.TileFrameX -= 36;
                                else if (worldTile.TileFrameX / 18 % data.Width != 1)
                                    worldTile.TileFrameX += 36;
                                break;
                        }
                        
                    }
                    //Fix Platform TileFrames
                    else if(TileID.Sets.Platforms[worldTile.TileType])
                    {
                        switch(worldTile.TileFrameX / 18)
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
                            case 25:
                                worldTile.TileFrameX += 18;
                                break;
                            case 26:
                                worldTile.TileFrameX -= 18;
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
                Rectangle placeInArea = new Rectangle(x, y, width, height);
                pilePlacementFunction?.Invoke(x + cornerX, y + cornerY, placeInArea);
            }
    }

    private static void TryToPlaceTileEntities(int x, int y, Tile t)
    {
        if (t.HasTile && t.TileFrameX == 0 && t.TileFrameY == 0)
        {
            int tileType = t.TileType;
            if (tileType == ModContent.TileType<ChargingStation>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEChargingStation>());
            }
            else if (tileType == ModContent.TileType<DraedonLabTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLabTurret>());
            }
            else if (tileType == ModContent.TileType<LabHologramProjector>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TELabHologramProjector>());
            }
            else if (tileType == ModContent.TileType<HostileFireTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileFireTurret>());
            }
            else if (tileType == ModContent.TileType<HostileIceTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileIceTurret>());
            }
            else if (tileType == ModContent.TileType<HostileLaserTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileLaserTurret>());
            }
            else if (tileType == ModContent.TileType<HostileOnyxTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileOnyxTurret>());
            }
            else if (tileType == ModContent.TileType<HostilePlagueTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostilePlagueTurret>());
            }
            else if (tileType == ModContent.TileType<HostileWaterTurret>())
            {
                TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEHostileWaterTurret>());
            }
        }
    }
}


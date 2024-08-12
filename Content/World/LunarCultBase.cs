using CalamityMod.Schematics;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Walls.DraedonStructures;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using static CalamityMod.Schematics.SchematicManager;
using static Terraria.WorldGen;

namespace Windfall.Content.World
{
    public static class LunarCultBase
    {
        public static bool ShouldAvoidLocation(Point placementPoint, bool careAboutLava = true)
        {
            Tile tile = CalamityUtils.ParanoidTileRetrieval(placementPoint.X, placementPoint.Y);
            if (tile.LiquidType == LiquidID.Lava && careAboutLava)
                return true;
            if (tile.TileType == TileID.BlueDungeonBrick ||
            tile.TileType == TileID.GreenDungeonBrick ||
            tile.TileType == TileID.PinkDungeonBrick)
            {
                return true;
            }
            if (tile.TileType == TileID.Crimstone ||
            tile.WallType == WallID.CrimstoneUnsafe ||
            tile.TileType == TileID.Ebonstone ||
            tile.WallType == WallID.EbonstoneUnsafe)
            {
                return true;
            }

            if (tile.TileType == TileID.SnowBlock ||
            tile.WallType == WallID.SnowWallUnsafe ||
            tile.TileType == TileID.IceBlock ||
            tile.WallType == WallID.IceUnsafe)
            {
                return true;
            }

            if (tile.TileType == TileID.WoodBlock ||
            tile.WallType == WallID.Planked ||
            tile.TileType == TileID.WoodenBeam ||
            tile.WallType == WallID.SpiderUnsafe)
            {
                return true;
            }

            if (tile.TileType == ModContent.TileType<LaboratoryPlating>() ||
            tile.WallType == ModContent.WallType<LaboratoryPlatingWall>() ||
            tile.TileType == ModContent.TileType<RustedPlating>() ||
            tile.WallType == ModContent.WallType<RustedPlatingWall>() ||
            tile.TileType == ModContent.TileType<LaboratoryPanels>() ||
            tile.WallType == ModContent.WallType<LaboratoryPanelWall>())
            {
                return true;
            }

            return false;
        }
        public static void PlaceLunarCultBase(StructureMap structures)
        {
            string mapKey = "Lunar Cult Base";
            int centerPlacementPositionX;
            if(Main.maxTilesX == 8400)
                centerPlacementPositionX = Main.dungeonX > Main.maxTilesX / 2 ? Main.spawnTileX + 1200 : Main.spawnTileX - 1200;
            else
                centerPlacementPositionX = Main.dungeonX > Main.maxTilesX / 2 ? Main.spawnTileX + 1600 : Main.spawnTileX - 1600;
            SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];
            Point placementPoint;
            Vector2 schematicSize = new(WFSchematicManager.TileMaps[mapKey].GetLength(0), WFSchematicManager.TileMaps[mapKey].GetLength(1));
            int underworldTop = Main.UnderworldLayer;
            int tries = 0;
            do
            {
                int placementPositionX = genRand.Next(centerPlacementPositionX - 400, centerPlacementPositionX + 400);
                int placementPositionY = genRand.Next(underworldTop - 400, underworldTop - 200);

                placementPoint = new Point(placementPositionX, placementPositionY);

                bool canGenerateInLocation = true;

                for (int x = placementPoint.X - 20; x < placementPoint.X + schematicSize.X + 20; x++)
                {
                    for (int y = placementPoint.Y - 20; y < placementPoint.Y + schematicSize.Y + 20; y++)
                    {
                        if (ShouldAvoidLocation(new Point(x, y), false))
                            canGenerateInLocation = false;
                    }
                }

                if (!canGenerateInLocation && !structures.CanPlace(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20))
                    tries++;
                else
                {
                    SchematicAnchor anchorType = SchematicAnchor.Center;

                    bool place = true;
                    PlaceSchematic<Action<Chest>>(mapKey, placementPoint, anchorType, ref place);
                    AddProtectedStructure(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20);
                    break;
                }
                placementPoint = new(-1, -1);

            } while (tries <= 10000);

            LunarCultActivitySystem.LunarCultBaseLocation = placementPoint;
        }
    }
}
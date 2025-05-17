using CalamityMod.Schematics;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Walls;
using CalamityMod.Walls.DraedonStructures;
using Terraria;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using static Terraria.WorldGen;

namespace Windfall.Content.World;

public static class LunarCultBase
{
    public static bool ShouldAvoidLocation(Point placementPoint, bool careAboutMushroom = false)
    {
        Tile tile = ParanoidTileRetrieval(placementPoint.X, placementPoint.Y);

        // Avoid Dungeon
        if (tile.TileType == TileID.BlueDungeonBrick ||
        tile.TileType == TileID.GreenDungeonBrick ||
        tile.TileType == TileID.PinkDungeonBrick)
            return true;

        // Avoid Evil Biomes
        if (tile.TileType == TileID.Crimstone ||
        tile.WallType == WallID.CrimstoneUnsafe ||
        tile.TileType == TileID.Ebonstone ||
        tile.WallType == WallID.EbonstoneUnsafe)
            return true;

        // Avoid Tundra
        if (tile.TileType == TileID.SnowBlock ||
        tile.WallType == WallID.SnowWallUnsafe ||
        tile.TileType == TileID.IceBlock ||
        tile.WallType == WallID.IceUnsafe)
            return true;

        // Avoid Underground Houses and Spider Biomes
        if (tile.TileType == TileID.WoodBlock ||
        tile.WallType == WallID.Planked ||
        tile.TileType == TileID.WoodenBeam ||
        tile.WallType == WallID.SpiderUnsafe)
            return true;

        // Avoid Deserts
        if (tile.WallType == WallID.Sandstone ||
        tile.TileType == TileID.Sandstone ||
        tile.WallType == WallID.HardenedSand)
            return true;

        // Avoid the Sunken Sea
        if (tile.TileType == ModContent.TileType<Navystone>() ||
        tile.WallType == ModContent.WallType<NavystoneWall>() ||
        tile.TileType == ModContent.TileType<EutrophicSand>() ||
        tile.WallType == ModContent.WallType<EutrophicSandWall>())
            return true;

        if (careAboutMushroom)
        {
            if (tile.TileType == TileID.MushroomGrass)
                return true;
        }

        // Avoid Draedon Labs
        if (tile.TileType == ModContent.TileType<LaboratoryPlating>() ||
        tile.WallType == ModContent.WallType<LaboratoryPlatingWall>() ||
        tile.TileType == ModContent.TileType<RustedPlating>() ||
        tile.WallType == ModContent.WallType<RustedPlatingWall>() ||
        tile.TileType == ModContent.TileType<LaboratoryPanels>() ||
        tile.WallType == ModContent.WallType<LaboratoryPanelWall>())
            return true;

        return false;
    }
    public static void PlaceLunarCultBase(StructureMap structures)
    {
        string mapKey = "Lunar Cult Base";
        int centerPlacementPositionX;
        bool facingLeft = Main.dungeonX > Main.maxTilesX / 2;

        if (Main.maxTilesX == 8400) //Large World
            centerPlacementPositionX = facingLeft ? Main.spawnTileX + 800 : Main.spawnTileX - 800;
        else
            centerPlacementPositionX = facingLeft ? Main.spawnTileX + 500 : Main.spawnTileX - 500;
        SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];
        Point placementPoint;
        Vector2 schematicSize = new(WFSchematicManager.TileMaps[mapKey].GetLength(0), WFSchematicManager.TileMaps[mapKey].GetLength(1));
        int underworldTop = Main.UnderworldLayer;
        int tries = 0;
        do
        {
            int placementPositionX = centerPlacementPositionX + (genRand.Next(0, Main.maxTilesX == 8400 ? 2800 : 2150) * (facingLeft ? 1 : -1));
            int placementPositionY = genRand.Next(underworldTop - 820, underworldTop - (int)schematicSize.Y - 12);

            placementPoint = new Point(placementPositionX, placementPositionY);

            bool canGenerateInLocation = true;

            int buffer = 2;

            for (int x = placementPoint.X - buffer; x < placementPoint.X + schematicSize.X + (buffer * 2); x++)
            {
                for (int y = placementPoint.Y - buffer; y < placementPoint.Y + schematicSize.Y + (buffer * 2); y++)
                {
                    if (ShouldAvoidLocation(new Point(x, y), tries < 10000))
                    {
                        canGenerateInLocation = false;
                        break;
                    }
                }
                if (!canGenerateInLocation)
                    break;
            }
            if (!canGenerateInLocation || !structures.CanPlace(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), buffer))
                tries++;
            else
            {
                SchematicAnchor anchorType = SchematicAnchor.TopLeft;

                bool place = true;
                WFSchematicManager.PlaceFlippableSchematic<Action<Chest>>(mapKey, placementPoint, anchorType, ref place, flipHorizontal: facingLeft);
                CalamityMod.CalamityUtils.AddProtectedStructure(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20);
                break;
            }
            placementPoint = new(-1, -1);

        } while (tries <= 20000);

        LunarCultBaseSystem.BaseFacingLeft = facingLeft;
        LunarCultBaseSystem.LunarCultBaseLocation = placementPoint + new Point(facingLeft ? (int)schematicSize.X : 0, (int)schematicSize.Y / 2 );
    }
}
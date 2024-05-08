using CalamityMod.Schematics;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using static CalamityMod.Schematics.SchematicManager;
using static Terraria.WorldGen;

namespace Windfall.Content.World
{
    public static class CultistHideouts
    {
        public static void PlaceSolarCultistHideout(StructureMap structures)
        {
            string mapKey = "Solar Hideout";
            int placementPositionX = Main.spawnTileX - 2400;

            Point placementPoint = PlaceHideout(structures, placementPositionX, mapKey);
            CultMeetingSystem.SolarHideoutLocation = placementPoint;
        }
        public static void PlaceVortexCultistHideout(StructureMap structures)
        {
            string mapKey = "Solar Hideout";

            int placementPositionX = Main.spawnTileX - 1200;

            Point placementPoint = PlaceHideout(structures, placementPositionX, mapKey);
            CultMeetingSystem.VortexHideoutLocation = placementPoint;
        }
        public static void PlaceNebulaCultistHideout(StructureMap structures)
        {
            string mapKey = "Solar Hideout";

            int placementPositionX = Main.spawnTileX + 1200;

            Point placementPoint = PlaceHideout(structures, placementPositionX, mapKey);
            CultMeetingSystem.NebulaHideoutLocation = placementPoint;
        }
        public static void PlaceStardustCultistHideout(StructureMap structures)
        {
            string mapKey = "Solar Hideout";

            int placementPositionX = Main.spawnTileX + 2400;

            Point placementPoint = PlaceHideout(structures, placementPositionX, mapKey);
            CultMeetingSystem.StardustHideoutLocation = placementPoint;
        }
        private static Point PlaceHideout(StructureMap structures, int placementPositionX, string mapKey)
        {
            SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];
            Point placementPoint;
            Vector2 schematicSize = new(WFSchematicManager.TileMaps[mapKey].GetLength(0), WFSchematicManager.TileMaps[mapKey].GetLength(1));
            int underworldTop = Main.maxTilesY - 200;
            int tries = 0;
            do
            {
                int placementPositionY = genRand.Next(underworldTop - 1100, underworldTop - 150);

                placementPoint = new Point(placementPositionX, placementPositionY);
                if (!structures.CanPlace(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y)))
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
            return placementPoint;
        }
    }
}
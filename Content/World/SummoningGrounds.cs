using CalamityMod.Schematics;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Content.Items.Journals;
using static CalamityMod.Schematics.SchematicManager;

namespace Windfall.Content.World
{
    public static class SummoningGrounds
    {
        public static void PlaceSummoningGrounds(StructureMap structures)
        {
            string mapKey = "Summoning Grounds";
            SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];

            bool DungeonLeft = Main.dungeonX < Main.maxTilesX / 2;

            int placementPositionX;
            if (!DungeonLeft)
                placementPositionX = WorldGen.genRand.Next(Main.dungeonX - 400, Main.dungeonX - 100);
            else
                placementPositionX = WorldGen.genRand.Next(Main.dungeonX + 100, Main.dungeonX + 400);

            int placementPositionY = (int)Main.worldSurface - (Main.maxTilesY / 6);

            bool foundValidGround = false;
            int attempts = 0;
            while (!foundValidGround && attempts++ < 100000)
            {
                while (!WorldGen.SolidTile(placementPositionX, placementPositionY) && placementPositionY <= Main.worldSurface)
                {
                    placementPositionY++;
                }

                if (Main.tile[placementPositionX, placementPositionY].HasTile || Main.tile[placementPositionX, placementPositionY].WallType > 0)
                {
                    foundValidGround = true;
                }
            }

            Point placementPoint = new(placementPositionX, placementPositionY);

            //EyeCultistsSpawnSystem.GroundsLocation = placementPoint;

            Vector2 schematicSize = new(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.BottomCenter;

            bool place = true;
            PlaceSchematic(mapKey, placementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillSummoningGroundsChest));

            Rectangle protectionArea = GetSchematicProtectionArea(schematic, placementPoint, anchorType);
            AddProtectedStructure(protectionArea, 30);
        }

        private static void FillSummoningGroundsChest(Chest chest, int Type, bool place)
        {
            List<ChestItem> contents = new()
            {
                new ChestItem(ModContent.ItemType<JournalDungeon>(), 1),
            };

            for (int i = 0; i < contents.Count; i++)
            {
                chest.item[i].SetDefaults(contents[i].Type);
                chest.item[i].stack = contents[i].Stack;
            }
        }
    }
}
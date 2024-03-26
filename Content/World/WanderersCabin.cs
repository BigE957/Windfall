using CalamityMod.Schematics;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Content.Items.Journals;
using static CalamityMod.Schematics.SchematicManager;

namespace Windfall.Content.World
{
    public struct ChestItem
    {
        internal int Type;

        internal int Stack;

        internal ChestItem(int type, int stack)
        {
            Type = type;
            Stack = stack;
        }
    }
    public static class WanderersCabin
    {
        public static void PlaceWanderersCabin(StructureMap structures)
        {
            string mapKey = "Wanderers Cabin";
            SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];

            bool desertLeft = Main.dungeonX < Main.maxTilesX / 2;

            int placementPositionX;
            if (!desertLeft)
                placementPositionX = WorldGen.genRand.Next(Main.spawnTileX - 400, Main.spawnTileX - 100);
            else
                placementPositionX = WorldGen.genRand.Next(Main.spawnTileX + 100, Main.spawnTileX + 400);

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

            Point placementPoint = new(placementPositionX, placementPositionY + 5);

            Vector2 schematicSize = new(schematic.GetLength(0), schematic.GetLength(1));
            SchematicAnchor anchorType = SchematicAnchor.BottomCenter;

            bool place = true;
            PlaceSchematic(mapKey, placementPoint, anchorType, ref place, new Action<Chest, int, bool>(FillWanderersChest));

            Rectangle protectionArea = CalamityUtils.GetSchematicProtectionArea(schematic, placementPoint, anchorType);
            CalamityUtils.AddProtectedStructure(protectionArea, 30);
        }

        private static void FillWanderersChest(Chest chest, int Type, bool place)
        {
            List<ChestItem> contents = new()
            {
                new ChestItem(ModContent.ItemType<JournalForest>(), 1),
                new ChestItem(ItemID.Binoculars, 1),
                new ChestItem(ItemID.HermesBoots, 1),
                new ChestItem(ItemID.SwiftnessPotion, WorldGen.genRand.Next(1, 3)),
                new ChestItem(ItemID.SpelunkerPotion, WorldGen.genRand.Next(1, 3)),
                new ChestItem(ItemID.GoldCoin, WorldGen.genRand.Next(1, 3)),
            };

            for (int i = 0; i < contents.Count; i++)
            {
                chest.item[i].SetDefaults(contents[i].Type);
                chest.item[i].stack = contents[i].Stack;
            }
        }
    }
}
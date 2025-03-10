using CalamityMod.Schematics;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Walls;
using CalamityMod.Walls.DraedonStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Content.Items.Journals;
using static Terraria.WorldGen;

namespace Windfall.Content.World;
public static class DragonMonkRuins
{
    public static bool ShouldAvoidLocation(Point placementPoint, bool careAboutMoss = false)
    {
        Tile tile = ParanoidTileRetrieval(placementPoint.X, placementPoint.Y);

        if (tile.LiquidType == LiquidID.Shimmer)
            return true;

        if (tile.TileType == TileID.BlueDungeonBrick ||
        tile.TileType == TileID.GreenDungeonBrick ||
        tile.TileType == TileID.PinkDungeonBrick)
            return true;

        if (tile.TileType == TileID.Crimstone ||
        tile.WallType == WallID.CrimstoneUnsafe ||
        tile.TileType == TileID.Ebonstone ||
        tile.WallType == WallID.EbonstoneUnsafe)
            return true;

        if (tile.TileType == TileID.Mud ||
        tile.WallType == WallID.MudUnsafe ||
        tile.TileType == TileID.JungleGrass ||
        tile.WallType == WallID.Jungle ||
        tile.TileType == TileID.JunglePlants ||
        tile.TileType == TileID.JunglePlants2 ||
        tile.TileType == TileID.JungleThorns ||
        tile.TileType == TileID.JungleVines ||
        tile.TileType == TileID.Hive ||
        tile.WallType == WallID.HiveUnsafe ||
        tile.TileType == TileID.LihzahrdBrick ||
        tile.WallType == WallID.LihzahrdBrickUnsafe ||
        tile.TileType == TileID.MushroomGrass)
            return true;

        if (tile.TileType == TileID.Sand ||
        tile.WallType == WallID.Sandstone ||
        tile.TileType == TileID.Sandstone ||
        tile.WallType == WallID.HardenedSand ||
        tile.TileType == TileID.HardenedSand)
            return true;

        if (tile.TileType == ModContent.TileType<Navystone>() ||
        tile.WallType == ModContent.WallType<NavystoneWall>() ||
        tile.TileType == ModContent.TileType<EutrophicSand>() ||
        tile.WallType == ModContent.WallType<EutrophicSandWall>() ||
        tile.TileType == ModContent.TileType<SmallCorals>() ||
        tile.TileType == ModContent.TileType<MediumCoral>() ||
        tile.TileType == ModContent.TileType<MediumCoral2>() ||
        tile.TileType == ModContent.TileType<CoralPileLarge>() ||
        tile.TileType == ModContent.TileType<SmallBrainCoral>() ||
        tile.TileType == ModContent.TileType<BrainCoral>() ||
        tile.TileType == ModContent.TileType<FanCoral>() ||
        tile.TileType == ModContent.TileType<TubeCoral>() ||
        tile.TileType == ModContent.TileType<SeaPrism>() ||
        tile.TileType == ModContent.TileType<SeaPrismCrystals>())
            return true;

        if (tile.TileType == ModContent.TileType<LaboratoryPlating>() ||
        tile.WallType == ModContent.WallType<LaboratoryPlatingWall>() ||
        tile.TileType == ModContent.TileType<RustedPlating>() ||
        tile.WallType == ModContent.WallType<RustedPlatingWall>() ||
        tile.TileType == ModContent.TileType<LaboratoryPanels>() ||
        tile.WallType == ModContent.WallType<LaboratoryPanelWall>())
            return true;

        if (careAboutMoss)
        {
            if (tile.TileType == TileID.ArgonMoss ||
                tile.TileType == TileID.KryptonMoss ||
                tile.TileType == TileID.LavaMoss ||
                tile.TileType == TileID.XenonMoss ||
                tile.TileType == TileID.VioletMoss)
                return true;
        }

        return false;
    }

    public static void PlaceDraconicRuins(StructureMap structures)
    {
        string mapKey = "Draconic Ruins";
        int centerPlacementPositionX;
        bool facingLeft = Main.dungeonX < Main.maxTilesX / 2;

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
            int placementPositionX = centerPlacementPositionX + (genRand.Next(0, 2200) * (facingLeft ? 1 : -1));
            int placementPositionY = genRand.Next(underworldTop - 800, underworldTop - 175);

            placementPoint = new Point(placementPositionX, placementPositionY);

            bool canGenerateInLocation = true;

            for (int x = placementPoint.X - 4; x < placementPoint.X + schematicSize.X + 4; x++)
            {
                for (int y = placementPoint.Y - 4; y < placementPoint.Y + schematicSize.Y + 4; y++)
                {
                    if (ShouldAvoidLocation(new Point(x, y), tries < 80000))
                        canGenerateInLocation = false;
                }
            }

            if (!canGenerateInLocation && !structures.CanPlace(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20))
                tries++;
            else
            {
                SchematicAnchor anchorType = SchematicAnchor.TopLeft;
                bool place = true;
                WFSchematicManager.PlaceFlippableSchematic<Action<Chest>>(mapKey, placementPoint, anchorType, ref place, flipHorizontal: facingLeft);
                AddProtectedStructure(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20);
                break;
            }
            placementPoint = new(-1, -1);

        } while (tries <= 10000);
    }

    private static void FillDraconicChests(Chest chest, int Type, bool place)
    {
        List<ChestItem> contents =
        [
            new (ModContent.ItemType<JournalForest>(), 1),
            new (ItemID.Binoculars, 1),
            new (ItemID.HermesBoots, 1),
            new (ItemID.SwiftnessPotion, genRand.Next(1, 3)),
            new (ItemID.SpelunkerPotion, genRand.Next(1, 3)),
            new (ItemID.GoldCoin, genRand.Next(1, 3)),
        ];

        for (int i = 0; i < contents.Count; i++)
        {
            chest.item[i].SetDefaults(contents[i].Type);
            chest.item[i].stack = contents[i].Stack;
        }
    }
}

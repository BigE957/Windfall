using CalamityMod.Schematics;
using CalamityMod.Tiles.DraedonStructures;
using CalamityMod.Tiles.SunkenSea;
using CalamityMod.Walls;
using CalamityMod.Walls.DraedonStructures;
using Terraria.WorldBuilding;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Journals;
using static Terraria.WorldGen;

namespace Windfall.Content.World;
public static class DragonMonkRuins
{
    public static bool ShouldAvoidLocation(Point placementPoint, bool careAboutMoss = false)
    {
        Tile tile = ParanoidTileRetrieval(placementPoint);

        if (tile.LiquidType == LiquidID.Shimmer)
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to the Shimmer");
            return true;
        }

        if (tile.TileType == TileID.WoodBlock ||
        tile.WallType == WallID.Planked ||
        tile.TileType == TileID.WoodenBeam)
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to an Underground House");
            return true;
        }

        if (tile.TileType == TileID.Crimstone ||
        tile.WallType == WallID.CrimstoneUnsafe ||
        tile.TileType == TileID.Ebonstone ||
        tile.WallType == WallID.EbonstoneUnsafe)
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to an Evil Biome");
            return true;
        }

        if (//tile.TileType == TileID.Mud ||
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
        tile.WallType == WallID.LihzahrdBrickUnsafe)
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to the Jungle");
            return true;
        }

        if (//tile.TileType == TileID.Sand ||
        tile.WallType == WallID.Sandstone ||
        tile.TileType == TileID.Sandstone ||
        tile.WallType == WallID.HardenedSand ||
        tile.TileType == TileID.HardenedSand)
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to a Desert");
            return true;
        }

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
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to the Sunken Sea");
            return true;
        }

        if (tile.TileType == ModContent.TileType<LaboratoryPlating>() ||
        tile.WallType == ModContent.WallType<LaboratoryPlatingWall>() ||
        tile.TileType == ModContent.TileType<RustedPlating>() ||
        tile.WallType == ModContent.WallType<RustedPlatingWall>() ||
        tile.TileType == ModContent.TileType<LaboratoryPanels>() ||
        tile.WallType == ModContent.WallType<LaboratoryPanelWall>())
        {
            Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to a Lab");
            return true;
        }

        if (careAboutMoss)
        {
            if (tile.TileType == TileID.ArgonMoss ||
                tile.TileType == TileID.KryptonMoss ||
                tile.TileType == TileID.LavaMoss ||
                tile.TileType == TileID.XenonMoss ||
                tile.TileType == TileID.VioletMoss ||
                tile.TileType == TileID.MushroomGrass)
            {
                Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to Moss");
                return true;
            }
        }
        return false;
    }

    public static void PlaceDraconicRuins(StructureMap structures)
    {
        string mapKey = "Draconic Ruins";
        
        bool facingLeft = Main.dungeonX < Main.maxTilesX / 2;

        int startPositionX = facingLeft ? Main.maxTilesX - 48 : 48;

        SchematicMetaTile[,] schematic = WFSchematicManager.TileMaps[mapKey];
        Point placementPoint;
        Vector2 schematicSize = new(WFSchematicManager.TileMaps[mapKey].GetLength(0), WFSchematicManager.TileMaps[mapKey].GetLength(1));
        int underworldTop = Main.UnderworldLayer;
        int tries = 0;
        do
        {
            int placementPositionX = startPositionX + (genRand.Next(0, Main.maxTilesX == 8400 ? 3000 : 2500) * (facingLeft ? -1 : 1));

            int placementPositionY = genRand.Next(underworldTop - 820, underworldTop - 100);

            placementPoint = new Point(placementPositionX, placementPositionY);

            bool canGenerateInLocation = true;

            int buffer = 2;
            for (int x = placementPoint.X - buffer; x < placementPoint.X + schematicSize.X + (buffer * 2); x++)
            {
                for (int y = placementPoint.Y - buffer; y < placementPoint.Y + schematicSize.Y + (buffer * 2); y++)
                {
                    if (ShouldAvoidLocation(new Point(x, y), tries < 2000))
                    {
                        canGenerateInLocation = false;
                        break;
                    }
                }
                if (!canGenerateInLocation)
                    break;
            }

            if (!canGenerateInLocation || !structures.CanPlace(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), buffer))
            {
                tries++;
                placementPoint = new(-1, -1);
                if(canGenerateInLocation)
                    Windfall.Instance.Logger.Debug("Draconic Ruins can't generate due to a Protected Structure or Non-standard tile");
            }
            else
            {
                Windfall.Instance.Logger.Debug("Draconic Ruins successfully generated!");
                SchematicAnchor anchorType = SchematicAnchor.TopLeft;
                bool place = true;
                WFSchematicManager.PlaceFlippableSchematic<Action<Chest>>(mapKey, placementPoint, anchorType, ref place, flipHorizontal: facingLeft);
                CalamityMod.CalamityUtils.AddProtectedStructure(new Rectangle(placementPoint.X, placementPoint.Y, (int)schematicSize.X, (int)schematicSize.Y), 20);
                break;
            }

        } while (tries <= 20000);

        DraconicRuinsSystem.DraconicRuinsLocation = placementPoint + (schematicSize / 2f).ToPoint();
        DraconicRuinsSystem.FacingLeft = facingLeft;
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

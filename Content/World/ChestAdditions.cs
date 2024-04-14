using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Tools;
using Windfall.Common.Utils;
using Windfall.Content.Items.Journals;

namespace Windfall.Content.World
{
    public static class ChestAdditions
    {
        public static void FillChestsAgain()
        {
            for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
            {

                Chest chest = Main.chest[chestIndex];
                if (chest != null)
                {
                    // Checks which sheet a chest belongs to
                    bool isContainer1 = Main.tile[chest.x, chest.y].TileType == TileID.Containers;
                    bool isContainer2 = Main.tile[chest.x, chest.y].TileType == TileID.Containers2;

                    // Pre-1.4 chests
                    bool isBrownChest = isContainer1 && Main.tile[chest.x, chest.y].TileFrameX == 0;
                    bool isGoldChest = isContainer1 && (Main.tile[chest.x, chest.y].TileFrameX == 36 || Main.tile[chest.x, chest.y].TileFrameX == 2 * 36); // Includes Locked Gold Chests
                    bool isMahoganyChest = isContainer1 && Main.tile[chest.x, chest.y].TileFrameX == 8 * 36;
                    bool isIvyChest = isContainer1 && Main.tile[chest.x, chest.y].TileFrameX == 10 * 36;
                    bool isIceChest = isContainer1 && Main.tile[chest.x, chest.y].TileFrameX == 11 * 36;
                    bool isMushroomChest = isContainer1 && Main.tile[chest.x, chest.y].TileFrameX == 32 * 36;
                    bool isMarniteChest = isContainer1 && (Main.tile[chest.x, chest.y].TileFrameX == 50 * 36 || Main.tile[chest.x, chest.y].TileFrameX == 51 * 36);

                    // 1.4 chests
                    bool isDeadManChest = isContainer2 && Main.tile[chest.x, chest.y].TileFrameX == 4 * 36;
                    bool isSandstoneChest = isContainer2 && Main.tile[chest.x, chest.y].TileFrameX == 10 * 36;


                    if (chest.item[0].type == ModContent.ItemType<FellerofEvergreens>())
                    {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].IsAir)
                            {
                                chest.item[inventoryIndex].SetDefaults(ModContent.ItemType<JournalJungle>());
                                chest.item[inventoryIndex].stack = 1;
                                break;
                            }
                        }
                    }
                    if (WindfallUtils.ChestContains(chest, ItemID.Toolbox) != -1)
                    {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].IsAir)
                            {
                                chest.item[inventoryIndex].SetDefaults(ModContent.ItemType<JournalTundra>());
                                chest.item[inventoryIndex].stack = 1;
                                break;
                            }
                        }
                    }
                    if (WindfallUtils.ChestContains(chest, ItemID.SpellTome) != -1)
                    {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                        {
                            if (chest.item[inventoryIndex].IsAir)
                            {
                                chest.item[inventoryIndex].SetDefaults(ModContent.ItemType<JournalDungeon>());
                                chest.item[inventoryIndex].stack = 1;
                                break;
                            }
                        }
                    }
                    // Replaces the Desert Medalions Calamity adds to chest with the vanity version if CalVal is on. Otherwise, murders them.
                    if (isSandstoneChest)
                    {
                        {
                            for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                            {
                                if (chest.item[inventoryIndex].type == ModContent.ItemType<DesertMedallion>())
                                {
                                    ModLoader.TryGetMod("CalValEX", out Mod CalVal);
                                    if (CalVal != null)
                                    {
                                        chest.item[inventoryIndex].type = CalVal.Find<ModItem>("DesertMedallion").Type;
                                    }
                                    else
                                    {
                                        chest.item[inventoryIndex].TurnToAir();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

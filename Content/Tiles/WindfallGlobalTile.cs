using Windfall.Common.Systems;
using Windfall.Common.Utilities;
using Windfall.Content.NPCs.WanderingNPCs;
using Windfall.Content.NPCs.WorldEvents.DragonCult;

namespace Windfall.Content.Tiles
{
    public class WindfallGlobalTile : GlobalTile
    {
        public override void RightClick(int i, int j, int type)
        {
            Tile myTile = Main.tile[new Point(i, j)];
            Chest chest = null;
            //detects if what was right clicked was a chest
            if (myTile.TileType == TileID.Containers)
            {
                //checks all chests and sees if any of their coordinates line up with the tile that was right clicked
                for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
                {
                    chest = Main.chest[chestIndex];

                    if (chest == null)
                        break;

                    List<int> chestXCoords = new() { chest.x, chest.x + 1 };
                    List<int> chestYCoords = new() { chest.y, chest.y + 1 };

                    if (chestXCoords.Contains(i) && chestYCoords.Contains(j))
                    {
                        //Main.NewText($"My Chest Found!", Color.Yellow);
                        break;
                    }
                    else
                    {
                        chest = null;
                    }
                }
            }
            //if what was right clicked is a chest, we use that chest
            if (chest != null)
            {
                //chest the opened chest to see if it is the Mechanic's Cabin Chest, and if true, begins the Lunar Cult World Event
                if (WindfallUtils.ChestContains(chest, ItemID.Toolbox) != -1)
                {
                    if (!WorldSaveSystem.MechanicCultistsEncountered)
                        cultistJumpscare(chest.x, chest.y);
                }
            }
        }
        internal static void cultistJumpscare(int x, int y)
        {
            WorldSaveSystem.MechanicCultistsEncountered = true;
            //Main.NewText($"Cultist Jumpscare!", Color.Yellow);
            Vector2 Cultist1Coords = new Vector2(x - 14, y - 4).ToWorldCoordinates();
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Cultist1Coords, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                d.noGravity = true;
            }
            NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), Cultist1Coords, ModContent.NPCType<DragonArcher>(), 0, 1);

            Vector2 Cultist2Coords = new Vector2(x + 4, y - 3).ToWorldCoordinates();
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Cultist2Coords, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                d.noGravity = true;
            }
            NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), Cultist2Coords, ModContent.NPCType<DragonArcher>(), 0, 1);

        }
    }
}

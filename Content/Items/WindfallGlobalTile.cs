using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;

namespace Windfall.Content.Items
{
    public class WindfallGlobalTile : GlobalTile
    {
        public override void RightClick(int i, int j, int type)
        {
            Tile myTile = Main.tile[new Point(i, j)];
            Chest chest = null;
            if (myTile.TileType == TileID.Containers)
            {
                //Main.NewText($"Chest Opened!", Color.Yellow);
                for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
                {
                    chest = Main.chest[chestIndex];
                    if (chest == null || (chest.x == i && chest.y == j))
                    {
                        break;
                    }
                    else
                    {
                        chest = null;
                    }
                }
            }
            if (chest != null)
            {
                if (Utilities.ChestContains(chest, ItemID.Toolbox) != -1)
                {
                    if(!WorldSaveSystem.MechanicCultistsEncountered)
                        cultistJumpscare();
                }
            }
        }
        internal static void cultistJumpscare()
        {
            WorldSaveSystem.MechanicCultistsEncountered = true;
            Main.NewText($"Cultist Jumpscare!", Color.Yellow);
        }
    }
}

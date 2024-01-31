using CalamityMod.Projectiles.Ranged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Accessories;
using Windfall.NPCs.Enemies;

namespace Windfall.Systems
{
    public class ItemNPCContactSystem : ModSystem
    {
        int targetNPC = -1;
        public override void PostUpdateNPCs()
        {
            if(isNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ItemID.DirtBlock, true))
            {
                if(Main.npc[targetNPC].life <= Main.npc[targetNPC].lifeMax / 4)
                {
                    Main.npc[targetNPC].ai[0] = 5f;
                    Main.npc[targetNPC].ai[1] = 0f;
                }
            }
        }
        internal bool isNPCTouchingItem(int npcType, int itemType, bool killItem)
        {
            targetNPC = -1;
            foreach (NPC npc in Main.npc.Where(n => (n.type == npcType && n.active)))
            {
                targetNPC = npc.whoAmI;
                break;
            }
            if (targetNPC > -1)
            {
                foreach (Item item in Main.item.Where(n => n.active && n.type == itemType))
                {
                    if (Main.npc[targetNPC].Hitbox.Intersects(item.Hitbox))
                    {
                        if(killItem)
                            item.active = false;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

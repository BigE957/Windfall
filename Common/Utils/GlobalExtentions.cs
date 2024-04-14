using Windfall.Common.Players;
using Windfall.Content.Items;
using Windfall.Content.NPCs.GlobalNPCs;
using Windfall.Content.Projectiles;

namespace Windfall.Common.Utils
{
    public static partial class WindfallUtils
    {
        public static WindfallPlayer Windfall(this Player player) => player.GetModPlayer<WindfallPlayer>();
        public static WindfallGlobalNPC Windfall(this NPC npc) => npc.GetGlobalNPC<WindfallGlobalNPC>();
        public static WindfallGlobalItem Windfall(this Item item) => item.GetGlobalItem<WindfallGlobalItem>();
        public static WindfallGlobalProjectile Windfall(this Projectile proj) => proj.GetGlobalProjectile<WindfallGlobalProjectile>();
    }
}

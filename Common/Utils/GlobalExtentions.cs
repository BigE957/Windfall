using Windfall.Common.Players;
using Windfall.Content.Items.GlobalItems;
using Windfall.Content.Items.Vanity.DevVanities;
using Windfall.Content.NPCs.GlobalNPCs;
using Windfall.Content.Projectiles;

namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    public static BuffPlayer Buff(this Player player) => player.GetModPlayer<BuffPlayer>();
    public static GodlyPlayer Godly(this Player player) => player.GetModPlayer<GodlyPlayer>();
    public static LunarCultPlayer LunarCult(this Player player) => player.GetModPlayer<LunarCultPlayer>();
    public static MonolithPlayer Monolith(this Player player) => player.GetModPlayer<MonolithPlayer>();
    public static DevVanityPlayer DevVanity(this Player player) => player.GetModPlayer<DevVanityPlayer>();
    public static HeadAnimationPlayer HeadAnimationPlayer(this Player player) => player.GetModPlayer<HeadAnimationPlayer>();
    
    public static WindfallGlobalNPC Windfall(this NPC npc) => npc.GetGlobalNPC<WindfallGlobalNPC>();
    public static DebuffGlobalNPC Debuff(this NPC npc) => npc.GetGlobalNPC<DebuffGlobalNPC>();

    public static WindfallGlobalItem Windfall(this Item item) => item.GetGlobalItem<WindfallGlobalItem>();
    public static LunarCultGlobalItem LunarCult(this Item item) => item.GetGlobalItem<LunarCultGlobalItem>();
    
    public static WindfallGlobalProjectile Windfall(this Projectile proj) => proj.GetGlobalProjectile<WindfallGlobalProjectile>();
}

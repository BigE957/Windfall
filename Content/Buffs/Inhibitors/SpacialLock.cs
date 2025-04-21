using CalamityMod;

namespace Windfall.Content.Buffs.Inhibitors;
public class SpacialLock : ModBuff
{
    public override string Texture => "CalamityMod/Buffs/StatDebuffs/MarkedforDeath";
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;

        CalamityLists.debuffList.Add(Type);
    }
    public override void Update(Player player, ref int buffIndex)
    {
        player.Buff().SpacialLock = true;
        player.chaosState = true;
    }
}

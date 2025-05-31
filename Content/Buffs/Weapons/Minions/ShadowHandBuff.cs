using Windfall.Common.Players;
using Windfall.Content.Items.Weapons.Summon;

namespace Windfall.Content.Buffs.Weapons.Minions;

public class ShadowHandBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;
        //Main.persistentBuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {           
        BuffPlayer buffPlayer = player.Buff();
        if (player.ownedProjectileCounts[ModContent.ProjectileType<OratorHandMinion>()] > 0)
            buffPlayer.OratorMinions = true;

        if (!buffPlayer.OratorMinions)
        {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
        else
            player.buffTime[buffIndex] = 18000;
    }
}

using CalamityMod.CalPlayer;
using CalamityMod.Projectiles.Summon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Players;
using Windfall.Content.Projectiles.Weapons.Summon;

namespace Windfall.Content.Buffs.Weapons.Minions
{
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
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ShadowHand_Minion>()] > 0)
            {
                buffPlayer.DeepSeeker = true;
            }
            if (!buffPlayer.DeepSeeker)
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}

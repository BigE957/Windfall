using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Buffs.DoT
{
    public class Entropy : ModBuff
    {
        public override string Texture => "CalamityMod/Buffs/StatBuffs/EmpyreanWrath";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Buff().Entropy = true;
        }
    }
}

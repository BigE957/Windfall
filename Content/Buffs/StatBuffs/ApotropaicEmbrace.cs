using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Buffs.StatBuffs;
public class ApotropaicEmbrace : ModBuff
{
    public override string Texture => "CalamityMod/Buffs/Pets/MiniMindBuff";
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Buff().ApotropaicEmbrace = true;
    }
}

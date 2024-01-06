using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using WindfallAttempt1.Utilities;

namespace WindfallAttempt1.NPCs.NPCOverrides
{
    public class CnidrionOverride : Cnidrion
    {
        public override void OnKill()
        {
            base.OnKill();
            DownedNPCSystem.downedCnidrion = true;
        }
    }
}

using CalamityMod.NPCs.Leviathan;
using WindfallAttempt1.Utilities;

namespace WindfallAttempt1.NPCs.NPCOverrides
{
    public class SirenLureOverride : LeviathanStart
    {
        public override void OnKill()
        {
            base.OnKill();
            DownedNPCSystem.downedSirenLure = true;
        }
    }
}

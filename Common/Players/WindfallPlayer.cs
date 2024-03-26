namespace Windfall.Common.Players
{
    public class WindfallPlayer : ModPlayer
    {
        public bool PerfectFlow = false;

        public override void ResetEffects()
        {
            PerfectFlow = false;
        }
        public override void UpdateDead()
        {
            PerfectFlow = false;
        }
    }
}
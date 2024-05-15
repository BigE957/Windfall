namespace Windfall.Common.Players
{
    public class BuffPlayer : ModPlayer
    {
        public bool PerfectFlow = false;
        public bool DeepSeeker = false;

        public override void ResetEffects()
        {
            PerfectFlow = false;
            DeepSeeker = false;
        }
        public override void UpdateDead()
        {
            PerfectFlow = false;
            DeepSeeker = false;
        }
    }
}
﻿namespace Windfall.Common.Players
{
    public class BuffPlayer : ModPlayer
    {
        public bool PerfectFlow = false;
        public bool DeepSeeker = false;
        public bool WretchedHarvest = false;

        public override void ResetEffects()
        {
            PerfectFlow = false;
            DeepSeeker = false;
            WretchedHarvest = false;
        }
        public override void UpdateDead()
        {
            PerfectFlow = false;
            DeepSeeker = false;
            WretchedHarvest = false;
        }
        public override void PostUpdateMiscEffects()
        {
            if (WretchedHarvest)
                Player.statDefense += 10;
        }
        public override void UpdateLifeRegen()
        {
            if (WretchedHarvest)
                Player.lifeRegen += 2;
        }
    }
}
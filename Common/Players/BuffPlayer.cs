namespace Windfall.Common.Players;

public class BuffPlayer : ModPlayer
{
    public bool PerfectFlow = false;
    public bool DeepSeeker = false;
    public bool WretchedHarvest = false;
    public bool Entropy = false;
    public bool Wildfire = false;

    public override void ResetEffects()
    {
        PerfectFlow = false;
        DeepSeeker = false;
        WretchedHarvest = false;
        Entropy = false;
        Wildfire = false;
    }
    public override void UpdateDead()
    {
        PerfectFlow = false;
        DeepSeeker = false;
        WretchedHarvest = false;
        Entropy = false;
        Wildfire = false;
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
    public override void UpdateBadLifeRegen()
    {
        if(Entropy || Wildfire)
        {
            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
        }

        if(Entropy)
            Player.lifeRegen -= 150;
        if(Wildfire)
            Player.lifeRegen -= 50;
    }
}
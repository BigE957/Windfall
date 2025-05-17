using Terraria.Graphics.Effects;

namespace Windfall.Common.Players;

public class MonolithPlayer : ModPlayer
{
    public byte OratorMonolith = 0;
    public override void PostUpdate()
    {
        if (SkyManager.Instance["Windfall:Orator"] != null && (OratorMonolith > 0) != SkyManager.Instance["Windfall:Orator"].IsActive())
        {
            if (OratorMonolith > 0)
                SkyManager.Instance.Activate("Windfall:Orator");
            else
                SkyManager.Instance.Deactivate("Windfall:Orator", []);
        }
        //OratorMonolith = false;
    }
    public override void ResetEffects()
    {
        if(OratorMonolith > 0)
            OratorMonolith--;
        if (OratorMonolith < 0)
            OratorMonolith = 0;
        if (OratorMonolith > 30)
            OratorMonolith = 30;
    }
}

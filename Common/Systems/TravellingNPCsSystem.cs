using Terraria.ModLoader.IO;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Systems;

public class TravellingNPCsSystem : ModSystem
{
    public override void PreUpdateWorld()
    {
        TravellingCultist.UpdateTravelingMerchant();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (TravellingCultist.spawnTime != double.MaxValue)
        {
            tag["spawnTime"] = TravellingCultist.spawnTime;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (!tag.TryGet("spawnTime", out TravellingCultist.spawnTime))
        {
            TravellingCultist.spawnTime = double.MaxValue;
        }
    }

    public override void ClearWorld()
    {
        TravellingCultist.spawnTime = double.MaxValue;
    }
}
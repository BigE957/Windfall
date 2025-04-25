using Terraria.ModLoader.IO;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Common.Systems;

public class WorldSaveSystem : ModSystem
{
    public static List<bool> JournalsCollected = new(13);
    public static bool CloneRevealed = false;
    public static bool ScoogFished = false;
    public static bool MechanicCultistsEncountered = false;
    public static bool SelenicChestOpened = false;

    public static List<string> CreditDataNames;
    public static List<int> CreditDataCredits;
    public override void ClearWorld()
    {
        ResetWorldData();
    }
    public override void LoadWorldData(TagCompound tag)
    {
        CloneRevealed = tag.GetBool("CloneRevealed");
        ScoogFished = tag.GetBool("ScoogFished");
        MechanicCultistsEncountered = tag.GetBool("MechanicCultistsEncountered");
        SelenicChestOpened = tag.GetBool("SelenicChestOpened");

        JournalsCollected = (List<bool>)tag.GetList<bool>("JournalsCollected");

        CreditDataNames = (List<string>)tag.GetList<string>("CreditDataNames");
        CreditDataCredits = (List<int>)tag.GetList<int>("CreditDataCredits");
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["CloneRevealed"] = CloneRevealed;
        tag["ScoogFished"] = ScoogFished;
        tag["MechanicCultistsEncountered"] = MechanicCultistsEncountered;
        tag["SelenicChestOpened"] = SelenicChestOpened;

        tag["JournalsCollected"] = JournalsCollected;

        tag["CreditDataNames"] = CreditDataNames;
        tag["CreditDataCredits"] = CreditDataCredits;
    }
    public static void ResetWorldData()
    {
        CloneRevealed = false;
        ScoogFished = false;
        MechanicCultistsEncountered = false;
        SelenicChestOpened = false;

        for (int i = 0; i < JournalsCollected.Count; i++)
        {
            JournalsCollected[i] = false;
        }

        CreditDataNames = [];
        CreditDataCredits = [];

        SealingRitualSystem.RitualSequenceSeen = false;
        DownedNPCSystem.downedOrator = false;
    }
}

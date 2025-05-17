using Terraria.ModLoader.IO;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Systems;

public class WorldSaveSystem : ModSystem
{
    public static List<bool> JournalsCollected = new(13);
    public static bool CloneRevealed = false;
    public static bool ScoogFished = false;
    public static bool MechanicCultistsEncountered = false;
    public static bool SelenicChestOpened = false;

    //public static List<string> CreditDataNames;
    //public static List<int> CreditDataCredits;

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

        TravellingCultist.CurrentDialogue = (TravellingCultist.DialogueState)tag.GetInt("DialogueState");

        if (tag.ContainsKey("CirculatingDialogue"))
        {
            TravellingCultist.pool.CirculatingDialogues.Clear();
            List<string> circulatingTrees = (List<string>)tag.GetList<string>("CirculatingDialogue");
            for (int i = 0; i < TravellingCultist.pool.CirculatingDialogues.Count; i++)
            {
                var (TreeKey, Requirement, Priority, Repeatable) = TravellingCultist.pool.Dialogues.First(d => d.TreeKey == circulatingTrees[i]);
                TravellingCultist.pool.CirculatingDialogues.Add(new(TreeKey, Requirement, Priority));
            }
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["CloneRevealed"] = CloneRevealed;
        tag["ScoogFished"] = ScoogFished;
        tag["MechanicCultistsEncountered"] = MechanicCultistsEncountered;
        tag["SelenicChestOpened"] = SelenicChestOpened;

        tag["JournalsCollected"] = JournalsCollected;

        tag["DialogueState"] = (int)TravellingCultist.CurrentDialogue;

        if (TravellingCultist.pool.CirculatingDialogues.Count != TravellingCultist.pool.Dialogues.Count)
        {
            List<string> circulatingTrees = [];
            for (int i = 0; i < TravellingCultist.pool.CirculatingDialogues.Count; i++)
            {
                circulatingTrees.Add(TravellingCultist.pool.CirculatingDialogues[i].TreeKey);
            }
            tag["CirculatingDialogue"] = circulatingTrees;
        }
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

        TravellingCultist.CurrentDialogue = TravellingCultist.DialogueState.SearchForHelp;
        TravellingCultist.pool.ResetCirculatingDialogues();

        SealingRitualSystem.RitualSequenceSeen = false;
        DownedNPCSystem.downedOrator = false;
    }
}

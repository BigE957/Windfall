using DialogueHelper.UI.Dialogue;
using Stubble.Core.Imported;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static void SetupFetchQuestTree(ref DialogueUISystem uiSystem, string path, string character, Tuple<int, int> itemStack, string turnInResponse = "Here you go!", string leaveResponse = "Can do!", string completeResponse = "You're welcome!")
    {
        Item item = new Item(itemStack.Item1, itemStack.Item2);
        ItemStack cost = new()
        {
            ItemName = item.Name,
            ItemID = item.type,
            Stack = item.stack,
            SourceMod = item.ModItem != null ? item.ModItem.Mod.Name : "",
        };

        string name = new(cost.ItemName.ToCharArray().Where(c => !c.IsWhitespace()).ToArray());
        name += ".";

        uiSystem.CurrentTree.Dialogues[0].DialogueText[0].Text = GetWindfallTextValue("Dialogue." + path + name + "Start");
        uiSystem.CurrentTree.Dialogues[0].Responses[0].Title = turnInResponse;
        uiSystem.CurrentTree.Dialogues[0].Responses[0].Cost = cost;
        uiSystem.CurrentTree.Dialogues[0].Responses[1].Title = leaveResponse;

        uiSystem.CurrentTree.Dialogues[1].DialogueText[0].Text = GetWindfallTextValue("Dialogue." + path + name + "During");
        uiSystem.CurrentTree.Dialogues[1].Responses[0].Title = turnInResponse;
        uiSystem.CurrentTree.Dialogues[1].Responses[0].Cost = cost;
        uiSystem.CurrentTree.Dialogues[1].Responses[1].Title = leaveResponse;

        uiSystem.CurrentTree.Dialogues[2].DialogueText[0].Text = GetWindfallTextValue("Dialogue." + path + name + "End");
        uiSystem.CurrentTree.Dialogues[2].Responses[0].Title = completeResponse;

        uiSystem.CurrentTree.Characters = [character];
    }
}

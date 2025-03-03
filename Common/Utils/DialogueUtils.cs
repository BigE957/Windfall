using DialogueHelper.UI.Dialogue;
using Stubble.Core.Imported;
using Terraria.Utilities;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static void SetupFetchQuestTree(ref DialogueUISystem uiSystem, string path, string character, Tuple<int, int> itemStack, string turnInResponse = "Here you go!", string leaveResponse = "Can do!", string completeResponse = "You're welcome!")
    {
        Item item = new(itemStack.Item1, itemStack.Item2);
        ItemStack cost = new(itemStack);

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
    public static void AttachCostToResponse(ref DialogueUISystem uiSystem, Tuple<int, int> itemStack, int dialogueIndex, int responseIndex)
    {
        ItemStack cost = new(itemStack);    
        uiSystem.CurrentTree.Dialogues[dialogueIndex].Responses[responseIndex].Cost = cost;
    }

    public class DialoguePool
    {
        public readonly List<(string TreeKey, Func<Player, bool> Requirement, byte Priority, bool Repeatable)> Dialogues;
        public List<(string TreeKey, Func<Player, bool> Requirement, byte Priority)> CirculatingDialogues = [];

        public DialoguePool(List<(string TreeKey, Func<Player, bool> Requirement, byte Priority, bool Repeatable)> dialogues)
        {
            Dialogues = dialogues;
            foreach (var (TreeKey, Requirement, Priority, Repeatable) in Dialogues)
            {
                (string TreeKey, Func<Player, bool> Requirement, byte Priority) myDialogue = new(TreeKey, Requirement, Priority);
                CirculatingDialogues.Add(myDialogue);
            }
        }

        public string GetTree(Player player)
        {
            WeightedRandom<string> keys = new();
            short topPriority = -1;
            foreach (var (TreeKey, Requirement, Priority) in CirculatingDialogues)
            {
                if(Requirement.Invoke(player) && Priority > topPriority)
                    topPriority = Priority;
            }

            if (topPriority == -1)
            {
                foreach (var (TreeKey, Requirement, Priority, Repeatable) in Dialogues)
                {
                    if(Repeatable)
                        CirculatingDialogues.Add(new(TreeKey, Requirement, Priority));
                }

                foreach (var (TreeKey, Requirement, Priority) in CirculatingDialogues)
                {
                    if (Requirement.Invoke(player) && Priority > topPriority)
                        topPriority = Priority;
                }
            }

            foreach (var (TreeKey, Requirement, Priority) in CirculatingDialogues)
            {
                if (Requirement.Invoke(player) && Priority >= topPriority)
                    keys.Add(TreeKey);
            }
            string key = keys.Get();
            CirculatingDialogues.Remove(CirculatingDialogues.First(d => d.TreeKey == key));

            if(CirculatingDialogues.Count == 0)
            {
                foreach(var (TreeKey, Requirement, Priority, Repeatable) in Dialogues)
                {
                    CirculatingDialogues.Add(new(TreeKey, Requirement, Priority));
                }
            }

            return key;
        }
    }
}

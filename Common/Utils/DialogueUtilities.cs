﻿using Windfall.Common.Systems;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WanderingNPCs;

namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    private static readonly List<string> PaladinQuests =
    [
        "CnidrionHunt",
        "ScoogHunt",
        "ShuckinClams",
        "ClamHunt",
        "ScoogHunt2",
    ];
    private static readonly List<string> GodseekerQuests =
    [
        "PestControl",
        "Decontamination",
    ];
    public struct dialogueButton
    {
        public string name = "";
        public int heading = -1;
        public bool end = false;

        public dialogueButton()
        { }
    }
    public struct dialogueDirections
    {
        public int MyPos;
        public dialogueButton Button1;
        public dialogueButton? Button2;
    }
    /// <param name="npc">The NPC who is giving dialouge currently.</param>
    /// <returns>
    /// Handles NPC dialogue for NPCs with Progressive Quests (quests that build off each other). Interacts with <see cref="QuestSystem"/> heavilly.
    /// </returns>
    public static void ProgressiveQuestDialogueHelper(NPC npc)
    {
        int index = -1;
        string npcName = npc.TypeName.Replace(" ", "");
        List<string> MyQuests = null;

        if (npcName == "IlmeranPaladin" || npcName == "IlmeranPaladinKnocked")
        {
            npcName = "IlmeranPaladin";
            MyQuests = PaladinQuests;
        }
        else if (npcName == "GodseekerKnight")
            MyQuests = GodseekerQuests;

        bool success = false;
        if (MyQuests != null)
        {
            for (int i = 0; i < MyQuests.Count; i++)
            {
                index = QuestSystem.QuestLog.FindIndex(quest => quest.Name == MyQuests[i]);
                if (index != -1)
                    if ((!QuestSystem.QuestLog[index].Completed || QuestSystem.QuestLog[index].Active) && QuestSystem.QuestLog[index].Unlocked)
                    {
                        success = true;
                        break;
                    }
            }
            if (!success)
                index = -1;
        }
        if (index != -1)
        {
            if (!QuestSystem.QuestLog[index].Completed)
            {
                if (!QuestSystem.QuestLog[index].Active)
                {
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.{QuestSystem.QuestLog[index].Name}.Start");

                    if (QuestSystem.QuestLog[index].QuestGifts != null)
                    {
                        var entitySource = npc.GetSource_GiftOrReward();
                        for (int i = 0; i < QuestSystem.QuestLog[index].QuestGifts.Count; i++)
                            Main.LocalPlayer.QuickSpawnItem(entitySource, QuestSystem.QuestLog[index].QuestGifts[i].Type, QuestSystem.QuestLog[index].QuestGifts[i].Stack);
                    }
                    QuestSystem.ToggleQuestActive(index);
                }
                else
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.{QuestSystem.QuestLog[index].Name}.During");
                Main.npcChatCornerItem = QuestSystem.QuestLog[index].QuestGifts[0].Type;
            }
            else
            {
                if (QuestSystem.QuestLog[index].Active)
                {
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.{QuestSystem.QuestLog[index].Name}.End");

                    var entitySource = npc.GetSource_GiftOrReward();
                    for (int i = 0; i < QuestSystem.QuestLog[index].QuestRewards.Count; i++)
                        Main.LocalPlayer.QuickSpawnItem(entitySource, QuestSystem.QuestLog[index].QuestRewards[i].Type, QuestSystem.QuestLog[index].QuestRewards[i].Stack);

                    QuestSystem.ToggleQuestActive(index);
                }
                else
                {
                    //Technically, this point should never be reached, as the conditions required are what we check for in the above
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.NoQuest");
                    Main.NewText("An index is being used despite being both Inactive and Completed. Please report this and send your log.txt.", Color.Yellow);
                }
            }
        }
        else
        {
            if (npcName == "GodseekerKnight" && !(DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator))
                Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.LilBitch");
            else
                Main.npcChatText = GetWindfallTextValue($"Dialogue.{npcName}.Quests.NoQuest");
        }
    }

    /// <param name="npc">The NPC who is giving dialouge currently.</param>
    /// <returns>
    /// Handles NPC dialogue for NPCs with Randomized Quests (quests that are randomly selected from a pool). Interacts with <see cref="QuestSystem"/> heavilly.
    /// </returns>
    public static void RandomizedQuestDialougeHelper(NPC npc)
    {
        string Path = null;
        List<string> MyQuests = null;
        if (npc.type == ModContent.NPCType<GodseekerKnight>())
        {
            Path = "GodseekerKnight.Quests";
            MyQuests = GodseekerQuests;
        }              
        if (MyQuests != null)
        {
            int index = -1;
            foreach (string name in MyQuests)
            {
                index = QuestSystem.QuestLog.FindIndex(quest => quest.Name == name);
                if (index != -1 && QuestSystem.QuestLog[index].Active)
                    break;
                else
                    index = -1;
            }
            if (index == -1)
            {
                index = Main.rand.Next(PaladinQuests.Count, MyQuests.Count + PaladinQuests.Count);
            }

            if (!QuestSystem.QuestLog[index].Active)
            {
                QuestSystem.ToggleQuestActive(index);
                Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{QuestSystem.QuestLog[index].Name}.Start");

            }
            else
            {
                Player player = Main.player[Main.myPlayer];
                if (QuestSystem.QuestLog[index].Completed)
                {
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{QuestSystem.QuestLog[index].Name}.End");
                    for (int i = 0; i < QuestSystem.QuestLog[index].QuestRewards.Count; i++)
                        Main.LocalPlayer.QuickSpawnItem(Entity.GetSource_None(), QuestSystem.QuestLog[index].QuestRewards[i].Type, QuestSystem.QuestLog[index].QuestRewards[i].Stack); QuestSystem.ToggleQuestActive(index);
                    
                    DisplayLocalizedText($"{QuestSystem.QuestLog[index].Completed}");
                    QuestSystem.ResetQuestProgress(index);
                    if (QuestSystem.QuestLog[index].Active)
                        QuestSystem.ToggleQuestActive(index);
                    //index = -1;
                }
                else
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{QuestSystem.QuestLog[index].Name}.During");
            }
        }
        else
            Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.NoQuest");
    }

    /// <param name="npc">The NPC who is giving dialouge currently.</param>
    /// <param name="QuestComplete">A bool determining whether this quest has been completed.</param>
    /// <param name="CurrentQuestItem">The current Quest Item this npc is looking for.</param>
    /// <param name="MyQuestItems">The list of Quest Item this npc can be looking for.</param>
    /// <returns>
    /// Handles NPC dialogue for NPCs with Randomized Quests (quests that are randomly selected from a pool). Does not interact with <see cref="QuestSystem"/>.
    /// </returns>
    public static QuestItem CollectorQuestDialogueHelper(NPC npc, ref bool QuestComplete, QuestItem CurrentQuestItem, List<QuestItem> MyQuestItems, int index = -1)
    {
        string Path = null;
        if (npc.type == ModContent.NPCType<TravellingCultist>())
        {
            if (npc.ModNPC is TravellingCultist cultist && WorldSaveSystem.cultistChatState == 5)
                Path = "LunarCult.TravellingCultist.Quests.Ritual";
            else
                Path = "LunarCult.TravellingCultist.Quests.Dungeon";
        }
        if (!QuestComplete)
        {
            bool questActive = true;
            if (CurrentQuestItem.Stack == 0)
            {
                if(index == -1)
                    index = Main.rand.Next(0, MyQuestItems.Count);
                CurrentQuestItem = MyQuestItems[index];
                questActive = false;
            }
            else
                index = MyQuestItems.IndexOf(CurrentQuestItem);
            string ItemName = ContentSamples.ItemsByType[CurrentQuestItem.Type].ModItem.Name;

            if (!questActive)
            {
                questActive = true;
                Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{ItemName}.Start");

            }
            else
            {
                Player player = Main.player[Main.myPlayer];
                foreach (Item item in player.inventory.Where(n => n.type == CurrentQuestItem.Type))
                {
                    if (item.stack >= CurrentQuestItem.Stack)
                    {
                        item.stack -= CurrentQuestItem.Stack;
                        QuestComplete = true;
                        break;
                    }
                    else
                    {
                        Main.npcChatText = "I'll need more than this... About " + CurrentQuestItem.Stack + " should be enough.";
                        return CurrentQuestItem;
                    }

                }
                if (QuestComplete)
                {
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{ItemName}.End");
                    Item.NewItem(npc.GetSource_GiftOrReward(), player.Center, Vector2.Zero, ItemID.DungeonFishingCrateHard);
                    CurrentQuestItem = new(0, 0);
                    questActive = false;
                    index = -1;
                }
                else
                    Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.{ItemName}.During");
            }
            if (questActive)
                Main.npcChatCornerItem = CurrentQuestItem.Type;
        }
        else
            Main.npcChatText = GetWindfallTextValue($"Dialogue.{Path}.NoQuest");
        return CurrentQuestItem;
    }

    /// <param name="MyDialogue">Instructions for this function ro read from.</param>
    /// <param name="CurrentDialogue">Dialogue state of this NPC (usually tied to an <see cref="enum"/>).</param>
    /// <param name="button">The button variable taken directly from <see cref="ModNPC.SetChatButtons(ref string, ref string)"/>.</param>
    /// <param name="button2">The button2 variable taken directly from <see cref="ModNPC.SetChatButtons(ref string, ref string)"/>.</param>
    /// <returns>
    /// Sets the Chat Buttons for an NPC when it is going through Conversation Dialogue.
    /// </returns>
    public static void SetConversationButtons(List<dialogueDirections> MyDialogue, int CurrentDialogue, ref string button, ref string button2)
    {
        dialogueDirections myDirections = MyDialogue.Find(n => n.MyPos == CurrentDialogue);
        button = myDirections.Button1.name;
        if (myDirections.Button2 != null)
            button2 = myDirections.Button2.Value.name;
    }

    /// <param name="MyDialogue">Instructions for this function ro read from.</param>
    /// <param name="CurrentDialogue">Dialogue state of this NPC (usually tied to an <see cref="enum"/>).</param>
    /// <param name="firstButton">The button variable taken directly from <see cref="ModNPC.OnChatButtonClicked(bool, ref string)"/>.</param>
    /// <returns>
    /// Handles setting the CurrentDialogue variable to its proper value, as well as closing the NPC Chat Box when the Conversation ends.
    /// </returns>
    public static int GetNPCConversation(List<dialogueDirections> MyDialogue, int CurrentDialogue, bool firstButton)
    {
        dialogueDirections myDirections = MyDialogue.Find(n => n.MyPos == CurrentDialogue);
        if (firstButton)
        {
            if (myDirections.Button1.end)
                Main.CloseNPCChatOrSign();
            return myDirections.Button1.heading;
        }
        else if (myDirections.Button2 != null)
        {
            if (myDirections.Button2.Value.end)
                Main.CloseNPCChatOrSign();
            return myDirections.Button2.Value.heading;
        }
        return CurrentDialogue;
    }
}

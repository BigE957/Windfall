using System.Collections.Generic;
using Terraria.Localization;
using Terraria;
using Microsoft.Xna.Framework;
using Windfall.Content.Systems;

namespace Windfall.Utilities
{
    public static partial class Utilities
    {
        private static readonly List<string> PaladinQuests = new()
        {
            "CnidrionHunt",
            "ScoogHunt",
        };
        public static void QuestDialogueHelper(NPC npc)
        {
            int index = -1;
            string npcName = npc.TypeName.Replace(" ", "");
            if (npcName == "IlmeranPaladin" || npcName == "IlmeranPaladinKnocked")
            {
                npcName = "IlmeranPaladin";
                bool success = false;
                for (int i = 0; i < PaladinQuests.Count; i++) 
                {
                    index = QuestSystem.QuestLog.FindIndex(quest => quest.Name == PaladinQuests[i]);
                    if(index != -1)
                    {
                        if (!QuestSystem.QuestLog[index].Completed || QuestSystem.QuestLog[index].Active)
                        {
                            success = true;
                            break;
                        }
                    }
                }
                if (!success) 
                {
                    index = -1;
                }
            }
            if (index != -1)
            {
                if (!QuestSystem.QuestLog[index].Completed)
                {
                    if (!QuestSystem.QuestLog[index].Active)
                    {
                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.{QuestSystem.QuestLog[index].Name}DialogueStart").Value;

                        if (QuestSystem.QuestLog[index].QuestGifts != null)
                        {
                            var entitySource = npc.GetSource_GiftOrReward();
                            Main.npcChatCornerItem = QuestSystem.QuestLog[index].QuestGifts[0].Type;
                            for (int i = 0; i < QuestSystem.QuestLog[index].QuestGifts.Count; i++)
                            {
                                Main.LocalPlayer.QuickSpawnItem(entitySource, QuestSystem.QuestLog[index].QuestGifts[i].Type, QuestSystem.QuestLog[index].QuestGifts[i].Stack);
                            }
                        }

                        QuestSystem.ToggleQuestActive(index);
                    }
                    else
                    {
                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.{QuestSystem.QuestLog[index].Name}DialogueDuring").Value;
                        Main.npcChatCornerItem = QuestSystem.QuestLog[index].QuestGifts[0].Type;
                    }
                }
                else
                {
                    if (QuestSystem.QuestLog[index].Active)
                    {
                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.{QuestSystem.QuestLog[index].Name}DialogueEnd").Value;

                        var entitySource = npc.GetSource_GiftOrReward();
                        for (int i = 0; i < QuestSystem.QuestLog[index].QuestRewards.Count; i++)
                        {
                            Main.LocalPlayer.QuickSpawnItem(entitySource, QuestSystem.QuestLog[index].QuestRewards[i].Type, QuestSystem.QuestLog[index].QuestRewards[i].Stack);
                        }

                        QuestSystem.ToggleQuestActive(index);
                    }
                    else
                    {
                        //Technically, this point should never be reached, as the conditions required are what we check for in the above
                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.NoQuestDialogue").Value;
                        Main.NewText("An index is being used despite being both Inactive and Completed.", Color.Yellow);
                    }
                }
            }
            else
            {
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.NoQuestDialogue").Value;
            }
        }
    }
}

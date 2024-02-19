using System.Collections.Generic;
using Terraria.Localization;
using Terraria;
using Microsoft.Xna.Framework;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;
using Windfall.Content.NPCs.WanderingNPCs;
using CalamityMod;
using System.Linq;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Utilities
{
    public static partial class Utilities
    {
        private static readonly List<string> PaladinQuests = new()
        {
            "CnidrionHunt",
            "ScoogHunt",
            "ShuckinClams",
            "ClamHunt",
            "ScoogHunt2",
        };
        private static readonly List<string> RoninQuests = new()
        {
            "SlimeGodHunt",
            "CrystalSearch",
            "QueenSlimeHunt",
        };
        public static void QuestDialogueHelper(NPC npc)
        {
            int index = -1;
            string npcName = npc.TypeName.Replace(" ", "");
            List<string> MyQuests = null;

            if (npcName == "IlmeranPaladin" || npcName == "IlmeranPaladinKnocked")
            {
                npcName = "IlmeranPaladin";
                MyQuests = PaladinQuests;
            }
            else if (npcName == "LoneRonin")
            {
                MyQuests = RoninQuests;
            }

            bool success = false;
            if (MyQuests != null)
            {               
                for (int i = 0; i < MyQuests.Count; i++)
                {
                    index = QuestSystem.QuestLog.FindIndex(quest => quest.Name == MyQuests[i]);
                    if (index != -1)
                    {
                        if ((!QuestSystem.QuestLog[index].Completed || QuestSystem.QuestLog[index].Active) && QuestSystem.QuestLog[index].Unlocked)
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
                if(npcName == "LoneRonin" && !(DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator))
                {
                    Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.LilBitchDialogue").Value;
                }
                else
                {
                    Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{npcName}.NoQuestDialogue").Value;

                }
            }
        }
        public static QuestItem CollectorQuestDialogueHelper(NPC npc, ref bool QuestComplete, QuestItem CurrentQuestItem)
        {
            string NPCPath = null;
            List<QuestItem> MyQuestItems = null;
            if (npc.type == ModContent.NPCType<TravellingCultist>())
            {
                NPCPath = "LunarCult.TravellingCultist";
                MyQuestItems = QuestSystem.TravellingCultistQuestItems;
            }
            int index = -1;
            if (!QuestComplete)
            {
                bool questActive = true;
                if (CurrentQuestItem.Stack == 0)
                {
                    index = Main.rand.Next(0, MyQuestItems.Count);
                    CurrentQuestItem = MyQuestItems[index];
                    questActive = false;
                }
                else
                {
                    index = MyQuestItems.IndexOf(CurrentQuestItem);
                }
                string ItemName = ContentSamples.ItemsByType[CurrentQuestItem.Type].Name.Replace(" ", "");

                if (!questActive)
                {
                    questActive = true;
                    Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{NPCPath}.{ItemName}Start").Value;
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
                        }
                        else
                        {
                            Main.npcChatText = "I'll need more than this... About " + CurrentQuestItem.Stack + " should be enough.";
                            return new(0,0);
                        }

                    }
                    if (QuestComplete)
                    {

                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{NPCPath}.{ItemName}End").Value;
                        Item.NewItem(npc.GetSource_GiftOrReward(), player.Center, Vector2.Zero, ItemID.DungeonFishingCrateHard);
                        CurrentQuestItem = new(0, 0);
                        questActive = false;
                        index = -1;
                    }
                    else
                    {
                        Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{NPCPath}.{ItemName}During").Value;
                    }
                }
            }
            else
            {
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{NPCPath}.NoQuest").Value;
            }
            return CurrentQuestItem;
        }
    }
}

using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Windfall.Items.Weapons.Misc;

namespace Windfall.Systems
{
    public struct Item
    {
        internal int Type;

        internal int Stack = 1;

        internal Item(int type, int stack)
        {
            Type = type;
            Stack = stack;
        }
    }
    public class QuestSystem : ModSystem
    {
        public struct Quest
        {
            public string Name;
            public bool Completed = false;
            public bool Active = false;
            public List<string> Objectives;
            public List<int> ObjectiveRequirements;
            public List<int> ObjectiveProgress;
            public List<Item> QuestGifts;
            public List<Item> QuestRewards;
            public Quest(string name, List<string> objective, bool completed, List<int> objReq, List<int> objProg, bool active, List<Item> questGifts, List<Item> questRewards)
            {
                Name = name;
                Completed = completed;
                Active = active;
                Objectives = objective;
                ObjectiveRequirements = objReq;
                ObjectiveProgress = objProg;
                QuestGifts = questGifts;
                QuestRewards = questRewards;
            }
        }
        public static List<Quest> QuestLog = InitializeQuestLog();
        public static List<int> Nums;

        public override void ClearWorld()
        {
            QuestLog = InitializeQuestLog();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            QuestLog = (List<Quest>)tag.GetList<Quest>("QuestLog");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["QuestLog"] = QuestLog;
        }
        internal static Quest CreateQuest(string Name, List<string>Objectives, List<int>ObjectiveRequirements, List<Item>QuestGifts = null, List<Item>QuestRewards = null)
        {
            List<int> objectiveProgress = new();
            for(int i = 0; i < ObjectiveRequirements.Count; i++ )
            {
                objectiveProgress.Add(0);
            }
            return new Quest { Name = Name, Completed = false, Objectives = Objectives, ObjectiveProgress = objectiveProgress, ObjectiveRequirements = ObjectiveRequirements, Active = false, QuestGifts = QuestGifts, QuestRewards = QuestRewards};
        }
        internal static List<Quest> InitializeQuestLog()
        {
            List<Quest> list = new()
            {
                CreateQuest("CnidrionHunt", new List<string>{"Pacify 5 Cnidrions"}, new List<int>{5}, new List<Item>{ new Item { Type = ModContent.ItemType<Cnidrisnack>(), Stack = 5 } }, new List<Item>{ new Item { Type = ModContent.ItemType<CnidrionBanner>(), Stack = 4 }, new Item {Type = ModContent.ItemType<AmidiasSpark>() } })
            };
            return list;
        }
        public static void IncrementQuestProgress(int questIndex, int questReqIndex)
        {
            if (QuestLog[questIndex].ObjectiveProgress[questReqIndex] < QuestLog[questIndex].ObjectiveRequirements[questReqIndex])
            {
                Quest temp = QuestLog[questIndex];
                temp.ObjectiveProgress[questReqIndex]++;
                if(temp.ObjectiveProgress[questReqIndex] >= temp.ObjectiveRequirements[questReqIndex])
                    temp.Completed = true;
                QuestLog[questIndex] = temp;
            }else if (!QuestLog[questIndex].Completed)
            {
                Quest temp = QuestLog[questIndex];
                temp.Completed = true;
                QuestLog[questIndex] = temp;
            }
        }
        public static void ToggleQuestActive(int questIndex)
        {
            Quest temp = QuestLog[questIndex];
            if (temp.Active)
                temp.Active = false;
            else
                temp.Active = true;
            QuestLog[questIndex] = temp;
        }
        public class QuestSerializer : TagSerializer<Quest, TagCompound>
        {
            public override TagCompound Serialize(Quest value) => new()
            {
                ["name"] = value.Name,
                ["completion"] = value.Completed,
                ["active"] = value.Active,
                ["objectives"] = value.Objectives,
                ["objReqs"] = value.ObjectiveRequirements,
                ["objProg"] = value.ObjectiveProgress,
                ["gifts"] = value.QuestGifts,
                ["rewards"] = value.QuestRewards
            };

            public override Quest Deserialize(TagCompound tag) => new(tag.GetString("name"), (List<string>)tag.GetList<string>("objectives"), tag.GetBool("completion"), (List<int>)tag.GetList<int>("objReqs"), (List<int>)tag.GetList<int>("objProg"), tag.GetBool("active"), (List<Item>)tag.GetList<Item>("gifts"), (List<Item>)tag.GetList<Item>("rewards"));
        }
        public class QuestItemSerializer : TagSerializer<Item, TagCompound>
        {
            public override TagCompound Serialize(Item value) => new()
            {
                ["type"] = value.Type,
                ["stack"] = value.Stack,

            };

            public override Item Deserialize(TagCompound tag) => new(tag.GetInt("type"), tag.GetInt("stack"));
        }
    }
}

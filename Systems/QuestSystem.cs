﻿using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Windfall.Items.Fishing;
using Windfall.Items.Weapons.Misc;
using Windfall.Projectiles.Fishing;

namespace Windfall.Systems
{
    public struct QuestItem
    {
        internal int Type;

        internal int Stack = 1;

        internal QuestItem(int type, int stack)
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
            public List<QuestItem> QuestGifts;
            public List<QuestItem> QuestRewards;
            public Quest(string name, List<string> objective, bool completed, List<int> objReq, List<int> objProg, bool active, List<QuestItem> questGifts, List<QuestItem> questRewards)
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
        public static List<Quest> QuestLog = InitializedQuestLog();
        public static List<int> Nums;

        public override void ClearWorld()
        {
            QuestLog = InitializedQuestLog();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            QuestLog = (List<Quest>)tag.GetList<Quest>("QuestLog");
            EnsureQuestLogPopulated(InitializedQuestLog());
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["QuestLog"] = QuestLog;
        }
        internal static Quest CreateQuest(string Name, List<string>Objectives, List<int>ObjectiveRequirements, List<QuestItem>QuestGifts = null, List<QuestItem>QuestRewards = null)
        {
            List<int> objectiveProgress = new();
            for(int i = 0; i < ObjectiveRequirements.Count; i++ )
            {
                objectiveProgress.Add(0);
            }
            return new Quest { Name = Name, Completed = false, Objectives = Objectives, ObjectiveProgress = objectiveProgress, ObjectiveRequirements = ObjectiveRequirements, Active = false, QuestGifts = QuestGifts, QuestRewards = QuestRewards};
        }
        internal static List<Quest> InitializedQuestLog()
        {
            List<Quest> list = new()
            {
                CreateQuest("CnidrionHunt", new List<string>{"Pacify 5 Cnidrions"}, new List<int>{5}, new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<Cnidrisnack>(), Stack = 5 } }, new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<CnidrionBanner>(), Stack = 4 }, new QuestItem {Type = ModContent.ItemType<AmidiasSpark>(), Stack = 1} }),
                CreateQuest("ScoogHunt", new List<string>{"Defeat Desert Scourge"}, new List<int>{1}, new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<AncientIlmeranRod>(), Stack = 1 }, new QuestItem { Type = ModContent.ItemType<Cnidrisnack>(), Stack = 5 } }, new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<DesertScourgeTrophy>(), Stack = 1 } })
            };
            return list;
        }
        internal static void EnsureQuestLogPopulated(List<Quest> initList)
        {
            for(int i = 0; i < initList.Count; i++)
            {
                if(i >= QuestLog.Count)
                    QuestLog.Add(initList[i]);
            }
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

            public override Quest Deserialize(TagCompound tag) => new(tag.GetString("name"), (List<string>)tag.GetList<string>("objectives"), tag.GetBool("completion"), (List<int>)tag.GetList<int>("objReqs"), (List<int>)tag.GetList<int>("objProg"), tag.GetBool("active"), (List<QuestItem>)tag.GetList<QuestItem>("gifts"), (List<QuestItem>)tag.GetList<QuestItem>("rewards"));
        }
        public class QuestItemSerializer : TagSerializer<QuestItem, TagCompound>
        {
            public override TagCompound Serialize(QuestItem value) => new()
            {
                ["type"] = value.Type,
                ["stack"] = value.Stack,

            };

            public override QuestItem Deserialize(TagCompound tag) => new(tag.GetInt("type"), tag.GetInt("stack"));
        }
    }
}

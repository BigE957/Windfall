using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.SummonItems;
using Windfall.Content.Items.Tools;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Common.Systems
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
            public bool Unlocked = false;
            public List<string> Objectives;
            public List<int> ObjectiveRequirements;
            public List<int> ObjectiveProgress;
            public List<QuestItem> QuestGifts;
            public List<QuestItem> QuestRewards;
            public Quest(string name, List<string> objective, bool completed, bool unlocked, List<int> objReq, List<int> objProg, bool active, List<QuestItem> questGifts, List<QuestItem> questRewards)
            {
                Name = name;
                Completed = completed;
                Active = active;
                Unlocked = unlocked;
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
            EnsureQuestLogUpToDate(InitializedQuestLog());
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["QuestLog"] = QuestLog;
        }
        internal static Quest CreateQuest(string Name, List<string> Objectives, List<int> ObjectiveRequirements, bool Unlocked, List<QuestItem> QuestGifts = null, List<QuestItem> QuestRewards = null)
        {
            List<int> objectiveProgress = new();
            for (int i = 0; i < ObjectiveRequirements.Count; i++)
            {
                objectiveProgress.Add(0);
            }
            return new Quest { Name = Name, Completed = false, Unlocked = Unlocked, Objectives = Objectives, ObjectiveProgress = objectiveProgress, ObjectiveRequirements = ObjectiveRequirements, Active = false, QuestGifts = QuestGifts, QuestRewards = QuestRewards };
        }
        internal static List<Quest> InitializedQuestLog()
        {
            List<Quest> list = new()
            {
                CreateQuest
                (
                    "CnidrionHunt", 
                    new List<string>{"Pacify 3 Cnidrions"}, 
                    new List<int>{3}, 
                    true, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<Cnidrisnack>(), Stack = 5 } }, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<CnidrionBanner>(), Stack = 4 }, new QuestItem {Type = ModContent.ItemType<AmidiasSpark>(), Stack = 1} }
                ),
                CreateQuest
                (
                    "ScoogHunt", 
                    new List<string>{"Defeat Desert Scourge"}, 
                    new List<int>{1}, 
                    true, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<AncientIlmeranRod>(), Stack = 1 }, new QuestItem { Type = ModContent.ItemType<Cnidrisnack>(), Stack = 5 } }, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<DesertScourgeTrophy>(), Stack = 1 } }
                ),
                CreateQuest
                (
                    "ShuckinClams", 
                    new List<string>{"Shuck 8 Clams"}, 
                    new List<int>{8}, 
                    true, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<IlmeranKnife>(), Stack = 1 } }, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<ClamBanner>(), Stack = 4 } }
                ),
                CreateQuest
                (
                    "ClamHunt", 
                    new List<string>{"Defeat the Giant Clam"}, 
                    new List<int>{1}, 
                    true, 
                    null, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<GiantClamTrophy>(), Stack = 1 } }
                ),
                CreateQuest
                (
                    "ScoogHunt2", 
                    new List<string>{"Defeat Aquatic Scourge"}, 
                    new List<int>{1}, 
                    Main.hardMode, 
                    null, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<AquaticScourgeTrophy>(), Stack = 1 } }
                ),
                CreateQuest
                (
                    "ParryIntro",
                    new List<string>{"Land 5 successful parries."},
                    new List<int>{1},
                    true,
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<ParryBlade>(), Stack = 1 } },
                    new List<QuestItem>{ new QuestItem { Type = ItemID.FrogLeg, Stack = 1 } }
                ),
                CreateQuest
                (
                    "SlimeGodHunt", 
                    new List<string>{"Defeat Slime God"}, 
                    new List<int>{1}, 
                    DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<RuneOfGula>(), Stack = 1 } }, 
                    new List<QuestItem>{ new QuestItem { Type = ModContent.ItemType<SlimeGodTrophy>(), Stack = 1 } }
                ),
                CreateQuest
                (
                    "CrystalSearch",
                    new List<string>{"Find a Gelatine Crystal"},
                    new List<int>{1},
                    Main.hardMode,
                    null,
                    new List<QuestItem>{ new QuestItem { Type = ItemID.PixieDust, Stack = 25 }, new QuestItem { Type = ItemID.UnicornHorn, Stack = 3 }, new QuestItem { Type = ItemID.PinkGel, Stack = 10 } }
                ),
                CreateQuest
                (
                    "QueenSlimeHunt",
                    new List<string>{"Defeat Queen Slime"},
                    new List<int>{1},
                    true,
                    null,
                    new List<QuestItem>{ new QuestItem { Type = ItemID.QueenSlimeTrophy, Stack = 1 } }
                ),

            };
            return list;
        }
        internal static void EnsureQuestLogUpToDate(List<Quest> initList)
        {
            for (int i = 0; i < initList.Count; i++)
            {
                if (i >= QuestLog.Count)
                    QuestLog.Add(initList[i]);
                else
                {
                    //Ensures changed details about pre-existing quests are updated

                    Quest temp = QuestLog[i];
                    if (temp.Name != initList[i].Name)
                        temp.Name = initList[i].Name;
                    if (temp.Unlocked != initList[i].Unlocked)
                        temp.Unlocked = initList[i].Unlocked;
                    if (temp.QuestGifts != initList[i].QuestGifts)
                        temp.QuestGifts = initList[i].QuestGifts;
                    if (temp.ObjectiveRequirements != initList[i].ObjectiveRequirements)
                        temp.ObjectiveRequirements = initList[i].ObjectiveRequirements;
                    if (temp.QuestRewards != initList[i].QuestRewards)
                        temp.QuestRewards = initList[i].QuestRewards;
                    if (temp.Objectives != initList[i].Objectives)
                        temp.Objectives = initList[i].Objectives;
                    for (int j = 0; j < temp.ObjectiveRequirements.Count; j++)
                        if (temp.ObjectiveProgress.Count <= j)
                            temp.ObjectiveProgress.Add(initList[i].ObjectiveRequirements[j]);
                        else if (temp.ObjectiveProgress[j] >= temp.ObjectiveRequirements[j])
                        {
                            temp.ObjectiveProgress[j] = temp.ObjectiveRequirements[j];
                        }
                    QuestLog[i] = temp;
                }
            }
        }
        public static void IncrementQuestProgress(int questIndex, int questReqIndex)
        {
            if (QuestLog[questIndex].ObjectiveProgress[questReqIndex] < QuestLog[questIndex].ObjectiveRequirements[questReqIndex])
            {
                Quest temp = QuestLog[questIndex];
                temp.ObjectiveProgress[questReqIndex]++;
                if (temp.ObjectiveProgress[questReqIndex] >= temp.ObjectiveRequirements[questReqIndex])
                    temp.Completed = true;
                QuestLog[questIndex] = temp;
            }
            else if (!QuestLog[questIndex].Completed)
            {
                Quest temp = QuestLog[questIndex];
                temp.Completed = true;
                QuestLog[questIndex] = temp;
            }
            Main.NewText("Quest Progress: " + QuestLog[questIndex].ObjectiveProgress[0], Color.Yellow);
            if (QuestLog[questIndex].Completed)
            {
                Main.NewText($"Quest Complete!", Color.Yellow);
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
                ["unlocked"] = value.Unlocked,
                ["objectives"] = value.Objectives,
                ["objReqs"] = value.ObjectiveRequirements,
                ["objProg"] = value.ObjectiveProgress,
                ["gifts"] = value.QuestGifts,
                ["rewards"] = value.QuestRewards
            };

            public override Quest Deserialize(TagCompound tag) => new(tag.GetString("name"), (List<string>)tag.GetList<string>("objectives"), tag.GetBool("completion"), tag.GetBool("unlocked"), (List<int>)tag.GetList<int>("objReqs"), (List<int>)tag.GetList<int>("objProg"), tag.GetBool("active"), (List<QuestItem>)tag.GetList<QuestItem>("gifts"), (List<QuestItem>)tag.GetList<QuestItem>("rewards"));
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

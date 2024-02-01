using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Windfall.Systems
{
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
            public Quest(string name, List<string> objective, bool completed, List<int> objReq, List<int> objProg, bool active)
            {
                Name = name;
                Completed = completed;
                Active = active;
                Objectives = objective;
                ObjectiveRequirements = objReq;
                ObjectiveProgress = objProg;
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
        internal static Quest CreateQuest(string Name, List<string>Objectives, List<int>ObjectiveRequirements)
        {
            List<int> objectiveProgress = new List<int>();
            for(int i = 0; i < ObjectiveRequirements.Count; i++ )
            {
                objectiveProgress.Add(0);
            }
            return new Quest { Name = Name, Completed = false, Objectives = Objectives, ObjectiveProgress = objectiveProgress, ObjectiveRequirements = ObjectiveRequirements, Active = false};
        }
        internal static List<Quest> InitializeQuestLog()
        {
            List<Quest> list = new()
            {
                CreateQuest("CnidrionHunt", new List<string>{"Pacify 5 Cnidrions"}, new List<int>{5}),
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
            public override TagCompound Serialize(Quest value) => new TagCompound
            {
                ["name"] = value.Name,
                ["completion"] = value.Completed,
                ["active"] = value.Active,
                ["objectives"] = value.Objectives,
                ["objReqs"] = value.ObjectiveRequirements,
                ["objProg"] = value.ObjectiveProgress,

            };

            public override Quest Deserialize(TagCompound tag) => new Quest(tag.GetString("name"), (List<string>)tag.GetList<string>("objectives"), tag.GetBool("completion"), (List<int>)tag.GetList<int>("objReqs"), (List<int>)tag.GetList<int>("objProg"), tag.GetBool("active"));
        }
    }
}

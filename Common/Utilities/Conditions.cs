﻿using Terraria;
using static Windfall.Common.Systems.QuestSystem;

namespace Windfall.Common.Utilities
{
    public class WindfallConditions
    {
        public static Condition CnidrionHuntCompleted = new("While the Cndirion Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "CnidrionHunt")].Completed);
        public static Condition ScoogHunt1ActiveOrCompleted = new("While the Desert Scourge Hunt Quest is active or completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Active || QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Completed);
        public static Condition ScoogHunt1Completed = new("While the Desert Scourge Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Completed);
        public static Condition ShuckinClamsActiveOrCompleted = new("While the Shuckin Clams Quest is active or completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Active || QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Completed);
        public static Condition ShuckinClamsCompleted = new("While the Shuckin Clams Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Completed);
        public static Condition ClamHuntCompleted = new("While the Giant Clam Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ClamHunt")].Completed);
        public static Condition ScoogHunt2Completed = new("While the Aquatic Scourge Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2")].Completed);
    }
}
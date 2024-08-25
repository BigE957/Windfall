using Terraria.GameContent.ItemDropRules;
using Windfall.Content.Items.Quest.SealingRitual;
using Windfall.Content.Items.Quest.Seamstress;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.TravellingNPCs;
using static Windfall.Common.Systems.QuestSystem;

namespace Windfall.Common.Utils
{
    public class WindfallConditions
    {
        #region Quest Conditions
        public static Condition CnidrionHuntCompleted = new("After the Cndirion Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "CnidrionHunt")].Completed);
        public static Condition ScoogHunt1ActiveOrCompleted = new("While the Desert Scourge Hunt Quest is active or completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Active || QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Completed);
        public static Condition ScoogHunt1Completed = new("After the Desert Scourge Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt")].Completed);
        public static Condition ShuckinClamsActiveOrCompleted = new("While the Shuckin Clams Quest is active or completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Active || QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Completed);
        public static Condition ShuckinClamsCompleted = new("After the Shuckin Clams Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ShuckinClams")].Completed);
        public static Condition ClamHuntCompleted = new("After the Giant Clam Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ClamHunt")].Completed);
        public static Condition ScoogHunt2Completed = new("After the Aquatic Scourge Hunt Quest is completed", () => QuestLog[QuestLog.FindIndex(quest => quest.Name == "ScoogHunt2")].Completed);
        public static Condition InsigniaQuestActive = new("While the Deific Insignia Quest is active", () => TravellingCultist.QuestArtifact.Type == ModContent.ItemType<DeificInsignia>() && NPC.AnyNPCs(ModContent.NPCType<TravellingCultist>()));
        #endregion

        public static IItemDropRuleCondition OratorNeverHeal = DropHelper.If((DropAttemptInfo info) => TheOrator.noSpawnsEscape, () => TheOrator.noSpawnsEscape, "If Orator never heals during the fight");
    }
}

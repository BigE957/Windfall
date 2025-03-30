using CalamityMod;
using Terraria.GameContent.ItemDropRules;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Common.Utils;

public class WindfallConditions
{
    #region Quest Conditions
    
    public static Condition CnidrionHuntCompleted = new("After the Cndirion Hunt Quest is completed", () => QuestSystem.Quests["CnidrionHunt"].Complete);
    public static Condition ScoogHunt1ActiveOrComplete = new("While the Desert Scourge Hunt Quest is active or completed", () => QuestSystem.Quests["ScoogHunt"].Active || QuestSystem.Quests["ScoogHunt"].Complete);
    public static Condition ScoogHunt1Complete = new("After the Desert Scourge Hunt Quest is completed", () => QuestSystem.Quests["ScoogHunt"].Complete);
    public static Condition ShuckinClamsActiveOrCompleted = new("While the Shuckin Clams Quest is active or completed", () => QuestSystem.Quests["ShuckinClams"].Active || QuestSystem.Quests["ShuckinClams"].Complete);
    public static Condition ShuckinClamsCompleted = new("After the Shuckin Clams Quest is completed", () => QuestSystem.Quests["ShuckinClams"].Complete);
    public static Condition ClamHuntCompleted = new("After the Giant Clam Hunt Quest is completed", () => QuestSystem.Quests["ClamHunt"].Complete);
    public static Condition ScoogHunt2Completed = new("After the Aquatic Scourge Hunt Quest is completed", () => QuestSystem.Quests["ScoogHunt2"].Complete);

    #endregion

    public static IItemDropRuleCondition OratorNeverHeal = DropHelper.If((DropAttemptInfo info) => TheOrator.noSpawnsEscape, () => TheOrator.noSpawnsEscape, "If Orator never heals during the fight");
}

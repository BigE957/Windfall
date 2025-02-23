using Terraria.GameContent.ItemDropRules;
using Windfall.Content.Items.Quest.SealingRitual;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Utils;

public class WindfallConditions
{
    public static IItemDropRuleCondition OratorNeverHeal = DropHelper.If((DropAttemptInfo info) => TheOrator.noSpawnsEscape, () => TheOrator.noSpawnsEscape, "If Orator never heals during the fight");
}

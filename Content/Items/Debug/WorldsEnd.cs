using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Quests.SealingRitual;

namespace Windfall.Content.Items.Debug;

public class WorldsEnd : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Debug";
    public override string Texture => "CalamityMod/Items/Weapons/Rogue/BallisticPoisonBomb";
    public override void SetDefaults()
    {
        Item.width = 25;
        Item.height = 29;
        Item.rare = ItemRarityID.Red;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        uiSystem.DisplayDialogueTree(Windfall.Instance, "TravellingCultist/QuestProgress/QuestLightShard", new(Name, [0]));
        return true;
    }
}
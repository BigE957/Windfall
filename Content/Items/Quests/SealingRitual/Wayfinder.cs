namespace Windfall.Content.Items.Quests.SealingRitual;

public class Wayfinder : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "CalamityMod/Items/Accessories/AscendantInsignia";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 99;
    }
}

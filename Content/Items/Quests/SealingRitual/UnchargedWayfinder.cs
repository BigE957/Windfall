
namespace Windfall.Content.Items.Quests.SealingRitual;

public class UnchargedWayfinder : ModItem, ILocalizedModType
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

    public override bool CanRightClick() => true;
    public override void RightClick(Player player)
    {
        Item.type = ModContent.ItemType<Wayfinder>(); 
    }
}

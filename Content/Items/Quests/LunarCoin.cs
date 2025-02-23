namespace Windfall.Content.Items.Quests;

public class LunarCoin : ModItem, ILocalizedModType
{
    public override string Texture => "Windfall/Assets/Items/Quest/LunarCoin";
    public new string LocalizationCategory => "Items.Quest";
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 99;
    }
}

namespace Windfall.Content.Items.Quest
{
    public class DeificInsignia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Quest;
            Item.maxStack = 99;
        }
    }
}

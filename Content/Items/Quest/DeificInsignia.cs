namespace Windfall.Content.Items.Quests
{
    public class DeificInsignia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "Windfall/Assets/Items/Quest/DeificInsignia";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.rare = ItemRarityID.Quest;
            Item.maxStack = 99;
        }
    }
}

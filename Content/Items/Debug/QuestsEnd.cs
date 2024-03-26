using Windfall.Common.Systems;

namespace Windfall.Items.Debug
{
    public class QuestsEnd : ModItem, ILocalizedModType
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
            QuestSystem.QuestLog = QuestSystem.InitializedQuestLog();
            return true;
        }
    }
}
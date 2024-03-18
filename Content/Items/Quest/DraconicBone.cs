using Terraria.ID;
using Terraria.ModLoader;

namespace Windfall.Content.Items.Quests
{
    public class DraconicBone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "Windfall/Assets/Items/Quest/DeificInsignia";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 99;
        }
    }
}

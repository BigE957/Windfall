using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.Items.Quest
{
    public class WFEidolonTablet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "CalamityMod/Items/SummonItems/EidolonTablet";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<HideoutSeeker>();
            Item.shootSpeed = 20f;
        }
    }
}

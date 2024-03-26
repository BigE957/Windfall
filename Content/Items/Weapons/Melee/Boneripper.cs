using CalamityMod.Items;
using Windfall.Content.Projectiles.Melee;

namespace Windfall.Content.Items.Weapons.Melee
{
    public class Boneripper : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Boneripper";

        public override void SetDefaults()
        {
            Item.damage = 1075;
            Item.knockBack = 7.5f;
            Item.useAnimation = Item.useTime = 25;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.noMelee = true;
            Item.channel = true;
            Item.autoReuse = true;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<BoneripperProj>();
            Item.width = 90;
            Item.height = 90;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.DD2_SkyDragonsFurySwing;
            Item.value = CalamityGlobalItem.Rarity7BuyPrice;
            Item.rare = ItemRarityID.Lime;
        }
    }
}
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;
using Windfall.Projectiles.Melee;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Windfall.Items.Weapons.Melee
{
    public class Boneripper : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
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
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Rarities;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Projectiles.Other;
using Terraria.DataStructures;

namespace Windfall.Items.Debug
{
    public class DraeDashGiftCard : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override void SetDefaults()
        {
            Item.width = 25;
            Item.height = 29;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.shoot = ModContent.ProjectileType<DraeDashDropPod>();
            Item.useTime = Item.useAnimation = 10;
            Item.UseSound = SoundID.Item82;
            Item.shootSpeed = 10f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = Main.MouseWorld - Vector2.UnitY * 1020f;
            velocity = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(9f, 10f);
            Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI, Main.MouseWorld.Y - 40f);
            return false;
        }
    }
}
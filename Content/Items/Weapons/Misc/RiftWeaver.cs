using CalamityMod.Items;
using Windfall.Content.Projectiles.Weapons.Misc;

namespace Windfall.Content.Items.Weapons.Misc
{
    internal class RiftWeaver : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Misc";
        public override string Texture => "Windfall/Assets/Items/Weapons/Misc/RiftWeaver";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 250;
            Item.crit = 2;
            Item.knockBack = 0.2f;
            Item.useAnimation = Item.useTime = 18;
            Item.DamageType = DamageClass.Default;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shootSpeed = 3.3f;
            Item.shoot = ModContent.ProjectileType<RiftWeaverStab>();

            Item.width = 54;
            Item.height = 54;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.UseSound = null;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
        }

        public override bool CanUseItem(Player player)
        {
            return true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (!Main.projectile.Any(p => p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<RiftWeaverThrow>()))
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), player.Center - (Vector2.UnitY * player.height / 2.5f) - (Vector2.UnitX * player.width / 2f * player.direction), Vector2.Zero, ModContent.ProjectileType<RiftWeaverThrow>(), Item.damage, Item.knockBack * 1.5f);
                else
                    return false;
            }
            else if (!AnyProjectiles(ModContent.ProjectileType<RiftWeaverThrow>()))
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.6f }, player.Center);
                return true;
            }
            else
            {
                Projectile weaver = Main.projectile.First(p => p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<RiftWeaverThrow>());
                if (weaver.ai[2] > 0)
                    weaver.ai[2] = 190;
                else if (weaver.ai[1] < 45)
                {
                    weaver.ai[1] = 45;
                    if (weaver.velocity.LengthSquared() < 256)
                        weaver.velocity = (player.Center - weaver.Center).SafeNormalize(Vector2.Zero) * 16f;
                }
            }
            return false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture+"Glow", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw
            (
                texture,
                Item.Center - Main.screenPosition,
                new Rectangle(0, 0, texture.Width, texture.Height),
                Color.White,
                rotation,
                texture.Size() * 0.5f,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}

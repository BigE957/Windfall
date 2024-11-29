using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using Windfall.Content.Projectiles.Weapons.Rogue;

namespace Windfall.Content.Items.Weapons.Rogue;
public class OratorSpear : RogueWeapon, ILocalizedModType
{
    public override string Texture => "Windfall/Assets/Items/Weapons/Rogue/OratorSpear";

    public override void SetDefaults()
    {
        Item.damage = 1;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 1f;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.maxStack = 1;
        Item.shoot = ModContent.ProjectileType<OratorSpearProj>();
        Item.shootSpeed = 22f;
        Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
    }
    public override float StealthVelocityMultiplier => 2f;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable())
        {
            int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }
        return true;
    }
}
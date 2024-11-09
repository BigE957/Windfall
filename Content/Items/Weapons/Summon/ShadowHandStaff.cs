using CalamityMod.Items;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Projectiles.Weapons.Summon;

namespace Windfall.Content.Items.Weapons.Summon;

public class ShadowHandStaff : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";
    public override string Texture => "Windfall/Assets/Items/Weapons/Summon/ShadowHandStaff";
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;
        Item.damage = 280;
        Item.mana = 10;
        Item.useTime = Item.useAnimation = 34;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 0.5f;
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
        Item.UseSound = SoundID.Item103;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<ShadowHand_Minion>();
        Item.shootSpeed = 10f;
        Item.DamageType = DamageClass.Summon;
    }

    public override void HoldItem(Player player)
    {
        player.Calamity().rightClickListener = true;
        player.Calamity().mouseWorldListener = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse != 2)
        {
            position = Main.MouseWorld;
            int seeker = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
            if (Main.projectile.IndexInRange(seeker))
                Main.projectile[seeker].originalDamage = Item.damage;
            for (int i = 0; i <= 20; i++)
            {
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));
            }
        }
        return false;
    }
}


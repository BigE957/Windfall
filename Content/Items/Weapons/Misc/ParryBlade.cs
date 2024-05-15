using Windfall.Common.Utils;
using Windfall.Content.Buffs.Cooldowns;
using Windfall.Content.Projectiles.Weapons.Misc;


namespace Windfall.Content.Items.Weapons.Misc
{
    public class ParryBlade : ModItem
    {
        public override string Texture => "CalamityMod/Items/Weapons/Melee/CosmicShiv";
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.knockBack = 4f;
            Item.useStyle = ItemUseStyleID.Rapier; // Makes the player do the proper arm motion
            Item.useAnimation = Item.useTime = 8;
            Item.width = 32;
            Item.height = 32;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.Default;
            Item.autoReuse = false;
            Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
            Item.noMelee = true; // The projectile will do the damage and not the item

            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 0, 10);

            Item.shoot = ModContent.ProjectileType<ParryBladeProj>(); // The projectile is what makes a shortsword work
            Item.shootSpeed = 2.1f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values
        }
        public override bool CanUseItem(Player player)
        {
            if (player.HasCooldown(ParryWeapon.ID))
                return false;
            else
                return true;
        }
        public override bool? UseItem(Player player)
        {
            if (!player.Buff().PerfectFlow)
                player.AddCooldown(ParryWeapon.ID, SecondsToFrames(30));
            return true;
        }
    }
}

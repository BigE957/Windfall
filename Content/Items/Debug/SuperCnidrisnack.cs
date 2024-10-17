using CalamityMod.Items;
using Windfall.Content.Projectiles.Weapons.Misc;

namespace Windfall.Content.Items.Debug
{
    public class SuperCnidrisnack : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override string Texture => "Windfall/Assets/Items/Debug/SuperCnidrisnack";

        public override void SetDefaults()
        {
            Item.damage = 1;
            Item.knockBack = 0f;
            Item.useAnimation = Item.useTime = 10;
            Item.DamageType = DamageClass.Default;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.consumable = true;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<SuperCnidrisnackThrow>();
            Item.maxStack = 99;
            Item.bait = 25;

            Item.width = 32;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item7;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.FishingBait;
        }        
    }
}

using CalamityMod.Items;
using Windfall.Content.Projectiles.Misc;

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
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/Items/Debug/SuperCnidrisnack_Glow", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw
            (
                texture,
                new Vector2(Item.Center.X - Main.screenPosition.X, Item.Center.Y - Main.screenPosition.Y),
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

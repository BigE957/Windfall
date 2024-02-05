using CalamityMod.Items;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.Items.Debug
{
    public class SuperCnidrisnack : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override string Texture => "Windfall/Assets/Items/Weapons/Misc/Cnidrisnack";

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
            Item.shoot = ModContent.ProjectileType<CnidrisnackThrow>();
            Item.maxStack = 99;
            Item.bait = 25;

            Item.width = Item.height = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item7;
            Item.value = CalamityGlobalItem.Rarity7BuyPrice;
            Item.rare = ItemRarityID.Blue;
        }
        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.FishingBait;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Misc/Cnidrisnack_Glow", AssetRequestMode.ImmediateLoad).Value;
            spriteBatch.Draw
            (
                texture,
                new Vector2(Item.Center.X - Main.screenPosition.X, Item.Center.Y - 8 - Main.screenPosition.Y),
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

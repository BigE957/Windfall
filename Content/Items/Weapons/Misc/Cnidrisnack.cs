﻿using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.Items.Weapons.Misc
{
    public class Cnidrisnack : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Misc";
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
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Misc/CnidrisnackGlow").Value);
        }
    }
}
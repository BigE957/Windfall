﻿using CalamityMod.Rarities;

namespace Windfall.Content.Items.Essences
{
    public class EoWEssence : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Essence";
        public override string Texture => "CalamityMod/Items/Ammo/BloodRune";

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.consumable = true;
            Item.UseSound = SoundID.Roar;
            Item.rare = ModContent.RarityType<CalamityRed>();
        }
        public override bool? UseItem(Player player)
        {
            player.Godly().Evil1Essence = true;
            return true;
        }
    }
}

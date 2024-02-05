using CalamityMod.Items;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.NPCs.WanderingNPCs;
using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Items.Utility
{
    public class IlmeranHorn : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "Windfall/Assets/Items/Utility/IlmeranHorn";

        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/IlmeranHorn");

        public override void SetDefaults()
        {
            Item.useAnimation = Item.useTime = 60;
            Item.width = 28;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = CalamityGlobalItem.Rarity2BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = UseSound;
            Item.useTurn = true;
        }
        public override bool? UseItem(Player player)
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladinKnocked>()) && player.ZoneDesert)
            {
                Projectile.NewProjectile(null, new Vector2(player.Center.X - 80 * player.direction, player.Center.Y + Main.rand.Next(150, 200)), new Vector2(0, -8), ModContent.ProjectileType<IlmeranPaladinDig>(), 0, 0);
            }
            return true;
        }
    }
}

using CalamityMod.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Placeables.DraedonStructures;
using CalamityMod.Items;
using System.Collections.Generic;
using CalamityMod.Rarities;
using Windfall.Content.Tiles.Special;

namespace Windfall.Content.Items.Placeables.Special
{
    internal class DatabaseTerminalItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "Windfall/Assets/Items/Placeables/Special/DatabaseTerminalItem";

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<DatabaseTerminal>();
            Item.rare = ModContent.RarityType<DarkOrange>();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 1);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LaboratoryPlating>(15)
                .AddIngredient<MysteriousCircuitry>(8)
                .AddIngredient<DubiousPlating>(4)
                .AddIngredient<AerialiteBar>(4)
                .AddIngredient<SeaPrism>(7)
                .AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(1, out var condition), condition)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}

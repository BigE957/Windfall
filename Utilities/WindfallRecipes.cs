﻿using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using CalamityMod.Items.SummonItems;

namespace WindfallAttempt1.Utilities
{
    public class WindfallRecipes : ModSystem
    {
        public override void PostAddRecipes()
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe.HasResult(ModContent.ItemType<EyeofDesolation>()))
                {
                    recipe.AddIngredient(ModContent.ItemType<AshesofCalamity>(), 5);
                }
            }
        }
    }
}

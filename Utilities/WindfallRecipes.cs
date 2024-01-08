using CalamityMod.Items.Weapons.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;

namespace WindfallAttempt1.Utilities
{
    public class WindfallRecipes : ModSystem
    {
        public override void PostAddRecipes()
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe.HasResult(ModContent.ItemType<TearsofHeaven>()))
                {
                    recipe.AddIngredient(ModContent.ItemType<AshesofCalamity>(), 5);
                }
            }
        }
    }
}

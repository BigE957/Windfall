using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using CalamityMod.Items.SummonItems;

namespace Windfall.Systems
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
                if (recipe.HasResult(ModContent.ItemType<DesertMedallion>()))
                {
                    recipe.AddIngredient(ModContent.ItemType<PearlShard>(), 5);
                }
            }
        }
    }
}

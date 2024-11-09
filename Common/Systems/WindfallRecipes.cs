using CalamityMod.Items.Materials;
using CalamityMod.Items.SummonItems;

namespace Windfall.Common.Systems;

public class WindfallRecipes : ModSystem
{
    public override void PostAddRecipes()
    {
        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe.HasResult(ModContent.ItemType<DesertMedallion>()))
            {
                recipe.AddIngredient(ModContent.ItemType<PearlShard>(), 5);
            }
            if (recipe.HasResult(ModContent.ItemType<Seafood>()))
            {
                recipe.AddIngredient(ModContent.ItemType<CorrodedFossil>(), 5);
            }
            if (recipe.HasResult(ModContent.ItemType<EyeofDesolation>()))
            {
                recipe.AddIngredient(ModContent.ItemType<AshesofCalamity>(), 5);
            }
            if (recipe.HasResult(ModContent.ItemType<OverloadedSludge>()))
            {
                recipe.AddIngredient(ModContent.ItemType<PurifiedGel>(), 5);
            }
        }
    }
}

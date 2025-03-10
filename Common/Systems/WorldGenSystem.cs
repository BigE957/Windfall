using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Windfall.Content.World;

namespace Windfall.Common.Systems;

public class WorldGenSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        // Wanderers Cabin
        int WanderersIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
        if (WanderersIndex != -1)
        {
            tasks.Insert(WanderersIndex + 2, new PassLegacy("Wanderers Cabin", (progress, config) =>
            {
                progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WorldGen.WanderersCabin").Value;
                WanderersCabin.PlaceWanderersCabin(GenVars.structures);
            }));
        }
        // All further tasks occur after vanilla worldgen is completed
        int FinalIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
        if (FinalIndex != -1)
        {
            int DraedonStructuresIndex = FinalIndex + 7;

            int currentFinalIndex = FinalIndex;
            tasks.Insert(currentFinalIndex, new PassLegacy("Lunar Cult Base", (progress, config) =>
            {
                progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WorldGen.LunarCultBase").Value;

                LunarCultBase.PlaceLunarCultBase(GenVars.structures);
            }));

            tasks.Insert(currentFinalIndex, new PassLegacy("Draconic Ruins", (progress, config) =>
            {
                progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WorldGen.DragonMonkRuins").Value;

                DragonMonkRuins.PlaceDraconicRuins(GenVars.structures);
            }));
        }
    }
    public override void PostWorldGen()
    {
        ChestAdditions.FillChestsAgain();
    }
}

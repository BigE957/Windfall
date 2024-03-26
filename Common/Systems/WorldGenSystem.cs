using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Windfall.Content.World;

namespace Windfall.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            //Place Summoning Grounds (got scrapped lol)
            /*
            int DungeonIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Settle Liquids Again"));
            if (DungeonIndex != -1)
            {
                tasks.Insert(DungeonIndex + 1, new PassLegacy("Summoning Grounds", (progress, config) =>
                {
                    progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WorldGen.SummoningGrounds").Value;
                    SummoningGrounds.PlaceSummoningGrounds(GenVars.structures);
                }));
            }
            */

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
                int currentFinalIndex = FinalIndex;
                tasks.Insert(++currentFinalIndex, new PassLegacy("Lunar Cult Hideouts", (progress, config) =>
                {
                    progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WorldGen.LunarHideouts").Value;

                    CultistHideouts.PlaceSolarCultistHideout(GenVars.structures);
                    CultistHideouts.PlaceVortexCultistHideout(GenVars.structures);
                    CultistHideouts.PlaceNebulaCultistHideout(GenVars.structures);
                    CultistHideouts.PlaceStardustCultistHideout(GenVars.structures);
                }));
            }
        }
        public override void PostWorldGen()
        {
            ChestAdditions.FillChestsAgain();
        }
    }
}

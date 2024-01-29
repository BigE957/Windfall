using System.Collections.Generic;
using CalamityMod.World;
using CalamityMod.World.Planets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using CalamityMod.Items.SummonItems;
using static CalamityMod.World.CalamityWorld;
using Terraria.Localization;
using Windfall.World;

namespace Windfall.Systems
{
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
                    progress.Message = Language.GetOrRegister("Mods.Windfall.UI.WanderersCabin").Value;
                    WanderersCabin.PlaceWanderersCabin(GenVars.structures);
                }));
            }
        }
        public override void PostWorldGen()
        {
            ChestAdditions.FillChestsAgain();
        }
    }        
}

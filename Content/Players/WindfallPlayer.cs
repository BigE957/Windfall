using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Windfall.Content.Players
{
    public class WindfallPlayer : ModPlayer
    {
        public bool PerfectFlow = false;

        public override void ResetEffects()
        {
            PerfectFlow = false;
        }
        public override void UpdateDead()
        {
            PerfectFlow = false;
        }
    }
}
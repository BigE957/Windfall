using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Windfall.Utilities
{
    public class DownedNPCSystem : ModSystem
    {
        internal static bool _downedCnidrion = false;
        internal static bool _downedSirenLure = false;

        public static bool downedCnidrion
        {
            get => _downedCnidrion;
            set
            {
                if (!value)
                    _downedCnidrion = false;
                else
                    NPC.SetEventFlagCleared(ref _downedCnidrion, -1);
            }
        }
        public static bool downedSirenLure
        {
            get => _downedSirenLure;
            set
            {
                if (!value)
                    _downedSirenLure = false;
                else
                    NPC.SetEventFlagCleared(ref _downedSirenLure, -1);
            }
        }
        internal static void ResetAllFlags()
        {
            downedCnidrion = false;
            downedSirenLure = false;
        }
        public override void OnWorldLoad() => ResetAllFlags();

        public override void OnWorldUnload() => ResetAllFlags();

        public override void SaveWorldData(TagCompound tag)
        {
            List<string> downed = new();

            if (downedCnidrion)
                downed.Add("Cnidrion");
            tag["downedFlags"] = downed;

            if (downedSirenLure)
                downed.Add("SirenLure");
            tag["downedFlags"] = downed;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> downed = tag.GetList<string>("downedFlags");

            downedCnidrion = downed.Contains("Cnidrion");
            downedSirenLure = downed.Contains("SirenLure");
        }
    }
}

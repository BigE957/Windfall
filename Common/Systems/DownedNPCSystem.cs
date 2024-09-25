using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems
{
    public class DownedNPCSystem : ModSystem
    {
        internal static bool _downedCnidrion = false;
        internal static bool _downedSirenLure = false;
        internal static bool _downedEvil2Summon = false;
        internal static bool _downedOrator = false;

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
        public static bool downedEvil2Summon
        {
            get => _downedEvil2Summon;
            set
            {
                if (!value)
                    _downedEvil2Summon = false;
                else
                    NPC.SetEventFlagCleared(ref _downedEvil2Summon, -1);
            }
        }
        public static bool downedOrator
        {
            get => _downedOrator;
            set
            {
                if (!value)
                    _downedOrator = false;
                else
                    NPC.SetEventFlagCleared(ref _downedOrator, -1);
            }
        }
        internal static void ResetAllFlags()
        {
            downedCnidrion = false;
            downedSirenLure = false;
            downedEvil2Summon = false;
            downedOrator = false;
        }
        public override void OnWorldLoad() => ResetAllFlags();

        public override void OnWorldUnload() => ResetAllFlags();

        public override void SaveWorldData(TagCompound tag)
        {
            List<string> downed = [];

            if (downedCnidrion)
                downed.Add("Cnidrion");
            tag["downedFlags"] = downed;

            if (downedSirenLure)
                downed.Add("SirenLure");
            tag["downedFlags"] = downed;

            if (downedEvil2Summon)
                downed.Add("Evil2Summon");
            tag["downedFlags"] = downed;

            if (downedEvil2Summon)
                downed.Add("Orator");
            tag["downedFlags"] = downed;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> downed = tag.GetList<string>("downedFlags");

            downedCnidrion = downed.Contains("Cnidrion");
            downedSirenLure = downed.Contains("SirenLure");
            downedEvil2Summon = downed.Contains("Evil2Summon");
            downedOrator = downed.Contains("Orator");
        }
    }
}

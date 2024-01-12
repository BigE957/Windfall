using WindfallAttempt1.Players;
using Terraria;

namespace WindfallAttempt1.Utilities
{
    public static partial class Utilities
    {
        public static CameraEffectsPlayer Windfall_Camera(this Player player) => player.GetModPlayer<CameraEffectsPlayer>();
    }
}

using Windfall.Players;
using Terraria;

namespace Windfall.Utilities
{
    public static partial class Utilities
    {
        public static CameraEffectsPlayer Windfall_Camera(this Player player) => player.GetModPlayer<CameraEffectsPlayer>();
    }
}

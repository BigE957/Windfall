using Terraria;
using Windfall.Content.Players;

namespace Windfall.Common.Utilities
{
    public static partial class Utilities
    {
        public static CameraEffectsPlayer Windfall_Camera(this Player player) => player.GetModPlayer<CameraEffectsPlayer>();
    }
}

using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Windfall.Common.Utilities;

namespace Windfall.Common.Utilities
{
    public static partial class Utilities
    {
        public static Point GetGroundPositionFrom(Point p, GenSearch search = null)
        {
            search ??= new Searches.Down(9001);

            if (!WorldUtils.Find(p, Searches.Chain(search, new Conditions.IsSolid(), new ActiveAndNotActuated()), out Point result))
                return result;
            return result;
        }

        public static Vector2 GetGroundPositionFrom(Vector2 v, GenSearch search = null)
        {
            search ??= new Searches.Down(9001);
            if (!WorldUtils.Find(v.ToTileCoordinates(), Searches.Chain(search, new Conditions.IsSolid(), new ActiveAndNotActuated(), new NotPlatform()), out Point result))
                return v;
            return result.ToWorldCoordinates();
        }
    }
}

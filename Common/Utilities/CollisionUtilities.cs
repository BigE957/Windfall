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
        public static bool AlignProjectileWithGround(Projectile Projectile)
        {
            bool InsideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
            int i;
            if (InsideTiles)
                for (i = 0; i < 200; i++)
                {
                    InsideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
                    if (InsideTiles)
                    {
                        Projectile.position.Y -= 16;
                    }
                    else
                    {
                        for(i = 0; i < 16; i++)
                        {
                            InsideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
                            if (InsideTiles)
                                break;
                            Projectile.position.Y++;
                        }
                        Projectile.position.Y--;
                        return true;
                    }
                }
            else
                for (i = 0; i < 200; i++)
                {
                    InsideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
                    if (InsideTiles)
                    {
                        for (i = 0; i < 16; i++)
                        {
                            InsideTiles = Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
                            if (!InsideTiles)
                                break;
                            Projectile.position.Y--;
                        }
                        Projectile.position.Y++;
                        return true;
                    }
                    else
                    {
                        Projectile.position.Y += 16;
                    }
                }
            return false;
        }
        public static bool AlignNPCWithGround(NPC npc)
        {
            bool InsideTiles = Collision.SolidCollision(npc.position, npc.width, npc.height);
            int i;
            if (InsideTiles)
                for (i = 0; i < 200; i++)
                {
                    InsideTiles = Collision.SolidCollision(npc.position, npc.width, npc.height);
                    if (InsideTiles)
                    {
                        npc.position.Y -= 16;
                    }
                    else
                    {
                        for (i = 0; i < 16; i++)
                        {
                            InsideTiles = Collision.SolidCollision(npc.position, npc.width, npc.height);
                            if (InsideTiles)
                                break;
                            npc.position.Y++;
                        }
                        npc.position.Y--;
                        return true;
                    }
                }
            else
                for (i = 0; i < 200; i++)
                {
                    InsideTiles = Collision.SolidCollision(npc.position, npc.width, npc.height);
                    if (InsideTiles)
                    {
                        for (i = 0; i < 16; i++)
                        {
                            InsideTiles = Collision.SolidCollision(npc.position, npc.width, npc.height);
                            if (!InsideTiles)
                                break;
                            npc.position.Y--;
                        }
                        npc.position.Y++;
                        return true;
                    }
                    else
                    {
                        npc.position.Y += 16;
                    }
                }
            return false;
        }
    }
}

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static bool AnyProjectiles(int projectileID)
    {
        ActiveEntityIterator<Projectile>.Enumerator enumerator = Main.ActiveProjectiles.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.type == projectileID)
            {
                return true;
            }
        }

        return false;
    }

    public static int FindFirstProjectile(int Type)
    {
        int result = -1;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.type == Type)
            {
                result = i;
                break;
            }
        }

        return result;
    }
}

using CalamityMod.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.DataStructures;
using CalamityMod;

namespace WindfallAttempt1.Utilities
{
    public class StatCorrections : ModSystem
    {
        private const double ExpertContactVanillaMultiplier = 2D;
        private const double MasterContactVanillaMultiplier = 3D;
        private const double NormalProjectileVanillaMultiplier = 2D;
        private const double ExpertProjectileVanillaMultiplier = 4D;
        private const double MasterProjectileVanillaMultiplier = 6D;
        public static int GetProjectileDamage(int projectileDamage)
        {
            double damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;

            // Safety check: If for some reason the projectile damage array is not initialized yet, return 1.


            int damageToUse = projectileDamage / (int)damageAdjustment;

            return damageToUse;
        }
    }
}

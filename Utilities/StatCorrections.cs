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
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Windfall.NPCs.WorldEvents;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;

namespace Windfall.Utilities
{
    public class StatCorrections : ModSystem
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct EnemyStats
        {
            public static SortedDictionary<int, double> ExpertDamageMultiplier;

            public static SortedDictionary<int, int[]> ContactDamageValues;

            public static SortedDictionary<Tuple<int, int>, int[]> ProjectileDamageValues;

            public static SortedDictionary<int, Tuple<bool, int[]>> DebuffImmunities;
        }

        private const double ExpertContactVanillaMultiplier = 2.0;

        private const double MasterContactVanillaMultiplier = 3.0;

        private const double NormalProjectileVanillaMultiplier = 2.0;

        private const double ExpertProjectileVanillaMultiplier = 4.0;

        private const double MasterProjectileVanillaMultiplier = 6.0;

        public static int ScaleContactDamage(int damage)
        {
            double damageAdjustment = 0.8 * (Main.masterMode ? MasterContactVanillaMultiplier : Main.expertMode ? ExpertContactVanillaMultiplier : 1);
            // If the assigned value would be -1, don't actually assign it. This allows for conditionally disabling the system.
            int damageToUse = (int)Math.Round(damage / damageAdjustment);
            return damageToUse;
        }

        // Gets the amount of damage a given projectile should do from this NPC.
        // Automatically compensates for Terraria's internal spaghetti scaling.
        public static int ScaleProjectileDamage(int damage)
        {
            double damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;

            int damageToUse = (int)Math.Round(damage / damageAdjustment);

            return damageToUse;
        }
    }
}

using CalamityMod.NPCs;
using Windfall.Content.Items.Weapons.Rogue;

namespace Windfall.Content.NPCs.GlobalNPCs;
public class DebuffGlobalNPC : GlobalNPC
{
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (npc.onFrostBurn2)
        {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            int projectileCount = Main.projectile.Where(p => p.active && p.ai[0] == 1f && p.ai[1] == npc.whoAmI && (
                    p.type == ModContent.ProjectileType<ExodiumSpearProj>()
                )).Count();

            if (projectileCount > 0)
            {                
                npc.lifeRegen -= projectileCount * 25;

                if (damage < projectileCount * 5)
                    damage = projectileCount * 5;
            }
        }
    }
    public static double HeatDamageMult(NPC npc)
    {
        bool slimeGod = CalamityLists.SlimeGodIDs.Contains(npc.type);
        bool slimed = npc.drippingSlime || npc.drippingSparkleSlime;
        double heatDamageMult = slimed ? ((wormBoss(npc) || slimeGod) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
        if (npc.Calamity().VulnerableToHeat.HasValue)
        {
            if (npc.Calamity().VulnerableToHeat.Value)
                heatDamageMult *= slimed ? ((wormBoss(npc) || slimeGod) ? 1.25 : 1.5) : ((wormBoss(npc) || slimeGod) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
            else
                heatDamageMult *= slimed ? ((wormBoss(npc) || slimeGod) ? 0.66 : 0.5) : 0.5;
        }

        if (npc.Calamity().IncreasedHeatEffects_Fireball)
            heatDamageMult += 0.25;
        if (npc.Calamity().IncreasedHeatEffects_FlameWakerBoots)
            heatDamageMult += 0.25;
        if (npc.Calamity().IncreasedHeatEffects_CinnamonRoll)
            heatDamageMult += 0.5;
        if (npc.Calamity().IncreasedHeatEffects_HellfireTreads)
            heatDamageMult += 0.5;

        return heatDamageMult;
    }
    public static double WaterDamageMult(NPC npc)
    {      
        double waterDamageMult = CalamityGlobalNPC.BaseDoTDamageMult;
        if (npc.Calamity().VulnerableToWater.HasValue)
        {
            if (npc.Calamity().VulnerableToWater.Value)
                waterDamageMult *= wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult;
            else
                waterDamageMult *= 0.5;
        }
        if (npc.Calamity().IncreasedSicknessAndWaterEffects_EvergreenGin)
        {
            waterDamageMult += 0.25;
        }
        return waterDamageMult;
    }

    public static double SicknessDamageMult(NPC npc)
    {
        double sicknessDamageMult = npc.Calamity().irradiated > 0 ? (wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
        if (npc.Calamity().VulnerableToSickness.HasValue)
        {
            if (npc.Calamity().VulnerableToSickness.Value)
                sicknessDamageMult *= npc.Calamity().irradiated > 0 ? (wormBoss(npc) ? 1.25 : 1.5) : (wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
            else
                sicknessDamageMult *= npc.Calamity().irradiated > 0 ? (wormBoss(npc) ? 0.66 : 0.5) : 0.5;
        }

        if (npc.Calamity().IncreasedSicknessEffects_ToxicHeart)
            sicknessDamageMult += 0.5;

        if (npc.Calamity().IncreasedSicknessAndWaterEffects_EvergreenGin)
            sicknessDamageMult += 0.25;
        return sicknessDamageMult;
    }

    public static double ColdDamageMult(NPC npc)
    {
        double coldDamageMult = CalamityGlobalNPC.BaseDoTDamageMult;
        if (npc.Calamity().VulnerableToCold.HasValue)
        {
            if (npc.Calamity().VulnerableToCold.Value)
                coldDamageMult *= wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult;
            else
                coldDamageMult *= 0.5;
        }
        if (npc.Calamity().IncreasedColdEffects_EskimoSet)
            coldDamageMult += 0.25;
        if (npc.Calamity().IncreasedColdEffects_CryoStone)
            coldDamageMult += 0.5;
        return coldDamageMult;
    }

    public static double ElectricityDamageMult(NPC npc)
    {
        bool increasedElectricityDamage = npc.wet || npc.honeyWet || npc.lavaWet || npc.dripping;
        double electricityDamageMult = increasedElectricityDamage ? (wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult) : CalamityGlobalNPC.BaseDoTDamageMult;
        if (npc.Calamity().VulnerableToElectricity.HasValue)
        {
            if (npc.Calamity().VulnerableToElectricity.Value)
                electricityDamageMult *= increasedElectricityDamage ? (wormBoss(npc) ? 1.25 : 1.5) : (wormBoss(npc) ? CalamityGlobalNPC.VulnerableToDoTDamageMult_Worms_SlimeGod : CalamityGlobalNPC.VulnerableToDoTDamageMult);
            else
                electricityDamageMult *= increasedElectricityDamage ? (wormBoss(npc) ? 0.66 : 0.5) : 0.5;
        }

        if (npc.Calamity().IncreasedElectricityEffects_Transformer)
            electricityDamageMult += 0.5;

        return electricityDamageMult;
    }

    public static double VanillaHeatDamageMult(NPC npc) => HeatDamageMult(npc) - CalamityGlobalNPC.BaseDoTDamageMult;

    public static double VanillaColdDamageMult(NPC npc) => ColdDamageMult(npc) - CalamityGlobalNPC.BaseDoTDamageMult;

    public static double VanillaSicknessDamageMult(NPC npc) => SicknessDamageMult(npc) - CalamityGlobalNPC.BaseDoTDamageMult;

    private static bool wormBoss(NPC npc) => CalamityLists.DesertScourgeIDs.Contains(npc.type) || CalamityLists.EaterofWorldsIDs.Contains(npc.type) || CalamityLists.PerforatorIDs.Contains(npc.type) ||
        CalamityLists.AquaticScourgeIDs.Contains(npc.type) || CalamityLists.AstrumDeusIDs.Contains(npc.type) || CalamityLists.StormWeaverIDs.Contains(npc.type);
}

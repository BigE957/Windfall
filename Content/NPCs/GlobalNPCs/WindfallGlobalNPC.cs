using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Polterghast;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Enemies;
using Windfall.Content.NPCs.Enemies.AstralSiphon;
using Windfall.Content.Projectiles.Enemies;
using Windfall.Content.Projectiles.NPCAnimations;
using Windfall.Content.Projectiles.Other;
using Windfall.Content.Projectiles.Props;
using Terraria.ID;

namespace Windfall.Content.NPCs.GlobalNPCs;

public class WindfallGlobalNPC : GlobalNPC
{
    private static SoundStyle PolterAmbiance = new("Windfall/Assets/Sounds/Ambiance/Polterghast/PolterAmbiance_", 3);
    
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        bool AstralSiphonSpawns = true;

        if (!spawnInfo.Player.InAstral(1))
            AstralSiphonSpawns = false;

        if (AstralSiphonSpawns && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive))
            AstralSiphonSpawns = false;

        if (AstralSiphonSpawns)
        {
            pool.Clear();

            pool[ModContent.NPCType<AstralSlime>()] = 0.75f;
            pool[ModContent.NPCType<Nova>()] = 0.66f;
            pool[ModContent.NPCType<AstralProbe>()] = 0.5f;
            if (Main.npc.Where(n => n.active && n.type == ModContent.NPCType<SiphonCollider>()).Count() < 5)
                pool[ModContent.NPCType<SiphonCollider>()] = 1f;
            if (Main.npc.Where(n => n.active && n.type == ModContent.NPCType<SiphonSpitter>()).Count() < 3)
                pool[ModContent.NPCType<SiphonSpitter>()] = 0.5f;
        }
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        bool AstralSiphonSpawns = true;

        if (!player.InAstral(1))
            AstralSiphonSpawns = false;

        if (AstralSiphonSpawns && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive))
            AstralSiphonSpawns = false;

        if (AstralSiphonSpawns)
        {
            spawnRate = 50;
            maxSpawns = 16;
        }
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
            npc.Transform(ModContent.NPCType<WFCnidrion>());
        if (!LunarCultBaseSystem.BetrayalActive && (npc.type == NPCID.CultistArcherBlue || npc.type == NPCID.CultistDevote))
            npc.active = false;
        if (npc.type == calamity.Find<ModNPC>("Polterghast").Type)
            SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, npc.Center);
    }
    
    public override void OnKill(NPC npc)
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type || npc.type == ModContent.NPCType<WFCnidrion>())
            DownedNPCSystem.downedCnidrion = true;

        if (npc.type == calamity.Find<ModNPC>("PerforatorCyst").Type || npc.type == calamity.Find<ModNPC>("HiveTumor").Type)
            DownedNPCSystem.downedEvil2Summon = true;

        if (npc.type == calamity.Find<ModNPC>("HiveMind").Type && !DownedBossSystem.downedHiveMind || npc.type == calamity.Find<ModNPC>("PerforatorHive").Type && !DownedBossSystem.downedPerforator)
            SpawnWorldEventProjectile(ModContent.ProjectileType<GodseekerKnightProj>(), 100);

        if (npc.type == NPCID.SkeletronHead && !NPC.downedBoss3)
            Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), new Vector2(Main.dungeonX, Main.dungeonY).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<LunaticCultistProj>(), 0, 0, -1, 1);

        if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive) && npc.type == calamity.Find<ModNPC>("Nova").Type)
        {
            NPC siphon = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SelenicSiphon>())];
            Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), npc.Center, (npc.Center - siphon.Center).SafeNormalize(Vector2.UnitX * npc.direction) * 6f, ModContent.ProjectileType<AstralEnergy>(), 0, 0f);
        }

        if (npc.type == NPCID.Plantera && !NPC.downedPlantBoss)
        {
            Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), npc.Center, Vector2.Zero, ModContent.ProjectileType<OratorEntourageSpawner>(), 0, 0);
            WorldSaveSystem.PlanteraJustDowned = true;
        }

        if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
            DownedNPCSystem.downedSirenLure = true;

        PhantomCheck(npc);
    }
    private static void SpawnWorldEventProjectile(int type, int xOffSet)
    {
        Projectile.NewProjectileDirect(Entity.GetSource_NaturalSpawn(), new Vector2(Main.player[0].Center.X + xOffSet, Main.player[0].Center.Y), Vector2.Zero, type, 0, 0);
    }

    private static void PhantomCheck(NPC npc)
    {
        if (!DownedBossSystem.downedPolterghast && !NPC.AnyNPCs(ModContent.NPCType<Polterghast>()) && (npc.type == ModContent.NPCType<PhantomSpirit>() || npc.type == ModContent.NPCType<PhantomSpiritS>() || npc.type == ModContent.NPCType<PhantomSpiritM>() ||
            npc.type == ModContent.NPCType<PhantomSpiritL>()))
        {
            if (CalamityMod.CalamityMod.ghostKillCount == 10)
            {
                SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, Main.player[0].Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.UnitX) * (Main.rand.Next(1200, 1500)));
            }
            else if (CalamityMod.CalamityMod.ghostKillCount == 20)
            {
                SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, Main.player[0].Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.UnitX) * (Main.rand.Next(800, 1000)));
            }
        }
    }

    public static int GetWormHead(int whoAmI)
    {
        NPC npc = Main.npc[whoAmI];
        if (npc == null || !npc.active)
            return -1;

        if (!WindfallLists.WormIDs.Contains(npc.type) || WindfallLists.WormHeadIDs.Contains(npc.type))
            return whoAmI;

        if (npc.type <= NPCID.BloodEelTail)
            npc = Main.npc[(int)npc.ai[1]];
        else
            npc = Main.npc[(int)npc.ai[2]];

        if (WindfallLists.WormHeadIDs.Contains(npc.type))
            return npc.whoAmI;
        else
            return GetWormHead(npc.whoAmI);
    }
}

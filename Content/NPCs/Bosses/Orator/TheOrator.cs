using CalamityMod.World;
using Terraria.Graphics.Shaders;
using Windfall.Common.Systems;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Items.Lore;
using Terraria.GameContent.ItemDropRules;
using Windfall.Common.Utils;
using CalamityMod.Buffs.StatBuffs;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Items.Weapons.Summon;
using Terraria.GameContent.Bestiary;
using Terraria;
using Windfall.Content.Skies;
using Terraria.Graphics.Effects;
using CalamityMod.Items.LoreItems;
using Windfall.Content.UI.BossBars;
using Terraria.ID;

namespace Windfall.Content.NPCs.Bosses.Orator
{
    [AutoloadBossHead]
    public class TheOrator : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/Bosses/TheOrator_Boss";
        public override string BossHeadTexture => "Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head";
        public static readonly SoundStyle Dash = new("CalamityMod/Sounds/Item/DeadSunShot") { PitchVariance = 0.35f, Volume = 0.5f };
        public static readonly SoundStyle DashWarn = new("CalamityMod/Sounds/Item/DeadSunRicochet") { Volume = 0.5f };
        public static readonly SoundStyle HurtSound = new("CalamityMod/Sounds/NPCHit/ShieldHit", 3);
        private static int MonsterDamage;
        internal static int GlobDamage;
        internal static int BoltDamage;
        private static int DashDelay;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(TheOrator)}")),
        ]);
        }
        public override void SetDefaults()
        {
            NPC.boss = true;
            NPC.width = NPC.height = 44;
            NPC.Size = new Vector2(150, 150);
            //Values gotten from Lunatic Cultist. Subject to change.
            NPC.DR_NERD(0.10f);
            NPC.LifeMaxNERB(Main.masterMode ? 330000 : Main.expertMode ? 240000 : 180000, 300000);
            NPC.BossBar = ModContent.GetInstance<OratorBossBar>();
            NPC.npcSlots = 5f;
            NPC.defense = 50;
            NPC.HitSound = HurtSound;
            NPC.DeathSound = SoundID.Zombie105;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.Calamity().canBreakPlayerDefense = true;

            noSpawnsEscape = true;
            MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 180);
            GlobDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 240 : CalamityWorld.death ? 200 : CalamityWorld.revenge ? 170 : Main.expertMode ? 125 : 100);
            BoltDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 205 : CalamityWorld.death ? 172 : CalamityWorld.revenge ? 160 : Main.expertMode ? 138 : 90);
            DashDelay = CalamityWorld.death ? 20 : CalamityWorld.revenge ? 25 : 30;
        }
        private int hitTimer = 0;
        private bool scytheSpin = false;
        private bool scytheSlice = false;

        public static bool noSpawnsEscape = true;
        public override void OnSpawn(IEntitySource source)
        {
            SkyManager.Instance.Activate("Windfall:Orator", args: []);
            target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];

            //AIState = States.Testing;
        }
        internal enum States
        {
            Spawning,
            DarkMonster,
            DarkSpawn,
            DarkBarrage,
            DarkSlice,
            DarkStorm,
            DarkCollision,

            DarkOrbit,
            DarkFlood,
            DarkCrush,
            DarkFlight,
            DarkTides,
            DarkEmbrace,

            PhaseChange,
            Defeat,
            Testing
        }
        internal States AIState
        {
            get => (States)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        internal int aiCounter
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        float attackCounter = 0;
        bool dashing = false;
        private int attackCycles = 0;
        Vector2 VectorToTarget = Vector2.Zero;
        public Player target = null;
        public override bool PreAI()
        {
            foreach (Player p in Main.player.Where(p => p != null && p.active && !p.dead))
            {
                p.AddBuff(ModContent.BuffType<BossEffects>(), 2);
            }
            return true;
        }
        public override void AI()
        {
            if (target == null || !target.active || target.dead)
            {
                target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                if (target == null || target.active == false || target.dead)
                    NPC.active = false;
            }
            if (!dashing)
            {
                if (NPC.Center.X < target.Center.X)
                    NPC.direction = 1;
                else
                    NPC.direction = -1;
            }
            NPC.spriteDirection = NPC.direction;
            //Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
            if (NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                NPC.dontTakeDamage = true;
            else
            {
                NPC.dontTakeDamage = false;
                if (NPC.life / (float)NPC.lifeMax <= 0.1f || AIState == States.PhaseChange)
                    NPC.DR_NERD(1f);
                else
                    NPC.DR_NERD(0.1f);
            }
            if (AIState >= States.DarkOrbit && AIState < States.PhaseChange)
                target.Calamity().infiniteFlight = true;
            switch (AIState)
            {
                //Phase 1               
                case States.DarkMonster:
                    if (aiCounter == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 40, ModContent.ProjectileType<DarkMonster>(), MonsterDamage, 0f);
                    if (aiCounter < 1200)
                    {
                        #region Movement 
                        NPC.velocity = target.velocity;
                        VectorToTarget = target.Center - NPC.Center;

                        float rotationRate = 0.03f;
                        if (target.velocity.Length() > 0.1f)
                            rotationRate *= Math.Abs(Utilities.AngleBetween(target.velocity, VectorToTarget) - Pi);
                        else
                        {
                            if (target.direction == 1)
                                rotationRate *= Math.Abs(Utilities.AngleBetween(VectorToTarget, 0f.ToRotationVector2()) - Pi);
                            else
                                rotationRate *= Math.Abs(Utilities.AngleBetween(VectorToTarget, Pi.ToRotationVector2()) - Pi);
                        }

                        float circleDistance = 450;
                        float currentDistance = VectorToTarget.Length();
                        int approachRate = 10;
                        if (currentDistance > circleDistance + approachRate)
                            currentDistance -= approachRate;
                        else if (currentDistance < circleDistance - approachRate)
                            currentDistance += approachRate;

                        if (target.velocity != Vector2.Zero)
                            NPC.Center = target.Center - VectorToTarget.SafeNormalize(Vector2.Zero).RotateTowards(target.velocity.ToRotation() + Pi, rotationRate) * currentDistance;
                        else
                        {
                            if (target.direction == 1)
                                NPC.Center = target.Center - VectorToTarget.SafeNormalize(Vector2.Zero).RotateTowards(Pi, rotationRate) * currentDistance;
                            else
                                NPC.Center = target.Center - VectorToTarget.SafeNormalize(Vector2.Zero).RotateTowards(0f, rotationRate) * currentDistance;
                        }
                        #endregion
                        #region Projectiles
                        Vector2 TargetVector = target.Center - NPC.Center;
                        if (aiCounter % (CalamityWorld.death ? 30 : CalamityWorld.revenge ? 34 : Main.expertMode ? 36 : 40) == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                            for (int i = 0; i <= 20; i++)
                            {
                                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(10f, 10f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                            }
                            int globCount = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 4 : 3;
                            for (int i = 0; i < globCount; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, TargetVector.SafeNormalize(Vector2.UnitX).RotatedBy(TwoPi / globCount * i) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                            }
                        }
                        #endregion
                    }
                    if (aiCounter == 900)
                    {
                        aiCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                        {
                            AIState = States.PhaseChange;
                            attackCounter = 0;
                        }
                        else
                            AIState = States.DarkSpawn;
                        if (FindFirstProjectile(ModContent.ProjectileType<DarkMonster>()) != -1)
                        {
                            Main.projectile[FindFirstProjectile(ModContent.ProjectileType<DarkMonster>())].ai[0] = 1;
                            Main.projectile[FindFirstProjectile(ModContent.ProjectileType<DarkMonster>())].ai[1] = 0;
                        }
                        return;
                    }
                    break;
                case States.DarkSpawn:
                    #region Movement
                    Vector2 homeInV = target.Center + Vector2.UnitY * -300;
                    
                    NPC.velocity = (homeInV - NPC.Center).SafeNormalize(Vector2.Zero) * ((homeInV - NPC.Center).Length() / 10f);
                    #endregion                                       
                    NPC.damage = 0;
                    const int EndTime = 1500;
                    int SpawnCount = CalamityWorld.death ? 3 : CalamityWorld.revenge || Main.expertMode ? 2 : 1;
                    if (!CalamityWorld.death && Main.npc.Where(n => n.type == ModContent.NPCType<ShadowHand>() && n.active).Count() <= SpawnCount + 1)
                        SpawnCount++;
                    if (NPC.AnyNPCs(ModContent.NPCType<ShadowHand>()))
                    {
                        if (aiCounter > EndTime)
                        {
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<ShadowHand>() && n.active))
                            {
                                if (spawn.ModNPC is ShadowHand darkSpawn)
                                    darkSpawn.CurrentAI = ShadowHand.AIState.Sacrifice;
                            }
                            attackCounter = -1;
                        }
                        else if (aiCounter > 150)
                        {
                            attackCounter = 0;
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<ShadowHand>() && n.active))
                            {
                                if (spawn.ModNPC is ShadowHand darkSpawn && darkSpawn.CurrentAI != ShadowHand.AIState.OnBoss)
                                    attackCounter++;
                            }
                            if (attackCounter < SpawnCount && Main.npc.Any(n => n.type == ModContent.NPCType<ShadowHand>() && n.active && n.ai[0] == (int)ShadowHand.AIState.OnBoss))
                            {
                                if (attackCounter >= SpawnCount)
                                    break;
                                NPC spawn = Main.npc.Last(n => n.type == ModContent.NPCType<ShadowHand>() && n.active && n.ai[0] == (int)ShadowHand.AIState.OnBoss);
                                if (spawn.ModNPC is ShadowHand darkSpawn)
                                {
                                    Vector2 ToTarget = target.Center - spawn.Center;
                                    spawn.velocity = ToTarget.SafeNormalize(Vector2.Zero) * -10;
                                    darkSpawn.CurrentAI = ShadowHand.AIState.Dashing;
                                    spawn.rotation = ToTarget.ToRotation();
                                    attackCounter++;
                                }
                            }
                        }
                    }
                    else if (aiCounter > 150 && aiCounter < EndTime)
                        aiCounter = EndTime;
                    else if (aiCounter >= EndTime + 90)
                    {
                        aiCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                            AIState = States.PhaseChange;
                        else
                        {
                            NPC.DR_NERD(0.1f);
                            if (attackCounter != -1)
                                AIState = States.DarkCollision;
                            else
                            {
                                aiCounter = -30;
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                                AIState = States.DarkSlice;
                            }
                        }
                        attackCounter = 0;
                        return;
                    }
                    break;
                case States.DarkBarrage:
                    if (aiCounter <= 1100)
                    {
                        #region Movement
                        Vector2 goal = target.Center + Vector2.UnitY * -300;
                        NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 10f);
                        #endregion

                        #region Scrapped Projectiles
                        /*
                        if (aiCounter > 0)
                        {                           
                            if (aiCounter % 45 == 0)
                            {
                                if (aiCounter % 90 == 0)
                                {
                                    scytheSpin = true;
                                    int radialCounter = CalamityWorld.death ? 12 : CalamityWorld.revenge ? 10 : 8;
                                    for (int i = 0; i < radialCounter; i++)
                                    {
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {
                                            Vector2 rotationVector = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / radialCounter * i);
                                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + NPC.velocity + rotationVector * 40, rotationVector * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, 5);
                                        }
                                    }
                                }
                                else
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 10f }, NPC.Center);
                                    int radialCounter = CalamityWorld.death ? 30 : CalamityWorld.revenge ? 28 : 24;
                                    for (int i = 0; i < radialCounter; i++)
                                    {
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + NPC.velocity, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / radialCounter * i) * 6, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, Main.rand.NextFloat(1f, 2f));
                                    }
                                }
                            }                            
                        }
                        */
                        #endregion
                    }
                    if (aiCounter > 1100 || !NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                    {
                        #region Attack Transition
                        if(aiCounter < 1240)
                            target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        Vector2 homeIn = target.Center - NPC.Center;
                        float targetDistance = homeIn.Length();
                        homeIn.Normalize();
                        if (targetDistance > 300f)
                            NPC.velocity = (NPC.velocity * 40f + homeIn * 16f) / 41f;
                        else
                        {
                            if (targetDistance < 250f)
                                NPC.velocity = (NPC.velocity * 40f + homeIn * -16f) / 41f;
                            else
                                NPC.velocity *= 0.975f;
                        }

                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 150f, Main.rand.NextVector2Circular(5f, 5f), 1.5f * ((aiCounter - 1100) / 2));
                        if (aiCounter > 1300 || !NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                        {
                            aiCounter = 0;
                            if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                            {
                                AIState = States.PhaseChange;
                                attackCounter = 0;
                            }
                            else
                            {
                                AIState = States.DarkMonster;
                                Particle pulse = new DirectionalPulseRing(NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8f, new(117, 255, 159), new Vector2(0.5f, 1f), (target.Center - NPC.Center).ToRotation(), 0f, 3f, 32);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                        #endregion
                    }
                    break;
                case States.DarkSlice:
                    if(aiCounter == -31)
                    {
                        attackCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (++attackCycles == (CalamityWorld.death ? 3 : CalamityWorld.revenge ? 2 : 1))
                        {
                            attackCycles = 0;
                            aiCounter = 0;
                            dashing = false;
                            if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                            {
                                AIState = States.PhaseChange;
                            }
                            else
                            {
                                NPC.damage = 0;
                                AIState = States.DarkStorm;
                            }
                            break;
                        }
                    }
                    if(aiCounter < -30)
                    {
                        NPC.damage = 0;
                        Vector2 vector = target.Center - NPC.Center;
                        float dist = vector.Length();
                        vector.Normalize();
                        if (dist > 500)
                            NPC.velocity = (NPC.velocity * 40f + vector * 16f) / 41f;
                        else
                        {
                            if (dist < 450)
                                NPC.velocity = (NPC.velocity * 40f + vector * -16f) / 41f;
                            else
                                NPC.velocity *= 0.975f;
                        }
                    }
                    int SliceDashCount = CalamityWorld.revenge ? 3 : Main.expertMode ? 2 : 1;
                    if (aiCounter < 0 && aiCounter >= -30)
                    {
                        if(aiCounter == -30)
                            SoundEngine.PlaySound(DashWarn);
                        if (CalamityWorld.death || CalamityWorld.revenge && aiCounter < -10 || Main.expertMode && aiCounter < -20)                        
                            VectorToTarget = target.Center - NPC.Center;                                                        
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, DashDelay, aiCounter + DashDelay, true);
                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = NPC.DirectionTo(target.Center) * -reelBackSpeed;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                    }
                    if (aiCounter == 0)
                    {                      
                        VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -75;
                        scytheSlice = true;
                        dashing = true;
                        //values gotten from Astrum Deus' contact damage. Subject to change.
                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);

                        Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                    if (aiCounter >= 0)
                    {
                        NPC.velocity = VectorToTarget;
                        VectorToTarget *= 0.93f;

                        #region Dash Projectiles
                        if (NPC.velocity.Length() > 15f)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(128f, 128f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                                dust.noGravity = true;
                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                            }
                            if (aiCounter % 10 == 0 && Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            else if (aiCounter % 5 == 0 && Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        }
                        
                        #endregion

                        if (VectorToTarget.Length() <= 4)
                        {
                            scytheSpin = true;
                            int radialCounter = CalamityWorld.death ? 12 : CalamityWorld.revenge ? 10 : 8;
                            for (int i = 0; i < radialCounter; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 rotationVector = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / radialCounter * i);
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + NPC.velocity + rotationVector * 124, rotationVector * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -10);
                                }
                            }
                            if (++attackCounter == SliceDashCount)
                                aiCounter = -200;
                            else
                                aiCounter = -30;
                            return;
                        }
                    }
                    break;
                case States.DarkStorm:
                    int AttackFrequency = CalamityWorld.death ? 10 : CalamityWorld.revenge ? 12 : Main.expertMode ? 14 : 16;
                    #region Movement
                    Vector2 goalPosition = target.Center + Vector2.UnitY * -300;
                    NPC.velocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPosition - NPC.Center).Length() / 10f);
                    #endregion
                    #region Projectiles
                    if (aiCounter > 60 && NPC.Center.Y < target.Center.Y - 50)
                    {
                        Projectile proj;
                        if (Main.netMode != NetmodeID.MultiplayerClient && NPC.AnyNPCs(ModContent.NPCType<OratorHand>()) && aiCounter % 10 == 0)
                        {
                            //Anti-Cheesers
                            NPC hand = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<OratorHand>())]; ;
                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), hand.Center + new Vector2(0, -32), new Vector2(Main.rand.NextFloat(0f, 2f), -12), ModContent.ProjectileType<DarkGlob>(), GlobDamage * 3, 0f, -1, 1, 1.5f);
                            proj.Calamity().DealsDefenseDamage = true;

                            hand = Main.npc.Last(n => n != null && n.active && n.type == ModContent.NPCType<OratorHand>());
                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), hand.Center + new Vector2(-64, -32), new Vector2(Main.rand.NextFloat(-2f, 0f), -12), ModContent.ProjectileType<DarkGlob>(), GlobDamage * 3, 0f, -1, 1, 1.5f);
                            proj.Calamity().DealsDefenseDamage = true;
                        }
                        if (aiCounter % AttackFrequency == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                            for (int i = 0; i < 10; i++)
                            {
                                EmpyreanMetaball.SpawnDefaultParticle(new(NPC.Center.X, NPC.Center.Y - 50), Main.rand.NextVector2Unit(0, -Pi) * Main.rand.NextFloat(0f, 15f), 25 * Main.rand.NextFloat(1f, 2f));
                            }
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                proj.Calamity().DealsDefenseDamage = true;
                                proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(-10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                proj.Calamity().DealsDefenseDamage = true;
                                if (CalamityWorld.revenge && aiCounter % 20 == 0)
                                {
                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(0, -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                    proj.Calamity().DealsDefenseDamage = true;
                                }
                            }
                        }

                    }
                    #endregion
                    #region Attack Shift
                    if (aiCounter > 720)
                    {
                        attackCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                        {
                            AIState = States.PhaseChange;
                            aiCounter = 0;
                        }
                        else
                        {
                            NPC.velocity = Vector2.UnitY * -10;
                            aiCounter = -60;
                            AIState = States.DarkBarrage;
                        }
                    }
                    #endregion
                    break;
                case States.DarkCollision:
                    #region Movement
                    Vector2 homeInVec = target.Center - NPC.Center;
                    float distance = homeInVec.Length();
                    homeInVec.Normalize();
                    if (distance > 350)
                        NPC.velocity = (NPC.velocity * 40f + homeInVec * 18f) / 41f;
                    else
                    {
                        if (distance < 300)
                            NPC.velocity = (NPC.velocity * 40f + homeInVec * -18f) / 41f;
                        else
                            NPC.velocity *= 0.975f;
                    }
                    #endregion

                    if (aiCounter % (80 + DarkCoalescence.fireDelay) == 0 && aiCounter < 700)
                    {
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            if (Main.rand.NextBool())
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X + 500, target.Center.Y), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, -1);
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X - 500, target.Center.Y), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 1);
                            }
                            else
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X, target.Center.Y + 500), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 0, -1);
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X, target.Center.Y - 500), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 0, 1);
                            }
                    }

                    if (aiCounter >= 850)
                    {
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                        {
                            AIState = States.PhaseChange;
                            attackCounter = 0;
                            aiCounter = 0;
                        }
                        else
                        {
                            aiCounter = -30;
                            SoundEngine.PlaySound(DashWarn);
                            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                            AIState = States.DarkSlice;
                        }
                    }
                    break;
                //Phase 2
                case States.DarkOrbit:
                    float velo = 0f;
                    int OrbitDashCount = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 3 : 2;
                    float OrbitRate = CalamityWorld.death ? 0.03f : CalamityWorld.revenge ? 0.025f : 0.02f;
                    Projectile border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    if (aiCounter >= 0)
                    {
                        if (aiCounter == 0)
                        {
                            NPC.position.X = border.position.X;
                            NPC.position.Y = border.position.Y - 700;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[3] = Main.rand.Next(700, 800);
                            NPC.netUpdate = true;
                            attackCycles++;
                        }
                        if (aiCounter < NPC.ai[3] - 60)
                        {
                            if (aiCounter < NPC.ai[3] - 240)
                            {
                                VectorToTarget = target.Center - NPC.Center;
                                NPC.Center = border.Center + new Vector2(0, -700).RotatedBy(aiCounter * OrbitRate);
                                if (aiCounter % 10 == 0 && aiCounter > 30 && Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f);
                                if (Main.expertMode && aiCounter % (CalamityWorld.revenge ? 90 : 120) == 0 && aiCounter > 30)
                                {
                                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                                    SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);
                                    for (int i = 0; i < 20; i++)
                                    {
                                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(Pi / 6) * Main.rand.NextFloat(0f, 25f), 30 * Main.rand.NextFloat(1f, 2f));
                                    }
                                    for (int i = 1; i < 8; i++)
                                    {
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {
                                            Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * (3.5f * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                            proj.timeLeft = (int)(proj.timeLeft / 1.5f);

                                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * (3.5f * -i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                            proj.timeLeft = (int)(proj.timeLeft / 1.5f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Vector2 vector = target.Center - NPC.Center;
                                float dist = vector.Length();
                                vector.Normalize();
                                if (dist > 500)
                                    NPC.velocity = (NPC.velocity * 40f + vector * 16f) / 41f;
                                else
                                {
                                    if (dist < 450)
                                        NPC.velocity = (NPC.velocity * 40f + vector * -16f) / 41f;
                                    else
                                        NPC.velocity *= 0.975f;
                                }
                            }
                        }
                        else if (aiCounter < NPC.ai[3])
                        {
                            if (aiCounter == NPC.ai[3] - DashDelay)
                            {
                                target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                                if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                                {
                                    AIState = States.Defeat;
                                    attackCounter = 0;
                                    aiCounter = 0;
                                }
                                else
                                {
                                    dashing = true;
                                    SoundEngine.PlaySound(DashWarn);
                                }
                            }
                            if (aiCounter - NPC.ai[3] < (CalamityWorld.death ? -20 : -30))
                                VectorToTarget = target.Center - NPC.Center;
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, DashDelay, (aiCounter - NPC.ai[3]) + DashDelay, true);
                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = NPC.DirectionTo(target.Center) * -reelBackSpeed;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                        }
                        else
                        {
                            if (aiCounter == NPC.ai[3])
                            {
                                VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -75;
                                NPC.ai[2] = -1;
                                scytheSlice = true;
                                //values gotten from Astrum Deus' contact damage. Subject to change.
                                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                            NPC.velocity = VectorToTarget;
                            VectorToTarget *= 0.95f;
                            if (border.ModProjectile is OratorBorder oraBorder && Vector2.Distance(border.Center, NPC.Center) >= oraBorder.Radius)
                            {
                                if (NPC.ai[2] == 0)
                                {
                                    VectorToTarget /= -2;
                                    NPC.velocity = VectorToTarget;
                                }
                            }
                            else
                                NPC.ai[2] = 0;

                            #region Dash Projectiles
                            if (NPC.velocity.Length() > 2f)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(128f, 128f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                    dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }
                            }
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (aiCounter % (CalamityWorld.revenge ? 8 : 10) == 0 && Main.expertMode)
                                {
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                                }
                                else if (aiCounter % (CalamityWorld.revenge ? 4 : 5) == 0 && Main.expertMode)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            }
                            #endregion
                            if (VectorToTarget.Length() <= 10)
                            {
                                NPC.damage = 0;
                                if (attackCounter++ == OrbitDashCount - 1)
                                {
                                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                                    dashing = false;
                                    aiCounter = 0;
                                    if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                                    {
                                        AIState = States.Defeat;
                                        attackCounter = 0;
                                    }
                                    else
                                    {
                                        AIState = States.DarkEmbrace;
                                        aiCounter = -1;
                                    }
                                }
                                else
                                    aiCounter = (int)NPC.ai[3] - DashDelay;
                                return;
                            }
                        }
                    }
                    else
                    {
                        homeInV = border.Center + Vector2.UnitY * -700 - NPC.Center;
                        velo = 60;
                        homeInV.Normalize();
                        NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                    }
                    break;
                case States.DarkCrush:                    
                    border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    OratorBorder oratorBorder = border.ModProjectile as OratorBorder;
                    #region Movement
                    if (aiCounter < 1620)
                        homeInV = border.Center - (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 64 - NPC.Center;
                    else
                        homeInV = border.Center - NPC.Center;
                    velo = 60;
                    homeInV.Normalize();
                    NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                    #endregion
                    if (aiCounter < 1500 && oratorBorder.trueScale > 1.5f)
                        oratorBorder.trueScale -= 0.001f;
                    if (aiCounter % 5 == 0 && aiCounter > 120)
                    {
                        if (aiCounter < 1620)
                        {
                            if (Main.expertMode && aiCounter % 120 == 0)
                            {
                                scytheSpin = true;
                                for (int i = 0; i < 6; i++)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / 6 * i) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                                }
                            }
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                        }
                        else if (aiCounter >= 1680 && aiCounter % 45 == 0)
                        {
                            scytheSpin = true;
                            Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                            int projCount = 8;
                            for (int i = 0; i < projCount; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, dir.RotatedBy(TwoPi / projCount * i) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            }
                        }
                    }
                    if (aiCounter >= 1620)
                    {
                        oratorBorder.trueScale += 0.003f;
                        if (oratorBorder.trueScale >= 3f)
                        {
                            oratorBorder.trueScale = 3f;
                            aiCounter = 0;
                            target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                            if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                            {
                                AIState = States.Defeat;
                                attackCounter = 0;
                            }
                            else
                            {
                                AIState = States.DarkFlood;
                                aiCounter = -30;
                                attackCounter = 0;
                            }
                        }
                    }
                    break;
                case States.DarkFlight:
                    Vector2 homeInVector;
                    float targetDist;
                    border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    oratorBorder = border.ModProjectile as OratorBorder;
                    #region Movement
                    if (aiCounter < 1000)
                    {
                        homeInVector = target.Center - NPC.Center;
                        targetDist = homeInVector.Length();
                        homeInVector.Normalize();
                        if (targetDist > 300f)
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * 12f) / 41f;
                        else
                        {
                            if (targetDist < 250f)
                                NPC.velocity = (NPC.velocity * 40f + homeInVector * -12f) / 41f;
                            else
                                NPC.velocity *= 0.975f;
                        }
                    }
                    else
                    {
                        OrbitDashCount = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 4 : 3;
                        OrbitRate = CalamityWorld.death ? 0.04f : CalamityWorld.revenge ? 0.035f : 0.03f;

                        if (aiCounter < 1060)
                        {
                            Vector2 goal = border.Center + (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * -900;
                            NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 30f);                            
                        }
                        else
                        {
                            if (aiCounter == 1060)
                            {
                                NPC.velocity = Vector2.Zero;
                                NPC.ai[3] = Main.rand.Next(180, 300);
                                NPC.netUpdate = true;
                            }
                            if (aiCounter < 1060 + NPC.ai[3])
                            {
                                if (aiCounter == 1060)
                                {
                                    VectorToTarget = (border.Center - NPC.Center).SafeNormalize(Vector2.UnitY * -1) * -850;
                                    NPC.Center = border.Center + VectorToTarget;
                                }
                                NPC.Center = border.Center + VectorToTarget.RotatedBy(aiCounter * OrbitRate);
                                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + ((border.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(PiOver2) * Main.rand.NextFloat(-48, 48)), (border.Center - NPC.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 14f), Main.rand.NextFloat(100f, 120f));
                            }
                            else
                            {
                                if (aiCounter == 1060 + NPC.ai[3])
                                {
                                    SoundEngine.PlaySound(DashWarn);
                                    NPC.velocity = Vector2.Zero;
                                    VectorToTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                }
                                else if (aiCounter >= 1060 + NPC.ai[3] + 30)
                                {
                                    if (aiCounter == 1060 + NPC.ai[3] + 30)
                                    {
                                        int projectileCount = CalamityWorld.death ? 24 : CalamityWorld.revenge ? 20 : 16;
                                        for (int i = 0; i < projectileCount; i++)
                                        {
                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                            {
                                                Vector2 spawnPos = NPC.Center + (VectorToTarget.RotatedBy(PiOver2) * 180f * i) - (VectorToTarget * (i * 80));
                                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPos, VectorToTarget * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);

                                                spawnPos = NPC.Center + (VectorToTarget.RotatedBy(-PiOver2) * 180f * i) - (VectorToTarget * (i * 80));
                                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPos, VectorToTarget * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                                            }
                                        }
                                        if(attackCounter + 1 >= OrbitDashCount)
                                            NPC.velocity = VectorToTarget * 70;
                                        else
                                            NPC.velocity = VectorToTarget * 90;
                                        scytheSlice = true;
                                        dashing = true;
                                        //values gotten from Astrum Deus' contact damage. Subject to change.
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);

                                        Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                                        GeneralParticleHandler.SpawnParticle(pulse);
                                    }
                                    else
                                    {
                                        NPC.velocity *= 0.96f;

                                        for (int i = 0; i < 2; i++)
                                        {
                                            int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                            Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(128f, 128f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                            dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                                            dust.noGravity = true;
                                            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                        }

                                        if (NPC.velocity.LengthSquared() < 400)
                                        {
                                            dashing = false;
                                            NPC.damage = 0;
                                            NPC.velocity = Vector2.Zero;
                                            if (++attackCounter < OrbitDashCount)
                                            {
                                                int projectileCount = CalamityWorld.death ? 24 : CalamityWorld.revenge ? 20 : 16;
                                                for (int i = 0; i < projectileCount; i++)
                                                {
                                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                                    {
                                                        Vector2 ToTarget = (border.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                                        Vector2 spawnPos = NPC.Center + (ToTarget.RotatedBy(PiOver2) * 180f * i) - (ToTarget * (i * 80));
                                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPos, ToTarget * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);

                                                        spawnPos = NPC.Center + (ToTarget.RotatedBy(-PiOver2) * 180f * i) - (ToTarget * (i * 80));
                                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPos, ToTarget * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                                                    }
                                                }

                                                aiCounter = 1060;
                                                return;
                                            }
                                            else
                                            {
                                                aiCounter = 0;
                                                target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                                                if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                                                {
                                                    AIState = States.Defeat;
                                                    attackCounter = 0;
                                                }
                                                else
                                                {
                                                    AIState = States.DarkTides;
                                                    aiCounter = -1;
                                                    attackCounter = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region Ambiant Projectiles
                    if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter > 0 && (aiCounter > 1000 && aiCounter % 20 == 0 || aiCounter <= 1000 && aiCounter % 10 == 0))
                    {
                        Vector2 spawnPosition = border.Center + (Vector2.UnitX * (oratorBorder.Radius + 200)).RotatedBy(Main.rand.NextFloat(TwoPi));
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPosition, (border.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.75f, 0.75f)) * Main.rand.NextFloat(4f, 6f), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                    }
                    #endregion

                    break;
                case States.DarkTides:
                    int attackFrequency = CalamityWorld.death ? 500 : CalamityWorld.revenge ? 600 : 700;
                    int attackGap = CalamityWorld.death ? 275 : CalamityWorld.revenge ? 300 : 335;
                    if (aiCounter < attackFrequency * 3 - 20)
                    {
                        border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                        if (aiCounter % attackFrequency == 0)
                        {
                            attackCounter = Main.rand.NextFloatDirection();
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile p = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center, attackCounter.ToRotationVector2(), ModContent.ProjectileType<DarkTide>(), 0, 0f, ai0: 60, ai1: 1500, ai2: 6);
                                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 10f }, p.Center);
                            }
                        }
                        else if (aiCounter % attackFrequency == attackGap)
                        {
                            Vector2 initialPos = border.Center + attackCounter.ToRotationVector2() * 175;
                            if (Main.expertMode)
                            {
                                for (int i = 0; i < (CalamityWorld.death ? 25 : CalamityWorld.revenge ? 20 : 10); i++)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
                                        Vector2 myPos = initialPos + attackCounter.ToRotationVector2() * Main.rand.NextFloat(-500, 0) + (attackCounter + PiOver2).ToRotationVector2() * Main.rand.NextFloat(-800, 800);
                                        Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), myPos, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.5f, 1.5f));
                                        proj.timeLeft = attackGap * 2;
                                    }
                                }
                            }
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile p = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center, -attackCounter.ToRotationVector2(), ModContent.ProjectileType<DarkTide>(), 0, 0f, ai0: 60, ai1: 1500, ai2: 6);
                                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 10f }, p.Center);
                            }
                        }

                        #region Movement
                        Vector2 targetPosition = border.Center + attackCounter.ToRotationVector2() * (aiCounter % attackFrequency < attackGap ? 600 : -600);
                        homeInV = targetPosition - NPC.Center;
                        velo = 60;
                        homeInV.Normalize();
                        NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                        #endregion
                    }
                    else
                    {
                        foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkGlob>()))
                            proj.timeLeft = 45;
                        aiCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                        {
                            AIState = States.Defeat;
                            attackCounter = 0;
                        }
                        else
                        {
                            AIState = States.DarkCrush;
                            aiCounter = -60;
                            attackCounter = 0;
                        }
                    }
                    break;
                case States.DarkEmbrace:
                    border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    #region Movement
                    homeInV = border.Center + Vector2.UnitY * -600 - NPC.Center;
                    velo = 60;
                    homeInV.Normalize();
                    NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                    #endregion
                    int attackDuration = 2000;
                    int tideOut = 600;
                    if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter % attackDuration == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy);
                        int tightness = CalamityWorld.death ? 1050 : CalamityWorld.revenge ? 1025 : 1000;
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), border.Center, Vector2.UnitX, ModContent.ProjectileType<DarkTide>(), 0, 0f, ai0: tideOut, ai1: tightness, ai2: 8);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), border.Center, Vector2.UnitX * -1, ModContent.ProjectileType<DarkTide>(), 0, 0f, ai0: tideOut, ai1: tightness, ai2: 8);
                    }
                    else if ((aiCounter > 150 && aiCounter < tideOut) || (aiCounter > attackDuration + 120 && aiCounter < attackDuration + tideOut))
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter % 20 == 0)
                        {
                            bool left = Main.rand.NextBool();
                            Vector2 spawnPosition = border.Center + new Vector2(left ? -275 : 275, Main.rand.NextFloat(-700f, 700f));
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), spawnPosition, (Vector2.UnitX * (left ? 1 : -1)).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), ModContent.ProjectileType<EmpyreanThorn>(), GlobDamage, 0f);
                        }
                    }
                    else if (aiCounter >= tideOut + 120 && aiCounter < attackDuration - 100)
                    {
                        if (aiCounter > tideOut + 260)
                        {
                            /*
                            if(aiCounter % 60 == 0)
                                for (int i = 0; i < 8; i++)
                                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), NPC.Center + Main.rand.NextVector2Circular(64f, 64f), (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                            */
                            if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter % 8 == 0)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + (Vector2.UnitX * ((float)(Math.Sin(aiCounter / 20f) * 100f) - 16)), new Vector2(0, -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.75f);
                        }
                    }
                    else if (aiCounter == attackDuration + tideOut + 120)
                    {
                        aiCounter = -30;
                        attackCounter = 0;
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                            AIState = States.Defeat;
                        else
                            AIState = States.DarkFlight;
                    }
                    if (aiCounter >= tideOut && aiCounter < attackDuration && aiCounter % 60 == 0) //Large orb columns (must be fired earlier and for a bit longer than what's otherwise allowed)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit);
                        if (aiCounter % 120 == 0) //top
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center + new Vector2(200 + 250 * i - 80, -900), Vector2.UnitY * 4, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 2f);
                                    proj.timeLeft += 180;
                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center + new Vector2(-200 - 250 * i - 80, -900), Vector2.UnitY * 4, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 2f);
                                    proj.timeLeft += 180;
                                }
                            }
                        }
                        else //bottom
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center + new Vector2(325 + 250 * i - 80, 900), Vector2.UnitY * -4, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 2f);
                                    proj.timeLeft += 180;
                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), border.Center + new Vector2(-325 - 250 * i - 80, 900), Vector2.UnitY * -4, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 2f);
                                    proj.timeLeft += 180;
                                }
                            }
                        }
                    }
                    break;
                case States.DarkFlood:
                    border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    if (aiCounter < 750)
                    {
                        #region Movement
                        homeInV = border.Center + Vector2.UnitY * -600 - NPC.Center;
                        velo = 60;
                        homeInV.Normalize();
                        NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                        #endregion
                        if (aiCounter >= 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter == 0)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), border.Center, Vector2.UnitY * -1, ModContent.ProjectileType<DarkTide>(), 0, 0f, ai0: 2000, ai1: 1360, ai2: 2);
                                NPC.ai[3] = -1;
                            }
                            AttackFrequency = CalamityWorld.death ? 8 : CalamityWorld.revenge ? 10 : Main.expertMode ? 12 : 14;
                            if (Main.netMode != NetmodeID.MultiplayerClient && aiCounter % AttackFrequency == 0)
                            {
                                Projectile proj;
                                SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                for (int i = 0; i < 10; i++)
                                {
                                    EmpyreanMetaball.SpawnDefaultParticle(new(NPC.Center.X, NPC.Center.Y - 50), Main.rand.NextVector2Unit(0, -Pi) * Main.rand.NextFloat(0f, 15f), 25 * Main.rand.NextFloat(1f, 2f));
                                }
                                proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(8 * Math.Sin(aiCounter)), -5), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                proj.Calamity().DealsDefenseDamage = true;
                                proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(-8 * Math.Sin(aiCounter)), -5), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                proj.Calamity().DealsDefenseDamage = true;
                                if (CalamityWorld.revenge && aiCounter % 20 == 0)
                                {
                                    //proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(0, -5), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                    //proj.Calamity().DealsDefenseDamage = true;
                                }
                            }
                        }
                    }
                    else if (aiCounter >= 750)
                    {
                        if (attackCounter >= 3)
                        {
                            aiCounter = -30;
                            attackCounter = 0;
                            target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                            if (NPC.life / (float)NPC.lifeMax <= 0.1f)
                                AIState = States.Defeat;
                            else
                                AIState = States.DarkOrbit;
                        }
                        if (aiCounter - 750 < 120)
                        {
                            homeInV = new Vector2(target.Center.X, border.Center.Y - 600) - NPC.Center;
                            velo = 20;
                            homeInV.Normalize();
                            NPC.velocity = (NPC.velocity * velo + homeInV * 18f) / (velo + 1f);
                            if (Math.Abs(NPC.velocity.Y) > 15)
                            {
                                if (NPC.velocity.Y >= 0)
                                    NPC.velocity.Y = 15;
                                else
                                    NPC.velocity.Y = -15;
                            }

                        }
                        else if (aiCounter - 750 < 145)
                        {
                            NPC.ai[3] = -1;
                            if (aiCounter - 750 == 120)
                            {                                
                                dashing = true;
                                SoundEngine.PlaySound(DashWarn);
                            }
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, 25, aiCounter - 870, true);
                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = Vector2.UnitY * -reelBackSpeed;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                        }
                        else
                        {
                            Projectile wall = Main.projectile.First(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkTide>());
                            float wallTop = wall.Center.Y - wall.width / 2 * wall.scale;
                            if (NPC.ai[3] == -1)
                            {
                                if (aiCounter - 750 == 145)
                                {
                                    VectorToTarget = Vector2.UnitY * 75;
                                    scytheSlice = true;
                                    //values gotten from Astrum Deus' contact damage. Subject to change.
                                    NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                    Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), 3 * Pi / 2, 0f, 1f, 24);
                                    GeneralParticleHandler.SpawnParticle(pulse);
                                }
                                NPC.velocity = VectorToTarget;
                                VectorToTarget *= 0.925f;

                                for (int i = 0; i < 2; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(128f, 128f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                    dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }

                                if (NPC.Center.Y > wallTop && NPC.ai[3] == -1)
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, NPC.Center);
                                    dashing = false;
                                    for (int i = 0; i < 32; i++)
                                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + new Vector2(Main.rand.NextFloat(-64, 64), 160), Vector2.UnitY * Main.rand.NextFloat(4f, 18f) * -1, Main.rand.NextFloat(110f, 130f));

                                    for (int i = 0; i < 10; i++)
                                    {
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {
                                            Projectile proj;
                                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + Vector2.UnitY * 64, new Vector2(-4, -2 * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                            proj.Calamity().DealsDefenseDamage = true;
                                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + Vector2.UnitY * 64, new Vector2(4, -2 * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                            proj.Calamity().DealsDefenseDamage = true;
                                        }
                                    }
                                    NPC.ai[3] = aiCounter + 5;
                                }
                            }
                            else if (aiCounter < NPC.ai[3] + 500)
                            {
                                if (aiCounter < NPC.ai[3] + 450)
                                {
                                    NPC.position.Y = wallTop + NPC.height + 32;
                                    if (NPC.Center.X < target.Center.X)
                                        NPC.velocity.X++;
                                    else
                                        NPC.velocity.X--;
                                    if (Math.Abs(NPC.velocity.X) > 8)
                                        if (NPC.velocity.X < 0)
                                            NPC.velocity.X = -8;
                                        else
                                            NPC.velocity.X = 8;
                                    EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + Vector2.UnitX * Main.rand.NextFloat(-48, 48), Vector2.UnitY * Main.rand.NextFloat(8f, 12f) * -1, Main.rand.NextFloat(90f, 110f));

                                    if (Main.expertMode && aiCounter % 60 == 0)
                                    {
                                        int radialCounter = CalamityWorld.death ? 12 : CalamityWorld.revenge ? 10 : 8;
                                        for (int i = 0; i < radialCounter; i++)
                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / radialCounter * i) * 5, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 2, 0.5f);
                                    }
                                }
                                else if (aiCounter >= NPC.ai[3] + 450)
                                {
                                    for (int i = 0; i < 2; i++)
                                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(10f, 14f) * -1, Main.rand.NextFloat(110f, 130f));
                                    if (aiCounter == NPC.ai[3] + 450)
                                    {
                                        dashing = true;
                                        SoundEngine.PlaySound(DashWarn);
                                        NPC.velocity = Vector2.Zero;
                                    }
                                }
                            }
                            else
                            {
                                if (aiCounter == NPC.ai[3] + 500)
                                {
                                    VectorToTarget = Vector2.UnitY * -75;
                                    scytheSlice = true;
                                    //values gotten from Astrum Deus' contact damage. Subject to change.
                                    NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                }
                                NPC.velocity = VectorToTarget;
                                VectorToTarget *= 0.91f;

                                for (int i = 0; i < 2; i++)
                                {
                                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(128f, 128f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                    dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                                    dust.noGravity = true;
                                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                                }

                                if (NPC.position.Y < wallTop && NPC.oldPosition.Y >= wallTop)
                                {
                                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, NPC.Center);

                                    for (int i = 0; i < 32; i++)
                                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(4f, 24f) * -1, Main.rand.NextFloat(110f, 130f));

                                    for (int i = 0; i < 10; i++)
                                    {
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {
                                            Projectile proj;
                                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + Vector2.UnitY * 64, new Vector2(-4, -2 * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                            proj.Calamity().DealsDefenseDamage = true;
                                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center + Vector2.UnitY * 64, new Vector2(4, -2 * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                                            proj.Calamity().DealsDefenseDamage = true;
                                        }
                                    }
                                }
                                else if (NPC.velocity.LengthSquared() <= 64 && NPC.position.Y < wallTop - 300 && NPC.oldPosition.Y < wallTop - 300)
                                {
                                    dashing = false;
                                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                                    attackCounter++;
                                    aiCounter = 750;
                                }
                            }
                        }
                    }
                    break;
                //Animations
                case States.Spawning:
                    DashDelay = CalamityWorld.death ? 20 : CalamityWorld.revenge ? 25 : 30;
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    int height = 300;
                    if (aiCounter > 270)
                        height = 500;
                    homeInVec = target.Center - NPC.Center;
                    targetDist = homeInVec.Length();
                    homeInVec.Normalize();
                    if (targetDist > height)
                        NPC.velocity = (NPC.velocity * 40f + homeInVec * 16f) / 41f;
                    else
                    {
                        if (targetDist < height - 50)
                            NPC.velocity = (NPC.velocity * 40f + homeInVec * -16f) / 41f;
                        else
                            NPC.velocity *= 0.975f;
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (aiCounter == 1)
                        {
                            NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<OratorHand>());
                            NPC.NewNPC(null, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<OratorHand>());
                        }
                    }
                    if (aiCounter == 360)
                    {
                        aiCounter = -30;
                        attackCounter = 0;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.PhaseChange:
                    string baseKey;
                    NPC.damage = 0;
                    dashing = false;
                    if (!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()))
                    {
                        #region Movement
                        goalPosition = target.Center + Vector2.UnitY * -250;
                        NPC.velocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPosition - NPC.Center).Length() / 10f);
                        #endregion

                        #region Dialogue and Events
                        baseKey = "LunarCult.TheOrator.BossText.PhaseChange.";
                        switch (aiCounter - 60)
                        {
                            case 30:
                                DisplayMessage(baseKey + 0, NPC);
                                break;
                            case 120:
                                DisplayMessage(baseKey + 1, NPC);
                                break;
                            case 240:
                                DisplayMessage(baseKey + 2, NPC);
                                break;
                            case 300:
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), target.Center, Vector2.Zero, ModContent.ProjectileType<OratorBorder>(), 0, 0f);
                                break;
                            case 360:
                                DisplayMessage(baseKey + 3, NPC);
                                break;
                            case 420:
                                AIState = States.DarkOrbit; //DarkOrbit
                                aiCounter = -60;
                                attackCounter = 0;
                                attackCycles = 0;
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        NPC.velocity *= 0.9f;
                        aiCounter = -1;
                    }
                    break;
                case States.Defeat:
                    #region Movement
                    NPC.dontTakeDamage = true;
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    homeInVector = target.Center - NPC.Center;
                    targetDist = homeInVector.Length();
                    homeInVector.Normalize();
                    if (targetDist > 300f)
                        NPC.velocity = (NPC.velocity * 40f + homeInVector * 12f) / 41f;
                    else
                    {
                        if (targetDist < 250f)
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * -12f) / 41f;
                        else
                            NPC.velocity *= 0.975f;
                    }
                    #endregion
                    NPC.damage = 0;
                    dashing = false;
                    #region Dialogue and Events
                    baseKey = "LunarCult.TheOrator.BossText.Defeat.";
                    switch (aiCounter - 60)
                    {
                        case 30:
                            DisplayMessage(baseKey + 0, NPC);
                            break;
                        case 180:
                            DisplayMessage(baseKey + 1, NPC);
                            break;
                        case 300:
                            DisplayMessage(baseKey + 2, NPC);
                            break;
                        case 420:
                            DisplayMessage(baseKey + 3, NPC);
                            break;
                        case 480:
                            WorldGen.TriggerLunarApocalypse();
                            break;
                        case 540:
                            DisplayMessage(baseKey + 4, NPC);
                            break;
                        case 600:
                            SoundEngine.PlaySound(SoundID.Zombie105, NPC.Center);
                            break;
                        case 720:
                            DisplayMessage(baseKey + 5, NPC);
                            break;
                        case 840:
                            DisplayMessage(baseKey + 6, NPC);
                            break;
                        case 960:
                            DisplayMessage(baseKey + 7, NPC);
                            break;
                        case 1080:
                            NPC.HitSound = null;
                            NPC.dontTakeDamage = false;
                            NPC.HideStrikeDamage = true;
                            NPC.StrikeInstantKill();
                            break;
                    }
                    #endregion

                    break;
                case States.Testing:
                    #region Movement
                    homeInVector = target.Center - NPC.Center;
                    targetDist = homeInVector.Length();
                    homeInVector.Normalize();
                    if (targetDist > 1000f)
                        NPC.velocity = (NPC.velocity * 40f + homeInVector * 12f) / 41f;
                    else
                    {
                        if (targetDist < 950f)
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * -12f) / 41f;
                        else
                            NPC.velocity *= 0.975f;
                    }
                    #endregion
                    aiCounter = -1;
                    break;
            }
            if (scytheSpin)
                DoScytheSpin();
            if (scytheSlice)
                DoScytheSlice();

            aiCounter++;
            if (hitTimer > 0)
                hitTimer--;
        }
        Particle spinSmear = null;
        Particle spinHandle = null;
        Vector2 spinDirection = Vector2.Zero;
        int spinCounter = 0;
        private void DoScytheSpin()
        {            
            if (spinCounter == 0)
            {
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                spinDirection = NPC.velocity.SafeNormalize(Vector2.UnitX * NPC.direction) * -1;
                spinDirection.Normalize();
                spinDirection.RotatedBy(PiOver2 + (Pi / 4));
            }

            spinDirection = spinDirection.RotatedBy(MathHelper.Clamp(1f - (spinCounter / 60f), 0.5f, 1f) * MathHelper.PiOver4 * 0.50f);
            spinDirection.Normalize();

            if (spinCounter > 15)
            {
                if (spinSmear != null)
                {
                    if (spinCounter < 30)
                    {
                        spinSmear.Rotation = spinDirection.ToRotation() + MathHelper.PiOver2;
                        spinSmear.Time = 0;
                        spinSmear.Position = NPC.Center;
                        spinSmear.Scale = NPC.scale * 1.9f;
                        spinSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, spinCounter / 40f);
                        spinSmear.Color.A = (byte)(255 - 255 * MathHelper.Clamp((spinCounter - 15) / 15f, 0f, 1f));
                    }
                    else
                    {
                        spinSmear.Kill();
                        spinSmear = null;

                        spinHandle.Kill();
                        spinHandle = null;

                        scytheSpin = false;
                        spinCounter = 0;
                        return;
                    }
                }
            }
            else
            {
                if(spinHandle == null)
                {
                    spinHandle = new SemiCircularSmearVFX(NPC.Center, Color.LimeGreen, spinDirection.ToRotation(), NPC.scale * 1.5f, Vector2.One);
                    GeneralParticleHandler.SpawnParticle(spinHandle);
                }
                if (spinSmear == null)
                {
                    spinSmear = new CircularSmearVFX(NPC.Center, Color.LimeGreen, spinDirection.ToRotation(), NPC.scale * 1.5f);
                    GeneralParticleHandler.SpawnParticle(spinSmear);
                    spinSmear.Color.A = 0;                   
                }
                else
                {
                    spinSmear.Rotation = spinDirection.ToRotation() + MathHelper.PiOver2;
                    spinSmear.Time = 0;
                    spinSmear.Position = NPC.Center;
                    spinSmear.Scale = NPC.scale * 1.9f;
                    spinSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, spinCounter / 40f);
                    spinSmear.Color.A = (byte)(255 * MathHelper.Clamp((spinCounter / 10f), 0f, 1f));
                }
            }
            if(spinSmear != null)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Vector2 speed = spinDirection.RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
                Dust dust = Dust.NewDustPerfect(NPC.Center + (spinDirection.RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(150f, 180f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;

                if(spinHandle != null)
                {
                    spinHandle.Rotation = spinSmear.Rotation + (Pi / 8f);
                    spinHandle.Time = 0;
                    spinHandle.Position = NPC.Center;
                    spinHandle.Scale = spinSmear.Scale / 1.8f;
                    spinHandle.Color = Color.Gold;
                    spinHandle.Color.A = (byte)(spinSmear.Color.A);
                }
            }
            spinCounter++;
        }
        Particle sliceSmear = null;
        Particle sliceHandle = null;
        Vector2 sliceDirection = Vector2.Zero;
        int sliceCounter = 0;
        private void DoScytheSlice()
        {
            if (sliceCounter == 0)
            {
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                SoundEngine.PlaySound(Dash);
                sliceDirection = NPC.velocity.SafeNormalize(Vector2.UnitX * NPC.direction) * 1;
                sliceDirection.Normalize();
                sliceDirection = sliceDirection.RotatedBy(-Pi + PiOver4);
            }

            sliceDirection = sliceDirection.RotatedBy(MathHelper.Clamp(1f - (sliceCounter / 60f), 0.5f, 1f) * MathHelper.PiOver4 * 0.35f);
            sliceDirection.Normalize();

            //Update the variables of the smear                      
            if (sliceCounter > 8)
            {               
                if (sliceSmear != null)
                {
                    if (sliceCounter < 16)
                    {
                        sliceSmear.Rotation = sliceDirection.ToRotation() + MathHelper.PiOver2;
                        sliceSmear.Time = 0;
                        sliceSmear.Position = NPC.Center;
                        sliceSmear.Scale = NPC.scale * 1.5f;
                        sliceSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, sliceCounter / 20f);
                        sliceSmear.Color.A = (byte)(255 - 255 * MathHelper.Clamp(((sliceCounter - 8) / 8f), 0f, 1f));
                    }
                    else
                    {
                        sliceSmear.Kill();
                        sliceSmear = null;

                        sliceHandle.Kill();
                        sliceHandle = null;

                        scytheSlice = false;
                        sliceCounter = 0;
                        return;
                    }
                }
                if (sliceCounter >= 120)
                    sliceCounter = -1;
            }
            else
            {
                if (sliceHandle == null)
                {
                    sliceHandle = new SemiCircularSmearVFX(NPC.Center, Color.LimeGreen, spinDirection.ToRotation(), NPC.scale * 1.5f, Vector2.One);
                    GeneralParticleHandler.SpawnParticle(sliceHandle);
                }
                if (sliceSmear == null)
                {
                    sliceSmear = new CircularSmearVFX(NPC.Center, Color.LimeGreen, sliceDirection.ToRotation(), NPC.scale * 1.5f);
                    GeneralParticleHandler.SpawnParticle(sliceSmear);
                    sliceSmear.Color.A = 0;
                }
                else
                {
                    sliceSmear.Rotation = sliceDirection.ToRotation() + MathHelper.PiOver2;
                    sliceSmear.Time = 0;
                    sliceSmear.Position = NPC.Center;
                    sliceSmear.Scale = NPC.scale * 1.5f;
                    sliceSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, sliceCounter / 20f);
                    sliceSmear.Color.A = (byte)(255 * MathHelper.Clamp((sliceCounter / 4f), 0f, 1f));
                }
            }
            if (sliceSmear != null)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Vector2 speed = sliceDirection.RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
                Dust dust = Dust.NewDustPerfect(NPC.Center + (sliceDirection.RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(110f, 150f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;

                if (sliceHandle != null)
                {
                    sliceHandle.Rotation = sliceSmear.Rotation + (Pi / 8f);
                    sliceHandle.Time = 0;
                    sliceHandle.Position = NPC.Center;
                    sliceHandle.Scale = sliceSmear.Scale / 1.8f;
                    sliceHandle.Color = Color.Gold;
                    sliceHandle.Color.A = sliceSmear.Color.A;
                }
            }
            sliceCounter++;
        }
        
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                hitTimer = 35;
                NPC.netUpdate = true;
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i <= 50; i++)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle);
                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
            }
            NPC.downedAncientCultist = true;
            DownedNPCSystem.downedOrator = true;
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.LunarCraftingStation);

            //Boss Bag
            npcLoot.Add(ItemDropRule.BossBag(ItemID.CultistBossBag));

            //Normal Only
            var normalOnly = npcLoot.DefineNormalOnlyDropSet();
            {
                normalOnly.Add(ItemID.BossMaskCultist, 7);
            }

            // Test Drop for not letting Orator heal
            npcLoot.DefineConditionalDropSet(WindfallConditions.OratorNeverHeal).Add(ModContent.ItemType<ShadowHandStaff>());

            // Trophy
            npcLoot.Add(ItemID.AncientCultistTrophy, 10);

            // Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ItemID.LunaticCultistMasterTrophy);

            //Lore
            npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedOrator, ModContent.ItemType<OraLore>(), desc: DropHelper.FirstKillText);
            npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedOrator, ModContent.ItemType<LorePrelude>(), desc: DropHelper.FirstKillText);
        }
        public override bool CheckActive() => false;
        internal static void DisplayMessage(string key, NPC NPC)
        {
            Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width / 2, NPC.width / 2);
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, Color.LightGreen, GetWindfallTextValue($"Dialogue.{key}"), true)];
            if (MyDialogue.text.Length > 50)
                MyDialogue.lifeTime = 60 + MyDialogue.text.Length;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = frameHeight;
            if (scytheSlice || scytheSpin)
            {
                NPC.frame.Y = frameHeight;
                NPC.frame.X = frameHeight;
            }
            else if (AIState == States.DarkStorm || AIState == States.DarkEmbrace || AIState == States.DarkMonster || AIState == States.DarkCollision)
            {
                NPC.frame.Y = frameHeight;
                NPC.frame.X = 0;
            }
            else if (AIState == States.DarkFlight || AIState == States.DarkSpawn || AIState == States.DarkBarrage || AIState == States.DarkTides || AIState == States.Defeat || AIState == States.PhaseChange)
            {
                NPC.frame.X = frameHeight;
                NPC.frame.Y = 0;
            }
            else
            {
                NPC.frame.X = 0;
                NPC.frame.Y = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 4, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }
}

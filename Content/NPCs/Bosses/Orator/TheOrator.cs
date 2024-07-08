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
using System.Diagnostics.Metrics;
using Terraria;

namespace Windfall.Content.NPCs.Bosses.TheOrator
{
    [AutoloadBossHead]
    public class TheOrator : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator";
        public override string BossHeadTexture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_Head";
        public static readonly SoundStyle Dash = new("CalamityMod/Sounds/Item/DeadSunShot") { PitchVariance = 0.35f, Volume = 0.5f };
        public static readonly SoundStyle DashWarn = new("CalamityMod/Sounds/Item/DeadSunRicochet") { Volume = 0.5f };
        public static readonly SoundStyle HurtSound = new("CalamityMod/Sounds/NPCHit/ShieldHit", 3);
        private static int MonsterDamage;
        internal static int GlobDamage;
        internal static int BoltDamage;
        private static int DashDelay;
        public override void SetStaticDefaults()
        {
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
            NPC.friendly = false;
            NPC.boss = true;
            NPC.width = NPC.height = 44;
            NPC.Size = new Vector2(150, 150);
            NPC.aiStyle = -1;
            //Values gotten from Lunatic Cultist. Subject to change.
            NPC.DR_NERD(0.10f);
            NPC.LifeMaxNERB(Main.masterMode ? 253000 : Main.expertMode ? 220000 : 180000, 244000);
            NPC.npcSlots = 5f;
            NPC.defense = 50;
            NPC.HitSound = HurtSound;
            NPC.DeathSound = SoundID.Zombie105;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.Calamity().canBreakPlayerDefense = true;
        }
        private float forcefieldOpacity = 0f;
        private int hitTimer = 0;
        private float forcefieldScale = 0f;

        
        public static bool noSpawnsEscape = true;
        public override void OnSpawn(IEntitySource source)
        {
            noSpawnsEscape = true;
            MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 180);
            GlobDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 255 : CalamityWorld.death ? 214 : CalamityWorld.revenge ? 186 : Main.expertMode ? 140 : 100);
            BoltDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 248 : CalamityWorld.death ? 172 : CalamityWorld.revenge ? 160 : Main.expertMode ? 138 : 90);
            DashDelay = CalamityWorld.death ? 20 : CalamityWorld.revenge ? 25 : 30;
        }
        private enum States
        {
            Spawning,
            DarkMonster,
            DarkSpawn,
            DarkBarrage,
            DarkSlice,
            DarkStorm,
            DarkCollision,

            DarkOrbit,

            PhaseChange,
            Defeat,
        }
        private States AIState
        {
            get => (States)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        int aiCounter = 0;
        int attackCounter = 0;
        bool dashing = false;
        private int AttackCycles = 0;
        Vector2 VectorToTarget = Vector2.Zero;
        public override bool PreAI()
        {
            foreach(Player p in Main.player)
            {
                if(p != null && p.active)
                    p.AddBuff(ModContent.BuffType<BossEffects>(), 2);
            }
            return true;
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            if (target.active == false || target.dead)
                NPC.active = false;
            if (NPC.Center.X < target.Center.X)
                NPC.direction = 1;
            else
                NPC.direction = -1;
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
            if ((float)NPC.life / (float)NPC.lifeMax <= 0.1f)
                if (AIState != States.Defeat)
                    NPC.DR_NERD(1f);
                else
                    NPC.DR_NERD(0.1f);

            switch (AIState)
            {
                //Phase 1
                case States.Spawning:
                    DashDelay = CalamityWorld.death ? 20 : CalamityWorld.revenge ? 25 : 30;
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];

                    int height = 150;
                    if (aiCounter > 270)
                        height = 300;

                    if (NPC.Center.Y < target.Center.Y - height)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;

                    if (aiCounter > 150)
                    {
                        forcefieldOpacity = Lerp(forcefieldOpacity, 1f, 0.08f);
                        forcefieldScale = Lerp(forcefieldScale, 1f, 0.08f);
                        if (forcefieldScale >= 0.99f)
                        {
                            NPC.dontTakeDamage = false;
                            forcefieldOpacity = 1f;
                            forcefieldScale = 1f;
                        }
                    }
                    if (aiCounter == 360)
                    {
                        aiCounter = -30;
                        SoundEngine.PlaySound(DashWarn);
                        attackCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.DarkMonster:
                    if (aiCounter == 1)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 40, ModContent.ProjectileType<DarkMonster>(), MonsterDamage, 0f);                   
                    if (aiCounter < 900)
                    {
                        #region Movement 
                        NPC.velocity = target.velocity;
                        VectorToTarget = target.Center - NPC.Center;

                        float rotationRate = 0.03f;
                        if(target.velocity.Length() > 0.1f)
                            rotationRate *=  Math.Abs(Utilities.AngleBetween(target.velocity, VectorToTarget) - Pi);
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
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, TargetVector.SafeNormalize(Vector2.UnitX).RotatedBy(TwoPi / globCount * i) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                            }
                        }
                        #endregion
                    }
                    if (aiCounter == 900)
                    {
                        aiCounter = 0;
                        if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
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
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];                   

                    if (NPC.Center.Y < target.Center.Y - 300)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    #endregion
                    
                    NPC.DR_NERD(0.1f + (0.1f * Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>()).Count()));
                    NPC.damage = 0;
                    const int EndTime = 1500;
                    int SpawnCount = CalamityWorld.death ? 3 : CalamityWorld.revenge || Main.expertMode ? 2 : 1;
                    if(!CalamityWorld.death && Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active).Count() <= SpawnCount + 1)
                        SpawnCount++;
                    if (NPC.AnyNPCs(ModContent.NPCType<DarkSpawn>()))
                    {
                        if (aiCounter > EndTime)
                        {
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                            {
                                if (spawn.ModNPC is DarkSpawn darkSpawn)
                                    darkSpawn.CurrentAI = DarkSpawn.AIState.Sacrifice;
                            }
                        }
                        if (aiCounter > 150)
                        {
                            attackCounter = 0;
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                            {
                                if (spawn.ModNPC is DarkSpawn darkSpawn && darkSpawn.CurrentAI != DarkSpawn.AIState.OnBoss)
                                    attackCounter++;
                            }
                            if (attackCounter < SpawnCount)
                                foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                                {
                                    if (attackCounter >= SpawnCount)
                                        break;
                                    if (spawn.ModNPC is DarkSpawn darkSpawn && darkSpawn.CurrentAI == DarkSpawn.AIState.OnBoss)
                                    {
                                        Vector2 ToTarget = (target.Center - spawn.Center);
                                        spawn.velocity = ToTarget.SafeNormalize(Vector2.Zero) * -10;
                                        darkSpawn.CurrentAI = DarkSpawn.AIState.Dashing;
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
                        attackCounter = 0;                                     

                        if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
                            AIState = States.PhaseChange;
                        else
                        {
                            NPC.DR_NERD(0.1f);
                            if (AttackCycles % 2 == 0)
                            {
                                AIState = States.DarkCollision;                                
                            }
                            else
                            {
                                aiCounter = -30;
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                                AIState = States.DarkSlice;
                            }
                        }
                        return;
                    }
                    break;
                case States.DarkBarrage:
                    if (aiCounter <= 1100)
                    {
                        #region Movement
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        float spinRate = CalamityWorld.death ? 0.045f : CalamityWorld.revenge ? 0.0475f : 0.045f;
                        if (aiCounter == 0)
                        {
                            NPC.position.X = target.position.X;
                            NPC.position.Y = target.position.Y - 400;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[3] = Main.rand.Next(500, 600);
                        }
                        else
                        {
                            VectorToTarget = target.Center - NPC.Center;
                            NPC.Center = target.Center + new Vector2(0, -400).RotatedBy(aiCounter * spinRate);
                            NPC.velocity = target.velocity;
                        }
                        #endregion

                        #region Projectiles     
                        if (aiCounter < 1000 && aiCounter > 30)
                        {
                            int gapSize = CalamityWorld.death ? 275 : CalamityWorld.revenge ? 300 : Main.expertMode ? 325 : 350;
                            if (aiCounter % (CalamityWorld.death ? 100 : CalamityWorld.revenge ? 110 : 120) == 0)
                            {
                                float offset = Main.rand.Next(0, 200);
                                for (int i = 0; i < 12; i++)
                                {
                                    Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), target.Center + new Vector2(-1500, -1200 + (gapSize * i) + offset), new Vector2(20, -12), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);
                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), target.Center + new Vector2(1500, -1200 + (gapSize * i) + offset), new Vector2(-20, -12), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);
                                }
                            }
                            else if (aiCounter % (CalamityWorld.death ? 50 : CalamityWorld.revenge ? 55 : 60) == 0)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                                Particle pulse = new DirectionalPulseRing(NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 4f, new(117, 255, 159), new Vector2(0.5f, 1f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                        else if (aiCounter % (CalamityWorld.death ? 50 : CalamityWorld.revenge ? 55 : 60) == 0)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            Particle pulse = new DirectionalPulseRing(NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 4f, new(117, 255, 159), new Vector2(0.5f, 1f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                            GeneralParticleHandler.SpawnParticle(pulse);
                        }

                    }
                    #endregion
                    if (aiCounter > 1100)
                    {
                        #region Attack Transition
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * (10f / (((float)aiCounter - 1000f) /66f) + 1f);
                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 150f, Main.rand.NextVector2Circular(5f, 5f), 1.5f * ((aiCounter - 1100) / 2));
                        if (aiCounter > 1300 || (float)NPC.life / (float)NPC.lifeMax <= 0.1f)
                        {
                            aiCounter = 0;
                            if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
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
                    int SliceDashCount = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 4 : 3;
                    if (aiCounter < 0)
                    {
                        if (CalamityWorld.death || (CalamityWorld.revenge && aiCounter < -10) || (Main.expertMode && aiCounter < -20))
                        {
                            VectorToTarget = target.Center - NPC.Center;
                            NPC.velocity = VectorToTarget.SafeNormalize(Vector2.UnitY) * -5;
                        }
                    }
                    if (aiCounter == 0)
                    {
                        if(attackCounter == 0)
                            AttackCycles++;
                        SoundEngine.PlaySound(Dash);
                        VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -75;
                        dashing = true;
                        //values gotten from Astrum Deus' contact damage. Subject to change.
                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);

                        Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }                   
                    if (aiCounter > 0)
                    {
                        NPC.velocity = VectorToTarget;
                        VectorToTarget *= 0.93f;

                        #region Dash Projectiles
                        if (NPC.velocity.Length() > 15f)
                        {
                            int dustStyle = Main.rand.NextBool() ? 66 : 263;
                            Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextBool(3) ? 191 : dustStyle);
                            dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                            dust.noGravity = true;
                            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                        }
                        if (aiCounter % 10 == 0 && Main.expertMode)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        else if (aiCounter % 5 == 0 && Main.expertMode)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        #endregion

                        if (VectorToTarget.Length() <= 5)
                        {
                            int radialCounter = CalamityWorld.death ? 12 : CalamityWorld.revenge ? 10 : 8;
                            for(int i = 0; i < radialCounter; i++)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(TwoPi / radialCounter * i) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            }
                            if (++attackCounter == SliceDashCount)
                            {
                                aiCounter = 0;
                                dashing = false;
                                if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
                                {
                                    AIState = States.PhaseChange;
                                    attackCounter = 0;
                                }
                                else
                                {
                                    NPC.damage = 0;
                                    AIState = States.DarkStorm;
                                }
                            }
                            else
                            {
                                aiCounter = DashDelay * -1;
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                            }
                            return;
                        }
                    }
                    break;
                case States.DarkStorm:
                    int AttackFrequency = CalamityWorld.death ? 10 : CalamityWorld.revenge ? 12 : Main.expertMode ? 14 : 16;
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    #region Movement
                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    #endregion
                    #region Projectiles
                    if (aiCounter > 120 && NPC.Center.Y < target.Center.Y - 50)
                    {
                        Projectile proj;
                        if (aiCounter % 5 == 0)
                        {
                            //The Anti-Cheesers
                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X - 400, target.Center.Y - 800), new Vector2(Main.rand.NextFloat(-6f, -3f), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage * 3, 0f, -1, 1, 3f);
                            proj.Calamity().DealsDefenseDamage = true;
                            proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X + 400, target.Center.Y - 800), new Vector2(Main.rand.NextFloat(3f, 6f), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage * 3, 0f, -1, 1, 3f);
                            proj.Calamity().DealsDefenseDamage = true;
                        }
                        if (aiCounter % AttackFrequency == 0)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                            for (int i = 0; i < 10; i++)
                            {
                                EmpyreanMetaball.SpawnDefaultParticle(new(NPC.Center.X, NPC.Center.Y - 50), Main.rand.NextVector2Unit(0, -Pi) * Main.rand.NextFloat(0f, 15f), 25 * Main.rand.NextFloat(1f, 2f));
                            }
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
                    #endregion
                    if(aiCounter > 720)
                    {
                        attackCounter = 0;
                        if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
                        {
                            AIState = States.PhaseChange;
                            aiCounter = 0;
                        }
                        else
                        {
                            aiCounter = 0;
                            AIState = States.DarkBarrage;
                        }
                    }
                    break;
                case States.DarkCollision:

                    #region Movement
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];

                    if (NPC.Center.Y < target.Center.Y - 300)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    #endregion

                    if(aiCounter % (80 + DarkCoalescence.fireDelay) == 0 && aiCounter < 700)
                    {
                        if(Main.rand.NextBool())
                        {
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(target.Center.X + 500, target.Center.Y), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, -1);
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(target.Center.X - 500, target.Center.Y), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 1);
                        }
                        else
                        {
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(target.Center.X, target.Center.Y + 500), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 0, -1);
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(target.Center.X, target.Center.Y - 500), Vector2.Zero, ModContent.ProjectileType<DarkCoalescence>(), MonsterDamage, 0f, -1, 0, 0, 1);
                        }
                    }

                    if(aiCounter >= 850)
                    {
                        if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f)
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
                    int OrbitDashCount = CalamityWorld.death ? 5 : CalamityWorld.revenge ? 3 : 1;
                    float OrbitRate = CalamityWorld.death ? 0.035f : CalamityWorld.revenge ? 0.03f : 0.025f;
                    Projectile border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                    if (aiCounter >= 0)
                    {
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (aiCounter == 0)
                        {
                            NPC.position.X = border.position.X;
                            NPC.position.Y = border.position.Y - 700;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[3] = Main.rand.Next(500, 600);
                        }
                        if (aiCounter < NPC.ai[3] - 45)
                        {
                            VectorToTarget = target.Center - NPC.Center;
                            NPC.Center = border.Center + new Vector2(0, -700).RotatedBy(aiCounter * OrbitRate);
                            if (aiCounter % 10 == 0 && aiCounter > 30)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f);
                            if (Main.expertMode && aiCounter % (CalamityWorld.revenge ? 90 : 120) == 0 && aiCounter > 30)
                            {
                                SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, NPC.Center);
                                for (int i = 0; i < 20; i++)
                                {
                                    EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(Pi / 6) * Main.rand.NextFloat(0f, 25f), 30 * Main.rand.NextFloat(1f, 2f));
                                }
                                for (int i = 1; i < 8; i++)
                                {
                                    Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * (3.5f * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                    proj.timeLeft = (int)(proj.timeLeft / 1.5f);

                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (border.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * (3.5f * -i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                    proj.timeLeft = (int)(proj.timeLeft / 1.5f);
                                }
                            }
                        }
                        else if (aiCounter < NPC.ai[3])
                        {
                            if (aiCounter == NPC.ai[3] - DashDelay)
                            {
                                if ((float)NPC.life / (float)NPC.lifeMax <= 0.1f)
                                {
                                    AIState = States.Defeat;
                                    attackCounter = 0;
                                    aiCounter = 0;
                                }
                                else
                                {
                                    dashing = true;
                                    SoundEngine.PlaySound(DashWarn);
                                    NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -4;
                                }
                            }
                            else if (aiCounter - NPC.ai[3] < (CalamityWorld.death ? - 15 : CalamityWorld.revenge  ? -20 : -25))
                            {
                                VectorToTarget = target.Center - NPC.Center;
                                NPC.velocity = VectorToTarget.SafeNormalize(Vector2.UnitY) * -4;
                            }
                        }
                        else
                        {
                            if (aiCounter == NPC.ai[3])
                            {
                                VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -75;
                                SoundEngine.PlaySound(Dash);
                                //values gotten from Astrum Deus' contact damage. Subject to change.
                                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                Particle pulse = new DirectionalPulseRing(NPC.Center, VectorToTarget.SafeNormalize(Vector2.Zero) * 0f, new(117, 255, 159), new Vector2(0.5f, 2f), (target.Center - NPC.Center).ToRotation(), 0f, 1f, 24);
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                            NPC.velocity = VectorToTarget;
                            VectorToTarget *= 0.95f;
                            if (Vector2.Distance(border.Center, NPC.Center) >= OratorBorder.Radius)
                            {
                                VectorToTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * VectorToTarget.Length();
                                NPC.velocity = VectorToTarget;
                            }

                            #region Dash Projectiles
                            if (NPC.velocity.Length() > 2f)
                            {
                                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                                Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextBool(3) ? 191 : dustStyle);
                                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                                //dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                                dust.noGravity = true;
                                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                            }
                            if (aiCounter % (CalamityWorld.revenge ? 8 : 10) == 0 && Main.expertMode)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            }
                            else if (aiCounter % (CalamityWorld.revenge ? 4 : 5) == 0 && Main.expertMode)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            #endregion
                            if (VectorToTarget.Length() <= 10)
                            {
                                NPC.damage = 0;
                                if (attackCounter++ == OrbitDashCount - 1)
                                {
                                    dashing = false;
                                    aiCounter = 0;
                                    if ((float)NPC.life / (float)NPC.lifeMax <= 0.1f)
                                    {
                                        AIState = States.Defeat;
                                        attackCounter = 0;
                                    }
                                    else
                                    {
                                        AIState = States.DarkOrbit;
                                        aiCounter = -60;
                                        attackCounter = 0;
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
                        if (NPC.Center.Y < border.Center.Y - 700)
                            NPC.velocity.Y++;
                        else
                            NPC.velocity.Y--;
                        if (NPC.Center.X < border.Center.X)
                            NPC.velocity.X++;
                        else
                            NPC.velocity.X--;
                        if (NPC.velocity.Length() > 15)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    }
                    break;
                //Animations
                case States.PhaseChange:
                    #region Movement
                    if (NPC.Center.Y < target.Center.Y - 175)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    #endregion
                    NPC.damage = 0;
                    #region Dialogue and Events
                    string baseKey = "LunarCult.TheOrator.BossText.PhaseChange.";
                    switch (aiCounter)
                    {
                        case 30:
                            DisplayMessage(baseKey + 0, NPC);
                            break;
                        case 180:
                            DisplayMessage(baseKey + 1, NPC);
                            break;
                        case 240:
                            DisplayMessage(baseKey + 2, NPC);
                            break;
                        case 300:
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), target.Center, Vector2.Zero, ModContent.ProjectileType<OratorBorder>(), 0, 0f);
                            break;
                        case 360:
                            DisplayMessage(baseKey + 3, NPC);
                            break;
                        case 420:
                            AIState = States.DarkOrbit;
                            aiCounter = -60;
                            attackCounter = 0;
                            break;
                    }
                    #endregion
                    break;
                case States.Defeat:
                    #region Movement
                    NPC.dontTakeDamage = true;                    
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    Vector2 TargetLocation = new(target.Center.X, target.Center.Y - 150);
                    if (attackCounter == 0)
                    {
                        if(aiCounter < 30)
                            aiCounter = 0;
                        if (NPC.velocity.Length() < 0.5f && (TargetLocation - NPC.Center).Length() < 25f)
                        {
                            NPC.velocity = Vector2.Zero;
                            attackCounter = 1;
                        }
                        else
                        {
                            if (NPC.Center.Y < TargetLocation.Y)
                                NPC.velocity.Y++;
                            else
                                NPC.velocity.Y--;
                            if (NPC.Center.X < TargetLocation.X)
                                NPC.velocity.X++;
                            else
                                NPC.velocity.X--;
                            if ((TargetLocation - NPC.Center).Length() < 25f)
                                NPC.velocity -= NPC.velocity.SafeNormalize(Vector2.Zero);
                            else if (NPC.velocity.Length() > 7.5f)
                                NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 7.5f;
                        }
                    }
                    else
                    {
                        dashing = true;
                        if (Math.Abs((TargetLocation - NPC.Center).Y) > 100f)
                        {
                            attackCounter = 0;
                            return;
                        }
                        if (Math.Abs(NPC.velocity.X) < 1f && Math.Abs((TargetLocation - NPC.Center).X) < 25f)
                        {
                            NPC.velocity.X = 0;
                        }
                        else if(!(Math.Abs(NPC.velocity.X) < 1f) || Math.Abs((TargetLocation - NPC.Center).X) > 100f)
                        {
                            if (NPC.Center.X < TargetLocation.X)
                                NPC.velocity.X++;
                            else
                                NPC.velocity.X--;
                            if (NPC.velocity.X != 0)
                            {
                                if (Math.Abs((TargetLocation - NPC.Center).X) < 25f)
                                    NPC.velocity.X -= 1 * (NPC.velocity.X / Math.Abs(NPC.velocity.X));
                                else if (Math.Abs(NPC.velocity.X) > 7.5f)
                                    NPC.velocity.X = 7.5f * (NPC.velocity.X / Math.Abs(NPC.velocity.X));
                            }
                        }
                        NPC.velocity.Y = (float)(Math.Sin((attackCounter - 1) / 10) * 1);
                        attackCounter++;
                    }
                    #endregion
                    NPC.damage = 0;
                    #region Dialogue and Events
                    baseKey = "LunarCult.TheOrator.BossText.Defeat.";
                    switch (aiCounter)
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
            }
            
            aiCounter++;
            if (hitTimer > 0)
                hitTimer--;
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
            npcLoot.DefineConditionalDropSet(WindfallConditions.OratorNeverHeal).Add(ModContent.ItemType<DeepSeekerStaff>());

            // Trophy
            npcLoot.Add(ItemID.AncientCultistTrophy, 10);

            // Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ItemID.LunaticCultistMasterTrophy);

            //Lore
            npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedOrator, ModContent.ItemType<OraLore>(), desc: DropHelper.FirstKillText);
        }
        internal static void DisplayMessage(string key, NPC NPC)
        {
            Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width / 2, NPC.width / 2);
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, Color.LightGreen, GetWindfallTextValue($"Dialogue.{key}"), true)];
            if (MyDialogue.text.Length > 50)
                MyDialogue.lifeTime = 60 + MyDialogue.text.Length;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.Center.Y) - screenPos;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            DrawForcefield(spriteBatch);
            return false;
        }
        public void DrawForcefield(SpriteBatch spriteBatch)
        {
            spriteBatch.EnterShaderRegion();
            float intensity = 35f / 35f;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            float flickerPower = 0f;
            if (lifeRatio < 0.6f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.3f)
                flickerPower += 0.15f;
            if (lifeRatio < 0.1f)
                flickerPower += 0.2f;
            if (lifeRatio < 0.05f)
                flickerPower += 0.1f;
            float opacity = forcefieldOpacity;
            opacity *= Lerp(1f, Max(1f - flickerPower, 0.56f), (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * Lerp(3f, 5f, flickerPower)), 24D));

            if (dashing)
                intensity = 1.1f;

            intensity *= 0.75f;
            opacity *= 0.75f;

            Texture2D forcefieldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SupremeCalamitas/ForcefieldTexture").Value;
            GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/Perlin");

            Color forcefieldColor = Color.Black;
            Color secondaryForcefieldColor = Color.PaleGreen;

            if (dashing)
            {
                forcefieldColor *= 0.25f;
                secondaryForcefieldColor = Color.Lerp(secondaryForcefieldColor, Color.Black, 0.7f);
            }

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(intensity);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

            spriteBatch.Draw(forcefieldTexture, NPC.Center - Main.screenPosition, null, Color.White * opacity, 0f, forcefieldTexture.Size() * 0.5f, forcefieldScale * 3f, SpriteEffects.None, 0f);

            spriteBatch.ExitShaderRegion();
        }
    }
}

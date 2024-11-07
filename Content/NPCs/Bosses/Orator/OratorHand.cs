using CalamityMod.World;
using Luminance.Core.Graphics;
using Terraria.GameContent.Bestiary;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.NPCs.Bosses.Orator
{
    public class OratorHand : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/Enemies/OratorHand";
        public override string BossHeadTexture => "Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head";
        private int OratorIndex
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private int WhatHand
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;

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
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(OratorHand)}")),
        ]);
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 75;
            NPC.damage = 0;
            NPC.DR_NERD(0.10f);
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.LifeMaxNERB(Main.masterMode ? 140000 : Main.expertMode ? 80000 : 75000, 100000);
            NPC.knockBackResist = 0f;
            NPC.scale = 1.25f;
            NPC.HitSound = SoundID.DD2_LightningBugHurt with { Volume = 0.5f };
            NPC.DeathSound = SoundID.DD2_LightningBugDeath;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
            NPC.Calamity().canBreakPlayerDefense = true;            
        }
        internal Vector2 direction = Vector2.UnitX;
        int effectCounter = 0;
        internal bool attackBool = false;
        public override void OnSpawn(IEntitySource source)
        {
            zoom = 0f;
            deadCounter = 0;
            if (WhatHand == 0)
            {
                if (Main.npc.Where(n => n != null && n.active && n.type == NPC.type).Count() > 1)
                    WhatHand = -1;
                else
                    WhatHand = 1;
            }
        }
        public override bool PreAI()
        {            
            if (Main.npc[OratorIndex] == null)
                return true;
            NPC orator = Main.npc[OratorIndex];
            TheOrator modOrator = orator.As<TheOrator>();
            if (modOrator.AIState == TheOrator.States.Spawning)
                return true;
            if (NPC.life == 0)
            {
                NPC.life = 1;
            }
            if (!Main.npc.Any(n => n != null && n.active && n.type == NPC.type && n.As<OratorHand>().WhatHand == 1))
                return true;
            NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type && n.As<OratorHand>().WhatHand == 1);
            if (Main.npc.Where(n => n != null && n.active && n.type == NPC.type).Count() == 1)
            {
                NPC.realLife = -1;
                NPC.life = 0;
            }
            else if(mainHand.life > 1)
            {                
                if (mainHand.whoAmI != NPC.whoAmI)
                {
                    NPC.realLife = mainHand.whoAmI;
                }
            }
            else
            {
                NPC.realLife = -1;
                NPC.life = 1;
            }
            return true;
        }
        private static int deadCounter = 0;
        private static float zoom = 0f;
        private Vector2 shakeOffset = Vector2.Zero;
        private enum Pose
        {
            Default,
            Fist,
            Palm
        }
        private Pose CurrentPose = Pose.Default;
        public override void AI()
        {
            if (Main.npc[OratorIndex] == null || !Main.npc[OratorIndex].active || Main.npc[OratorIndex].type != ModContent.NPCType<TheOrator>())
            {
                if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
                    OratorIndex = NPC.FindFirstNPC(ModContent.NPCType<TheOrator>());
                else
                {
                    NPC.active = false;
                    return;
                }
            }
            NPC orator = Main.npc[OratorIndex];
            TheOrator modOrator = orator.As<TheOrator>();
            int aiCounter = modOrator.aiCounter;

            #region Death Scene
            if ((NPC.life == 1 || (NPC.realLife != -1 && Main.npc[NPC.realLife].life == 1)) && NPC.damage == 0)
            {
                CurrentPose = Pose.Default;
                if (deadCounter <= 1)
                {
                    NPC.dontTakeDamage = true;

                    for (int i = 0; i < NPC.buffTime.Length; i++)
                    {
                        NPC.buffTime[i] = 0;
                    }

                    NPC.velocity = (orator.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8f * (modOrator.AIState == TheOrator.States.DarkCollision ? 2.5f : (orator.Center - NPC.Center).Length() > 300f ? 1.5f : -1);
                }

                NPC otherHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type && n.As<OratorHand>().WhatHand == -1);

                NPC.Center -= shakeOffset;

                if (WhatHand == 1 && Main.npc.Where(n => n != null && n.active && n.type == NPC.type).Count() > 1)
                {
                    if (deadCounter < 60)
                        zoom = Lerp(0f, 0.4f, deadCounter / 60f);
                    else
                        zoom = 0.4f;
                    CameraPanSystem.Zoom = zoom;
                    CameraPanSystem.PanTowards(NPC.Center + ((otherHand.Center - NPC.Center) / 2f), Clamp(deadCounter / 60f, 0f, 1f));
                }

                if (deadCounter < 300)
                {
                    shakeOffset = Main.rand.NextVector2Circular(1f, 1f) * Clamp(Lerp(0f, 16f, (deadCounter - 120) / 240f), 0f, 16f);
                    NPC.Center += shakeOffset;
                }
                else
                    shakeOffset = Vector2.Zero;


                if (deadCounter == 360)
                {
                    CalamityMod.Particles.Particle p = new PulseRing(NPC.Center, Vector2.Zero, Color.Gray, 0f, 2f, 45);
                    GeneralParticleHandler.SpawnParticle(p);

                    for (int i = 0; i <= 50; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(NPC.Center, DustID.Ash);
                        dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                        dust.velocity = Main.rand.NextVector2Circular(16f, 16f);
                        dust.noGravity = true;
                    }

                    DeathAshParticle.CreateAshesFromNPC(NPC);

                    for(int i = 0; i < 30; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(NPC.Center - (NPC.rotation.ToRotationVector2() * (NPC.width / 1.5f)) + (NPC.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-16f, 16f)), (NPC.rotation.ToRotationVector2().RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(2f, 6f) * 3f), Main.rand.NextFloat(20f, 40f));
                }

                if (deadCounter < 300)
                    EmpyreanMetaball.SpawnDefaultParticle(NPC.Center - (NPC.rotation.ToRotationVector2() * (NPC.width / 1.5f)) + (NPC.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-16f, 16f)), (NPC.rotation.ToRotationVector2().RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(2f, 6f) * (1f + deadCounter / 100f)), Main.rand.NextFloat(20f, 40f));

                if (deadCounter == 420)
                    NPC.active = false;
                NPC.rotation += (0.025f * NPC.velocity.Length() * -WhatHand);
                NPC.velocity *= 0.95f;
                if (WhatHand == 1)
                    deadCounter++;
                return;
            }
            #endregion

            if (NPC.AnyNPCs(ModContent.NPCType<ShadowHand>()))
                NPC.dontTakeDamage = true;
            else if(NPC.life != 1)
                NPC.dontTakeDamage = false;

            if (modOrator.AIState == TheOrator.States.Spawning)
            {
                if (effectCounter > 60)
                {
                    CurrentPose = Pose.Default;
                    Vector2 goalPosition = orator.Center + orator.velocity + new Vector2(124 * WhatHand, +75);
                    goalPosition.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                    #region Movement
                    NPC.velocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPosition - NPC.Center).Length() / 10f);
                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                    NPC.direction = WhatHand;
                    #endregion
                }
                else
                {
                    if (effectCounter == 0)
                    {
                        CurrentPose = Pose.Fist;
                        NPC.velocity = Vector2.UnitY.RotatedBy(-PiOver4 * WhatHand) * 32f;
                        NPC.rotation = PiOver2 + (-PiOver4 * WhatHand);
                        NPC.direction = WhatHand;
                    }
                    NPC.velocity *= 0.9f;
                }
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center - (NPC.rotation.ToRotationVector2() * (NPC.width / 1.5f)) + (NPC.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-16f, 16f)), (NPC.rotation.ToRotationVector2().RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(2f, 6f)), Main.rand.NextFloat(20f, 40f));

            }
            else
            {
                switch (modOrator.AIState)
                {
                    case TheOrator.States.DarkSlice:
                        Vector2 goalPos = Vector2.Zero;
                        if (aiCounter >= -30)
                        {
                            CurrentPose = Pose.Default;

                            Vector2 goalDirection = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                            goalPos = orator.Center + goalDirection * -128f;
                            NPC.direction = -WhatHand;
                            if (WhatHand == 1)
                            {
                                Vector2 rotation = goalDirection.RotatedBy(PiOver2);
                                goalPos += rotation * 64;
                                NPC.rotation = rotation.RotatedBy(3 * Pi / 2).ToRotation();
                            }
                            else
                            {
                                Vector2 rotation = goalDirection.RotatedBy(3 * Pi / 2);
                                goalPos += rotation * 64;
                                NPC.rotation = rotation.RotatedBy(Pi / 2).ToRotation();
                            }
                            NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 3f);
                        }
                        else
                        {
                            CurrentPose = Pose.Fist;

                            aiCounter += 200;
                            if (aiCounter > 110)
                                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                            else
                                NPC.damage = 0;

                            if (WhatHand == 1)
                            {
                                if (aiCounter < 90)
                                {
                                    direction = direction.RotatedBy(Lerp(0.15f, 0.025f, aiCounter / 90f));
                                    direction.Normalize();
                                    //direction *= WhatHand;
                                    goalPos = modOrator.target.Center + (direction * 500);

                                    #region Movement
                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.rotation = NPC.DirectionTo(modOrator.target.Center).ToRotation();
                                    if (NPC.Center.X > modOrator.target.Center.X)
                                        NPC.direction = -1;
                                    else
                                        NPC.direction = 1;
                                    #endregion
                                }
                                else
                                {
                                    if (aiCounter < 110)
                                    {
                                        if (aiCounter == 90)//CalamityWorld.death || CalamityWorld.revenge && aiCounter < -10 || Main.expertMode && aiCounter < -20)
                                        {
                                            direction = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction);
                                            attackBool = false;
                                        }
                                        float reelBackSpeedExponent = 2.6f;
                                        float reelBackCompletion = Utils.GetLerpValue(0f, 20, aiCounter - 90, true);
                                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                        Vector2 reelBackVelocity = direction * -reelBackSpeed;
                                        NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                                    }
                                    else
                                    {
                                        if (aiCounter == 110)
                                            NPC.velocity = direction * 75;
                                        NPC.velocity *= 0.93f;
                                        NPC subHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type && n.As<OratorHand>().WhatHand == -1);
                                        if (NPC.Hitbox.Intersects(subHand.Hitbox) && !attackBool)
                                        {
                                            Vector2 midPoint = NPC.Center + ((subHand.Center - NPC.Center) / 2);

                                            NPC.Center = midPoint - (NPC.rotation.ToRotationVector2() * (NPC.width/3f));
                                            subHand.Center = midPoint - (subHand.rotation.ToRotationVector2() * (subHand.width / 3f));


                                            ScreenShakeSystem.StartShake(12.5f);
                                            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, midPoint);
                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                            {
                                                int projCount = 16;
                                                for (int i = 0; i < projCount; i++)
                                                {
                                                    for(int j = 1; j < 6; j++)
                                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), midPoint, (TwoPi / projCount * i).ToRotationVector2() * (i % 2 == 0 ? j * 3.25f : j * 3.75f), ModContent.ProjectileType<HandRing>(), TheOrator.BoltDamage, 0f);
                                                }
                                            }
                                            CalamityMod.Particles.Particle pulse = new PulseRing(midPoint, Vector2.Zero, new(253, 189, 53), 0f, 3f, 16);
                                            GeneralParticleHandler.SpawnParticle(pulse);
                                            CalamityMod.Particles.Particle explosion = new DetailedExplosion(midPoint, Vector2.Zero, new(255, 133, 187), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                                            GeneralParticleHandler.SpawnParticle(explosion);

                                            NPC.velocity = Vector2.Zero;
                                            subHand.velocity = Vector2.Zero;

                                            attackBool = true;
                                            return;
                                        }
                                        else if (attackBool)
                                        {
                                            NPC.velocity = Vector2.Zero;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (aiCounter >= 0)
                                {
                                    NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type && n.As<OratorHand>().WhatHand == 1);
                                    direction = mainHand.As<OratorHand>().direction;
                                    if (aiCounter < 90)
                                    {
                                        //direction *= WhatHand;
                                        goalPos = Main.LocalPlayer.Center + (direction * -500);

                                        #region Movement
                                        NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                        NPC.rotation = mainHand.rotation + Pi;
                                        NPC.direction = mainHand.direction *= -1;
                                        #endregion
                                    }
                                    else
                                    {
                                        if (aiCounter < 110)
                                        {
                                            float reelBackSpeedExponent = 2.6f;
                                            float reelBackCompletion = Utils.GetLerpValue(0f, 20, aiCounter - 90, true);
                                            float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                            Vector2 reelBackVelocity = direction * reelBackSpeed;
                                            NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
                                        }
                                        else if (!mainHand.As<OratorHand>().attackBool)
                                        {
                                            if (aiCounter == 110)
                                                NPC.velocity = direction * -75;
                                            NPC.velocity *= 0.93f;
                                            NPC.velocity = NPC.velocity.RotateTowards((mainHand.Center - NPC.Center).ToRotation(), PiOver4);
                                        }
                                        else
                                        {
                                            NPC.velocity = Vector2.Zero;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case TheOrator.States.DarkStorm:
                        CurrentPose = Pose.Palm;

                        NPC.damage = 0;
                        Vector2 goal = new(orator.Center.X + (600 * WhatHand), orator.Center.Y);
                        goal.X += (float)Math.Sin(aiCounter / 20f) * 48f * WhatHand;

                        #region Movement
                        NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 10f);
                        NPC.rotation = 3 * PiOver2;
                        NPC.direction = -WhatHand;
                        #endregion

                        break;
                    case TheOrator.States.DarkCollision:
                        if(Main.projectile.Any(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>()))
                        {
                            CurrentPose = Pose.Palm;

                            NPC.direction = WhatHand;
                            if (WhatHand == 1)
                            {
                                Projectile proj = Main.projectile.First(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>() && p.ai[1] == 1 || p.ai[2] == 1);
                                if (proj.ai[1] == 1)
                                {
                                    NPC.rotation = 0;
                                    goal = proj.Center - new Vector2(proj.width / 1.75f, 0);
                                }
                                else
                                {
                                    NPC.rotation = 3 * Pi / 2;
                                    goal = proj.Center + new Vector2(0, proj.height / 1.75f);
                                }
                            }
                            else
                            {
                                Projectile proj = Main.projectile.First(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>() && p.ai[1] == -1 || p.ai[2] == -1);
                                if (proj.ai[1] == -1)
                                {
                                    NPC.rotation = Pi;
                                    goal = proj.Center + new Vector2(proj.width/ 1.75f + NPC.width, 0);
                                }
                                else
                                {
                                    NPC.rotation = Pi / 2;
                                    goal = proj.Center - new Vector2(0, proj.height / 1.75f);
                                }
                            }
                            NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 8f);
                        }
                        else
                        {
                            CurrentPose = Pose.Default;

                            NPC.damage = 0;
                            goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, +75);
                            goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                            #region Movement
                            NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                            NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                            NPC.direction = WhatHand;
                            #endregion
                        }
                        break;
                    case TheOrator.States.DarkBarrage:
                        if (aiCounter > 1100) //Attack Transition
                        {
                            CurrentPose = Pose.Palm;

                            NPC.damage = 0;
                            Vector2 goalDirection = (modOrator.target.Center - orator.Center).SafeNormalize(Vector2.Zero);
                            goalPos = orator.Center + goalDirection * 175f;
                            NPC.direction = -WhatHand;
                            if (WhatHand == -1)
                            {
                                Vector2 rotation = goalDirection.RotatedBy(PiOver2);
                                goalPos -= rotation * Lerp(32, 150, (aiCounter - 1100) / 200f);
                                NPC.rotation = rotation.ToRotation();
                            }
                            else
                            {
                                Vector2 rotation = goalDirection.RotatedBy(-PiOver2);
                                goalPos -= rotation * Lerp(80, 160, (aiCounter - 1100) / 200f);
                                NPC.rotation = rotation.ToRotation();
                            }
                            NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                        }
                        else //Actual Attack
                        {
                            float startSpeed = CalamityWorld.death ? 1f : CalamityWorld.revenge ? 0f : Main.expertMode ? -1f : -2f;

                            if (WhatHand == 1)
                            {
                                if(aiCounter < 270)
                                {
                                    CurrentPose = Pose.Palm;

                                    goalPos = modOrator.target.Center + new Vector2(600f, (float)Math.Sin(aiCounter / 20D) * 320);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = Pi;

                                    if((aiCounter + 45) % 90 == 0 && aiCounter != 0)
                                    {
                                        Vector2 ToTarget = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                    }
                                }
                                else if(aiCounter < 300)
                                {
                                    CurrentPose = Pose.Fist;

                                    goalPos = modOrator.target.Center + new Vector2(600f, 0);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = Pi;
                                }
                                else if(aiCounter < 330)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = false;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 300, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = -Vector2.UnitX * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.Y = (modOrator.target.Center.Y - NPC.Center.Y) / 10f;
                                }
                                else if(attackBool == false && aiCounter < 700)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 330)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = -Vector2.UnitX * 75;
                                    }
                                    NPC.velocity *= 0.95f;
                                    if (aiCounter >= 340 && NPC.velocity.LengthSquared() < 16)
                                        attackBool = true;
                                }
                                else if(aiCounter <= 640)
                                {
                                    CurrentPose = Pose.Palm;

                                    NPC.damage = 0;
                                    goalPos = modOrator.target.Center + new Vector2(-600f, (float)Math.Sin(aiCounter / 20D) * 320);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = WhatHand;
                                    NPC.rotation = 0;

                                    if (aiCounter % 90 == 0)
                                    {
                                        Vector2 ToTarget = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                    }
                                }
                                else if (aiCounter < 700)
                                {
                                    CurrentPose = Pose.Fist;

                                    goalPos = modOrator.target.Center + new Vector2(-600f, 0);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = WhatHand;
                                    NPC.rotation = 0;
                                }
                                else if (aiCounter < 730)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = true;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 700, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = Vector2.UnitX * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.Y = (modOrator.target.Center.Y - NPC.Center.Y) / 10f;
                                }
                                else if (attackBool == true && aiCounter < 900)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 730)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = Vector2.UnitX * 75;
                                    }
                                    NPC.velocity *= 0.95f;
                                    if (aiCounter >= 740 && NPC.velocity.LengthSquared() < 16)
                                        attackBool = false;
                                }
                                else if (aiCounter < 900)
                                {
                                    CurrentPose = Pose.Default;

                                    NPC.damage = 0;
                                    goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, 75);
                                    goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                                    NPC.direction = WhatHand;
                                }
                                else if(aiCounter < 960)
                                {
                                    CurrentPose = Pose.Fist;

                                    goalPos = modOrator.target.Center + new Vector2(32f, -400);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = 3 * PiOver2;
                                }
                                else if (aiCounter < 990)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = false;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 960, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = Vector2.UnitY * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.X = (modOrator.target.Center.X - NPC.Center.X) / 10f;
                                }
                                else if (attackBool == false)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 990)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = Vector2.UnitY * 90;
                                        NPC.direction *= -1;
                                        NPC.rotation += Pi;
                                    }
                                    NPC.velocity *= 0.95f;
                                    Vector2 midPoint = new(NPC.Center.X - 32, NPC.Center.Y);
                                    Tile tile = Main.tile[midPoint.ToTileCoordinates()];
                                    if (aiCounter >= 1000 && (NPC.velocity.LengthSquared() < 784 || (tile.HasTile && (tile.IsTileSolid() || TileID.Sets.Platforms[tile.TileType]) && midPoint.Y > modOrator.target.Center.Y)))
                                    {
                                        ScreenShakeSystem.StartShake(12.5f);
                                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, midPoint);
                                        int projCount = 16;
                                        for (int i = 0; i < projCount; i++)
                                        {
                                            for (int j = 1; j < 8; j++)
                                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), midPoint, (TwoPi / projCount * i).ToRotationVector2() * (i % 2 == 0 ? j * 2 : j * 3), ModContent.ProjectileType<HandRing>(), TheOrator.BoltDamage, 0f);
                                        }
                                        CalamityMod.Particles.Particle pulse = new PulseRing(midPoint, Vector2.Zero, new(253, 189, 53), 0f, 3f, 16);
                                        GeneralParticleHandler.SpawnParticle(pulse);
                                        CalamityMod.Particles.Particle explosion = new DetailedExplosion(midPoint, Vector2.Zero, new(255, 133, 187), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                                        GeneralParticleHandler.SpawnParticle(explosion);

                                        attackBool = true;
                                    }
                                }
                                else
                                {
                                    CurrentPose = Pose.Default;

                                    NPC.damage = 0;
                                    goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, 75);
                                    goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                                    NPC.direction = WhatHand;
                                }
                            }
                            else
                            {
                                if (aiCounter < 390)
                                {
                                    CurrentPose = Pose.Palm;

                                    goalPos = modOrator.target.Center + new Vector2(-600f, (float)Math.Sin(aiCounter / 20D) * -320);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = 0;

                                    if ((aiCounter+90) % 90 == 0)
                                    {
                                        Vector2 ToTarget = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                    }
                                }
                                else if (aiCounter < 420)
                                {
                                    CurrentPose = Pose.Fist;

                                    goalPos = modOrator.target.Center + new Vector2(-600f, 0);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = 0;
                                }
                                else if (aiCounter < 450)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = false;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 420, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = Vector2.UnitX * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.Y = (modOrator.target.Center.Y - NPC.Center.Y) / 10f;
                                }
                                else if (attackBool == false && aiCounter < 700)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 450)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = Vector2.UnitX * 75;
                                    }
                                    NPC.velocity *= 0.95f;
                                    if (aiCounter >= 460 && NPC.velocity.LengthSquared() < 16)
                                        attackBool = true;
                                }
                                else if (aiCounter <= 760)
                                {
                                    CurrentPose = Pose.Palm;

                                    NPC.damage = 0;
                                    goalPos = modOrator.target.Center + new Vector2(600f, (float)Math.Sin(aiCounter / 20D) * -320);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = WhatHand;
                                    NPC.rotation = Pi;

                                    if ((aiCounter + 45) % 90 == 0)
                                    {
                                        Vector2 ToTarget = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-Pi / 6), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                        //Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (ToTarget).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, startSpeed);
                                    }
                                }
                                else if (aiCounter < 820)
                                {
                                    CurrentPose = Pose.Fist;

                                    goalPos = modOrator.target.Center + new Vector2(600f, 0);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = WhatHand;
                                    NPC.rotation = Pi;
                                }
                                else if (aiCounter < 850)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = true;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 820, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = -Vector2.UnitX * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.Y = (modOrator.target.Center.Y - NPC.Center.Y) / 10f;
                                }
                                else if (attackBool == true && aiCounter < 900)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 850)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = -Vector2.UnitX * 75;
                                    }
                                    NPC.velocity *= 0.95f;
                                    if (aiCounter >= 860 && NPC.velocity.LengthSquared() < 16)
                                        attackBool = false;
                                }
                                else if (aiCounter < 900)
                                {
                                    CurrentPose = Pose.Default;

                                    goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, 75);
                                    goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                                    NPC.direction = WhatHand;
                                }
                                else if (aiCounter < 960)
                                {
                                    CurrentPose = Pose.Fist;

                                    NPC.damage = 0;
                                    goalPos = modOrator.target.Center + new Vector2(-32f, -400);

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.direction = -WhatHand;
                                    NPC.rotation = 3 * PiOver2;
                                }
                                else if (aiCounter < 990)
                                {
                                    CurrentPose = Pose.Fist;

                                    attackBool = false;
                                    float reelBackSpeedExponent = 2.6f;
                                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, aiCounter - 960, true);
                                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                    Vector2 reelBackVelocity = Vector2.UnitY * -reelBackSpeed;
                                    NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                                    NPC.velocity.X = (modOrator.target.Center.X - NPC.Center.X) / 10f;
                                }
                                else if (attackBool == false)
                                {
                                    CurrentPose = Pose.Fist;

                                    if (aiCounter == 990)
                                    {
                                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                                        NPC.velocity = Vector2.UnitY * 90;
                                        NPC.direction *= -1;
                                        NPC.rotation += Pi;
                                    }
                                    NPC.velocity *= 0.95f;
                                    if (aiCounter >= 1000 && NPC.velocity.LengthSquared() < 576)
                                        attackBool = true;
                                }
                                else
                                {
                                    CurrentPose = Pose.Default;

                                    NPC.damage = 0;
                                    goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, 75);
                                    goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                                    NPC.direction = WhatHand;
                                }
                            }
                        }
                        break;
                    case TheOrator.States.DarkMonster:
                        CurrentPose = Pose.Default;

                        Vector2 goalDir = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        goalPos = orator.Center + goalDir * -48f;
                        NPC.direction = -WhatHand;
                        if (WhatHand == 1)
                        {
                            Vector2 rotation = goalDir.RotatedBy(PiOver2);
                            goalPos += rotation * 128;
                            NPC.rotation = rotation.RotatedBy(3 * Pi / 2).ToRotation();
                        }
                        else
                        {
                            Vector2 rotation = goalDir.RotatedBy(3 * Pi / 2);
                            goalPos += rotation * 128;
                            NPC.rotation = rotation.RotatedBy(Pi / 2).ToRotation();
                        }
                        NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 3f);
                        break;
                    default:
                        CurrentPose = Pose.Default;

                        NPC.damage = 0;
                        goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, +75);
                        goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                        #region Movement
                        NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                        NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                        NPC.direction = WhatHand;
                        #endregion
                        break;
                }
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center - (NPC.rotation.ToRotationVector2().RotatedBy(CurrentPose == Pose.Palm ? -PiOver2 * NPC.direction : 0) * (NPC.width / 1.5f)) + (NPC.rotation.ToRotationVector2().RotatedBy(PiOver2 - (CurrentPose == Pose.Palm ? PiOver2 * NPC.direction : 0)) * Main.rand.NextFloat(-16f, 16f)), (NPC.rotation.ToRotationVector2().RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4) - (CurrentPose == Pose.Palm ? PiOver2 * NPC.direction : 0)) * Main.rand.NextFloat(2f, 6f)), Main.rand.NextFloat(20f, 40f));
            }
            effectCounter++;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            base.HitEffect(hit);

            if (NPC.life <= 0)
            {
                NPC.dontTakeDamage = true;               
                if (OratorIndex != -1)
                    Main.npc[OratorIndex].As<TheOrator>().AIState = TheOrator.States.PhaseChange;
                
                if (NPC.realLife != -1)
                    Main.npc[NPC.realLife].life = 1;
                NPC.life = 1;
            }
        }
        public override void FindFrame(int frameHeight)
        {            
            NPC.frame.Width = ModContent.Request<Texture2D>(this.Texture).Width() / 3;
            switch (CurrentPose)
            {
                case Pose.Fist:
                    NPC.frame.X = NPC.frame.Width;
                    NPC.frame.Y = 0;
                    return;
                case Pose.Palm:
                    NPC.frame.X = NPC.frame.Width * 2;
                    NPC.frame.Y = 0;
                    return;
                default:
                    NPC.frame.X = 0; 
                    break;
            }
            
            if (deadCounter > 300)
                return;            
            NPC.frameCounter++;
            if (deadCounter > 0 && deadCounter < 300)
                NPC.frameCounter += ((deadCounter / 300f) * 4f);
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 6)
                    NPC.frame.Y = 0;            
            }            
        }
        public override bool CheckActive() => false;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            Vector2 origin = NPC.frame.Size() * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.direction == -1)
                spriteEffects = SpriteEffects.FlipVertically;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
            return false;
        }
        Rectangle cuffFrame = new(0, 0, 150, 114);
        int cuffCounter = 0;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {            
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/NPCs/Enemies/OratorHandCuffs").Value;
            cuffFrame.Width = texture.Width;
            cuffFrame.Height = texture.Height / 6;
            cuffCounter++;

            if (cuffCounter >= 16)
            {
                cuffCounter = 0;
                cuffFrame.Y += cuffFrame.Height;
                if (cuffFrame.Y >= cuffFrame.Height * 6)
                    cuffFrame.Y = 0;
            }
            
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            Vector2 origin = cuffFrame.Size() * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.direction == -1)
                spriteEffects = SpriteEffects.FlipVertically;
            spriteBatch.Draw(texture, drawPosition, cuffFrame, NPC.GetAlpha(drawColor), NPC.rotation - (CurrentPose == Pose.Palm ? PiOver2 * NPC.direction : 0), origin, NPC.scale, spriteEffects, 0f);
        }
    }
}

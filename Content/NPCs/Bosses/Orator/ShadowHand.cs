using CalamityMod.World;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Common.Utils;
using Windfall.Content.Items.Lore;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Content.NPCs.Bosses.Orator
{
    public class ShadowHand : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/Enemies/ShadowHand";
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
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(ShadowHand)}")),
        ]);
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 64;
            NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 500 : CalamityWorld.death ? 420 : CalamityWorld.revenge ? 350 : Main.expertMode ? 240 : 180);
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lifeMax = 1200;
            NPC.knockBackResist = 0f;
            NPC.scale = 1.25f;
            NPC.HitSound = SoundID.DD2_LightningBugHurt with { Volume = 0.5f };
            NPC.DeathSound = SoundID.DD2_LightningBugDeath;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = true;
            NPC.Calamity().canBreakPlayerDefense = true;
            NPC.Opacity = 0f;
        }
        internal enum AIState
        {
            Spawning,
            OnBoss,
            Hunting,
            Dashing,
            Globbing,
            Aiming,
            Sacrifice,
        }
        internal AIState CurrentAI
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
        private int aiCounter
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        Vector2 toTarget = Vector2.Zero;
        bool attackBool = false;
        private enum Pose
        {
            Default,
            FingerGun,
            Fist,
            Palm
        }
        private Pose CurrentPose = Pose.Default;
        public override void OnSpawn(IEntitySource source)
        {
            NPC.Opacity = 0f;
            CurrentAI = AIState.Spawning;
            NPC.velocity = Main.rand.NextFloat(0, TwoPi).ToRotationVector2() * Main.rand.Next(10, 15);
            NPC.rotation = NPC.velocity.ToRotation();
            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(7f, 7f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
        }
        bool movingBackward = false;
        public override void AI()
        {
            NPC.Opacity = 1f;
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            NPC Orator = null;
            if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
            if (CurrentAI <= AIState.OnBoss)
            {
                NPC.dontTakeDamage = true;
                NPC.damage = 0;
            }
            else
            {
                NPC.dontTakeDamage = false;
                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 500 : CalamityWorld.death ? 420 : CalamityWorld.revenge ? 350 : Main.expertMode ? 240 : 180);
            }            
            #region Despawning

            if (Orator == null)
            {
                toTarget = target.Center - NPC.Center;
                NPC.velocity -= toTarget.SafeNormalize(Vector2.UnitX);
                //NPC.rotation = (target.Center - NPC.Center).ToRotation() + Pi;
                //NPC.spriteDirection = NPC.direction * -1;
                if (toTarget.Length() > 800)
                    NPC.active = false;
                return;
            }
            else if (Orator.ai[0] != 2 && Orator.ai[0] != 0)
            {
                if (CurrentAI != AIState.Sacrifice)
                    aiCounter = 0;
                CurrentAI = AIState.Sacrifice;
            }

            //testing code :P
            //if (CurrentAI == AIState.OnBoss || CurrentAI == AIState.Spawning)
            //CurrentAI = AIState.Hunting;
            #endregion

            switch (CurrentAI)
            {
                case AIState.Spawning:
                    NPC.velocity *= 0.98f;

                    Dust dust = Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * Main.rand.NextFloat(-16, 16) + new Vector2(-54, 0).RotatedBy(NPC.rotation), DustID.RainbowTorch);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                    dust.noGravity = true;
                    dust.color = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);

                    if (NPC.velocity.Length() < 2)
                    {
                        CurrentAI = AIState.OnBoss;
                        aiCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    }

                    SetNPCDirection();
                    break;
                case AIState.OnBoss:
                    NPC[] inactiveHands = Main.npc.Where(n => n != null && n.active && n.type == NPC.type && n.ai[0] == (int)AIState.OnBoss).ToArray();
                    int myIndex = -1;
                    for(int i = 0; i < inactiveHands.Length; i++)
                    {
                        if (inactiveHands[i].whoAmI == NPC.whoAmI)
                            myIndex = i;
                    }
                    int WhatHand = (myIndex % 2 == 0 ? 1 : -1);
                    int height = (int)Math.Floor(myIndex / 2f) + 1;

                    Vector2 goalPos = Orator.Center + Orator.velocity + new Vector2((124) * WhatHand, 75 - (75 * height));
                    goalPos.Y += (float)Math.Sin(aiCounter / 20f) * (16f + height);

                    #region Movement
                    NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                    NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * (height + 1) * WhatHand);
                    NPC.direction = WhatHand;
                    #endregion
                    break;
                case AIState.Hunting:
                    CurrentPose = Pose.Default;

                    #region Movement
                    Vector2 homeInVector = target.Center - NPC.Center;
                    float targetDist = homeInVector.Length();
                    homeInVector.Normalize();
                    if (targetDist > 300f)
                    {
                        float velocity = 12f;
                        NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                        movingBackward = NPC.velocity.LengthSquared() < NPC.oldVelocity.LengthSquared();
                    }
                    else
                    {
                        if (targetDist < 250f)
                        {
                            float velocity = -12f;
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                            movingBackward = NPC.velocity.LengthSquared() > NPC.oldVelocity.LengthSquared();
                        }
                        else
                        {
                            NPC.velocity *= 0.9f;
                        }
                    }
                    NPC.rotation = (target.Center - NPC.Center).ToRotation();
                    if (NPC.rotation + Pi > Pi / 2 && NPC.rotation + Pi < 3 * Pi / 2)
                        NPC.rotation = 0 + PiOver4 * (NPC.velocity.Length() / (10 * (movingBackward ? -1 : 1)));
                    else
                        NPC.rotation = Pi - PiOver4 * (NPC.velocity.Length() / (10 * (movingBackward ? -1 : 1)));
                    #endregion

                    #region Attack
                    Vector2 toTarget = target.Center - NPC.Center;
                    attackBool = false;
                    if ((aiCounter > 120 && Main.rand.NextBool(60)) || toTarget.Length() > 800f)
                    {
                        NPC.rotation = toTarget.ToRotation();
                        if (Main.rand.NextBool(5) || toTarget.Length() > 600f)
                        {
                            attackBool = true;
                            CurrentAI = AIState.Dashing;
                            aiCounter = 0;
                            return;
                        }
                        else
                        {
                            if (Main.rand.NextBool(3))
                            {
                                attackBool = NPC.position.X > target.position.X;
                                NPC.velocity = Vector2.Zero;
                                CurrentAI = AIState.Globbing;
                                aiCounter = -15;
                                return;
                            }
                            else
                            {
                                CurrentPose = Pose.FingerGun;
                                CurrentAI = AIState.Aiming;
                                aiCounter = 0;
                                return;
                            }
                        }
                    }
                    #endregion

                    SetNPCDirection();
                    break;
                case AIState.Dashing:
                    CurrentPose = Pose.Fist;

                    if(aiCounter <= 30)
                    {
                        toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0, 30, aiCounter, true);
                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = toTarget * -reelBackSpeed;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);

                        SetNPCDirection();
                    }
                    else if (aiCounter == 31)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, NPC.Center);
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * -48;

                        SetNPCDirection();
                    }
                    else
                    {
                        NPC.velocity *= 0.96f;

                        NPC.velocity = NPC.velocity.RotateTowards((target.Center - NPC.Center).ToRotation(), 0.02f);
                        NPC.rotation = NPC.velocity.ToRotation();

                        dust = Dust.NewDustPerfect(NPC.Center + Vector2.UnitY * Main.rand.NextFloat(-16, 16) + new Vector2(-54, 0).RotatedBy(NPC.rotation), DustID.RainbowTorch);
                        dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                        dust.noGravity = true;
                        dust.color = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);

                        if (NPC.velocity.LengthSquared() < 64f)
                        {
                            CurrentAI = AIState.Hunting;
                            aiCounter = 0;
                            NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        }
                    }
                    break;
                case AIState.Globbing:
                    Vector2 baseAngle;
                    float rotation;
                    if (attackBool)
                    {
                        baseAngle = 0f.ToRotationVector2();
                        rotation = -Pi / 8;
                        NPC.direction = -1;
                    }
                    else
                    {
                        baseAngle = (Pi).ToRotationVector2();
                        rotation = Pi / 8;
                        NPC.direction = 1;
                    }
                    if (aiCounter >= 0)
                    {
                        CurrentPose = Pose.Palm;
                        if (aiCounter > 0 && aiCounter % 5 == 0 && aiCounter <= 35)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, NPC.Center);
                            Vector2 myAngle = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * Math.Ceiling((double)aiCounter / 5));
                            for (int i = 0; i < 10; i++)
                                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + myAngle * 40, myAngle.RotatedByRandom(Pi / 6) * Main.rand.NextFloat(0f, 15f), 20 * Main.rand.NextFloat(1f, 2f));
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, myAngle * 15, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                        }
                        NPC.rotation = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * ((float)aiCounter / 5) - (PiOver2 * NPC.direction)).ToRotation();
                        if (aiCounter >= 60)
                            CurrentAI = AIState.Hunting;
                    }
                    else
                        NPC.rotation = Lerp(baseAngle.ToRotation() + (Pi * NPC.direction), baseAngle.ToRotation() - (PiOver2 * NPC.direction), SineOutEasing((aiCounter + 15) / 15f, 1));
                    break;
                case AIState.Aiming:
                    #region Movement
                    if (aiCounter < 90 || aiCounter > 120)
                    {
                        homeInVector = target.Center - NPC.Center;
                        targetDist = homeInVector.Length();
                        homeInVector.Normalize();
                        if (targetDist > 300f)
                        {
                            float velocity = 24f;
                            if (aiCounter > 120)
                                velocity /= 2f;
                            NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                        }
                        else
                        {
                            if (targetDist < 250f)
                            {
                                float velocity = -24f;
                                if (aiCounter > 120)
                                    velocity /= 2f;
                                NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                            }
                            else
                            {
                                NPC.velocity *= 0.9f;
                            }
                        }
                        if(aiCounter < 90)
                            NPC.rotation = (target.Center - NPC.Center).ToRotation();
                    }
                    #endregion

                    if(aiCounter >= 90)
                    {
                        if (aiCounter == 90)
                        {
                            SoundEngine.PlaySound(SoundID.DD2_OgreSpit, NPC.Center);
                            toTarget = target.Center - NPC.Center;
                            NPC.velocity = toTarget.SafeNormalize(Vector2.Zero) * -12;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                if (Main.rand.NextBool())
                                {
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(-PiOver4), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                }
                                else
                                {
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / 8), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / -8), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 15);
                                }

                            SetNPCDirection();
                        }
                        else if(aiCounter <= 120)
                        {
                            NPC.velocity *= 0.95f;

                            NPC.rotation = Lerp((target.Center - NPC.Center).ToRotation(), (target.Center - NPC.Center).ToRotation() + ((PiOver2 + PiOver4) * -NPC.direction), CalamityUtils.ExpOutEasing((aiCounter - 90) / 30f, 1));

                            if (aiCounter == 120)
                            {
                                NPC.ai[2] = NPC.rotation;
                                attackBool = Main.rand.NextBool(2);
                            }
                        }
                        else
                        {
                            if(attackBool)
                                NPC.rotation = Lerp(NPC.ai[2], (target.Center - NPC.Center).ToRotation(), (aiCounter - 120) / 30f);
                            else
                                NPC.rotation = Lerp(NPC.ai[2], NPC.direction == -1 ? (Pi * (target.Center.Y > NPC.Center.Y ? 1 : -1)) : 0, (aiCounter - 120) / 30f);

                            #region Movement
                            homeInVector = target.Center - NPC.Center;
                            targetDist = homeInVector.Length();
                            homeInVector.Normalize();
                            if (targetDist > 300f)
                            {
                                float velocity = 12f;
                                NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                            }
                            else
                            {
                                if (targetDist < 250f)
                                {
                                    float velocity = -12f;
                                    NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                                }
                                else
                                {
                                    NPC.velocity *= 0.9f;
                                }
                            }
                            #endregion

                            if (aiCounter == 150)
                            {
                                if(attackBool)
                                {
                                    CurrentAI = AIState.Aiming;
                                    aiCounter = 45;
                                }
                                else
                                { 
                                    CurrentAI = AIState.Hunting;
                                    aiCounter = 0;
                                }
                            }
                        }
                    }
                    else
                        SetNPCDirection();
                    break;
                case AIState.Sacrifice:

                    #region Movement
                    toTarget = Orator.Center - NPC.Center;
                    NPC.velocity = toTarget.SafeNormalize(Vector2.UnitY) * aiCounter / 5;
                    if (NPC.velocity.Length() > 20)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 20;
                    NPC.rotation = toTarget.ToRotation();

                    SetNPCDirection();
                    #endregion

                    #region Healing
                    if (NPC.Hitbox.Intersects(Orator.Hitbox))
                    {
                        if (Main.npc.Any(n => n != null && n.active && n.type == ModContent.NPCType<OratorHand>()))
                        {
                            NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == ModContent.NPCType<OratorHand>());
                            mainHand.life += mainHand.lifeMax / 100;
                            if (mainHand.life > mainHand.lifeMax)
                                mainHand.life = mainHand.lifeMax;
                        }
                        else
                        { 
                            Orator.life += Orator.lifeMax / 100;
                            if (Orator.life > Orator.lifeMax)
                                Orator.life = Orator.lifeMax;
                        }
                        CombatText.NewText(NPC.Hitbox, Color.LimeGreen, Orator.lifeMax / 100);
                        if (Orator.ModNPC is TheOrator orator)
                            TheOrator.noSpawnsEscape = false;
                        SoundEngine.PlaySound(SoundID.AbigailUpgrade, NPC.Center);
                        for (int i = 0; i <= 20; i++)
                            EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(6f, 6f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                        NPC.active = false;
                    }
                    #endregion

                    break;
            }
            aiCounter++;

            if (CurrentAI != AIState.OnBoss)
            {
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + new Vector2(-32, 0).RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(NPC.rotation) * -8, NPC.scale * 34);
                if (Main.rand.NextBool(3))
                    EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + Main.rand.NextVector2Circular(2, 2) + new Vector2(-32, 0).RotatedBy(NPC.rotation), Vector2.UnitX.RotatedBy(NPC.rotation + Main.rand.NextFloat(-0.5f, 0.5f)) * -Main.rand.NextFloat(6f, 8f), NPC.scale * Main.rand.NextFloat(30f, 40f));
            }
            //Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
        public override void OnKill()
        {
            for (int i = 0; i <= 25; i++)
                EmpyreanMetaball.SpawnDefaultParticle(NPC.Center, Main.rand.NextVector2Circular(8f, 8f), 40 * Main.rand.NextFloat(1.75f, 2.5f));
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = ModContent.Request<Texture2D>(this.Texture).Width() / 4;
            switch (CurrentPose)
            {
                case Pose.FingerGun:
                    NPC.frame.X = NPC.frame.Width;
                    NPC.frame.Y = 0;
                    return;
                case Pose.Fist:
                    NPC.frame.X = NPC.frame.Width * 2;
                    NPC.frame.Y = 0;
                    return;
                case Pose.Palm:
                    NPC.frame.X = NPC.frame.Width * 3;
                    NPC.frame.Y = 0;
                    return;
                default:
                    NPC.frame.X = 0;
                    break;
            }

            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * (Main.npcFrameCount[Type] - 1))
                    NPC.frame.Y = 0;
            }
        }
        public override bool CheckActive() => false;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
        public void DrawSelf(Vector2 drawPosition, Color color, float rotation)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if(NPC.direction == -1)
                spriteEffects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, color, rotation, NPC.frame.Size() * 0.5f, NPC.scale, spriteEffects, 0);
        }
        private void SetNPCDirection()
        {
            if (!(NPC.rotation + Pi > Pi / 2 && NPC.rotation + Pi < 3 * Pi / 2) && CurrentAI != AIState.Globbing && CurrentAI != AIState.OnBoss)
                NPC.direction = -1;
            else
                NPC.direction = 1;
        }
    }
}

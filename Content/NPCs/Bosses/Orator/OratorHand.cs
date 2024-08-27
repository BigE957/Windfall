using CalamityMod.World;
using Luminance.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Windfall.Common.Systems;
using Windfall.Content.Items.Lore;
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
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(OratorHand)}")),
        });
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 75;
            NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 500 : CalamityWorld.death ? 420 : CalamityWorld.revenge ? 350 : Main.expertMode ? 240 : 180);
            NPC.DR_NERD(0.10f);
            NPC.defense = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.LifeMaxNERB(Main.masterMode ? 120000 : Main.expertMode ? 80000 : 70000, 100000);
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
            if (WhatHand == 0)
            {
                if (Main.npc.Where(n => n != null && n.active && n.type == NPC.type).Count() > 1)
                    WhatHand = -1;
                else
                    WhatHand = 1;
            }
            NPC.life /= 2;
            NPC.lifeMax /= 2;
        }
        public override bool PreAI()
        {
            NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type);
            if (mainHand.whoAmI != NPC.whoAmI)
                NPC.realLife = mainHand.whoAmI;

            return true;
        }
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

            if (NPC.AnyNPCs(ModContent.NPCType<ShadowHand>()))
                NPC.DR_NERD(0.1f + 0.1f * Main.npc.Where(n => n.type == ModContent.NPCType<ShadowHand>()).Count());
            else
                NPC.DR_NERD(0.1f);

            if (modOrator.AIState == TheOrator.States.Spawning || (modOrator.AIState == TheOrator.States.DarkSlice && aiCounter >= -30))
            {
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
                switch (modOrator.AIState)
                {
                    case TheOrator.States.DarkSlice:
                        Vector2 goalPos = Vector2.Zero;
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
                                goalPos = Main.LocalPlayer.Center + (direction * 500);

                                #region Movement
                                NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                                NPC.rotation = NPC.DirectionTo(Main.LocalPlayer.Center).ToRotation();
                                if (NPC.Center.X > Main.LocalPlayer.Center.X)
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
                                        direction = (Main.LocalPlayer.Center - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction);
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
                                    NPC subHand = Main.npc.Last(n => n != null && n.active && n.type == NPC.type);
                                    if (NPC.Hitbox.Intersects(subHand.Hitbox) && !attackBool)
                                    {
                                        NPC.Center -= NPC.velocity * 1.5f;
                                        subHand.Center -= subHand.velocity * 1.5f;

                                        Vector2 midPoint = NPC.Center + ((subHand.Center - NPC.Center) / 2);

                                        ScreenShakeSystem.StartShake(12.5f);
                                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, midPoint);
                                        for (int i = 0; i < 24; i++)
                                        {
                                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), midPoint, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0);
                                        }
                                        CalamityMod.Particles.Particle pulse = new PulseRing(midPoint, Vector2.Zero, Color.Teal, 0f, 3f, 16);
                                        GeneralParticleHandler.SpawnParticle(pulse);
                                        CalamityMod.Particles.Particle explosion = new DetailedExplosion(midPoint, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                                        GeneralParticleHandler.SpawnParticle(explosion);

                                        NPC.velocity = Vector2.Zero;
                                        subHand.velocity = Vector2.Zero;

                                        attackBool = true;
                                        return;
                                    }
                                    else if(attackBool)
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
                                NPC mainHand = Main.npc.First(n => n != null && n.active && n.type == NPC.type);
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
                                    else if(!mainHand.As<OratorHand>().attackBool)
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
                        break;
                    case TheOrator.States.DarkStorm:
                        Vector2 goal = new(orator.Center.X + (600 * WhatHand), orator.Center.Y);
                        goal.X += (float)Math.Sin(aiCounter / 20f) * 48f * WhatHand;

                        #region Movement
                        NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 10f);
                        NPC.rotation = Pi;
                        if (WhatHand == 1)
                            NPC.rotation += Pi;
                        NPC.direction = -WhatHand;
                        #endregion

                        break;
                    case TheOrator.States.DarkCollision:
                        if(Main.projectile.Any(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>()))
                        {
                            NPC.direction = WhatHand;
                            if (WhatHand == 1)
                            {
                                Projectile proj = Main.projectile.First(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>() && p.ai[1] == 1 || p.ai[2] == 1);
                                if (proj.ai[1] == 1)
                                {
                                    NPC.rotation = 0;
                                    goal = proj.Center - new Vector2(proj.width / 1.5f, 0);
                                }
                                else
                                {
                                    NPC.rotation = 3 * Pi / 2;
                                    goal = proj.Center + new Vector2(0, proj.height / 1.5f);
                                }
                            }
                            else
                            {
                                Projectile proj = Main.projectile.First(p => p != null && p.active && p.type == ModContent.ProjectileType<DarkCoalescence>() && p.ai[1] == -1 || p.ai[2] == -1);
                                if (proj.ai[1] == -1)
                                {
                                    NPC.rotation = Pi;
                                    goal = proj.Center + new Vector2(proj.width/ 1.5f + NPC.width, 0);
                                }
                                else
                                {
                                    NPC.rotation = Pi / 2;
                                    goal = proj.Center - new Vector2(0, proj.height / 1.5f);
                                }
                            }
                            NPC.velocity = (goal - NPC.Center).SafeNormalize(Vector2.Zero) * ((goal - NPC.Center).Length() / 8f);
                        }
                        break;
                    case TheOrator.States.DarkBarrage:
                        //
                        if (aiCounter > 1100)
                        {
                            Vector2 goalDirection = (modOrator.target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                            goalPos = orator.Center + goalDirection * 150f;
                            NPC.direction = -WhatHand;
                            if (WhatHand == 1)
                            {
                                Vector2 rotation = goalDirection.RotatedBy(PiOver2);
                                goalPos += rotation * 128;
                                NPC.rotation = rotation.RotatedBy(3 * Pi / 2).ToRotation();
                            }
                            else
                            {
                                Vector2 rotation = goalDirection.RotatedBy(3 * Pi / 2);
                                goalPos += rotation * 128;
                                NPC.rotation = rotation.RotatedBy(Pi / 2).ToRotation();
                            }
                            NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                        }
                        else 
                        {
                            goalPos = orator.Center + orator.velocity + new Vector2(124 * WhatHand, +75);
                            goalPos.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                            NPC.velocity = (goalPos - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPos - NPC.Center).Length() / 10f);
                            NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                            NPC.direction = WhatHand;
                        }
                        break;
                    default:
                        Vector2 goalPosition = orator.Center + orator.velocity + new Vector2(124 * WhatHand, +75);
                        goalPosition.Y += (float)Math.Sin(aiCounter / 20f) * 16f;

                        #region Movement
                        NPC.velocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * ((goalPosition - NPC.Center).Length() / 10f);
                        NPC.rotation = (-3 * Pi / 2) - (Pi / 8 * WhatHand);
                        NPC.direction = WhatHand;
                        #endregion
                        break;
                }
            }
            effectCounter++;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
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
    }
}

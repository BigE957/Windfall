using CalamityMod.Dusts;
using CalamityMod.DataStructures;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Windfall.Content.Systems;

namespace Windfall.Content.NPCs
{
    public class CalamityAIs
    {
        public static int despawnTimer = 0;
        public static void CataclysmAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            CalamityGlobalNPC.cataclysm = npc.whoAmI;

            // Emit light
            Lighting.AddLight((int)((npc.position.X + npc.width / 2) / 16f), (int)((npc.position.Y + npc.height / 2) / 16f), 1f, 0f, 0f);

            bool death = CalamityWorld.death;
            bool revenge = CalamityWorld.revenge;
            bool expertMode = Main.expertMode;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            Player player = Main.player[npc.target];

            float calCloneBroPlayerXDist = npc.position.X + npc.width / 2 - player.position.X - player.width / 2;
            float calCloneBroPlayerYDist = npc.position.Y + npc.height - 59f - player.position.Y - player.height / 2;
            float calCloneBroRotation = (float)Math.Atan2(calCloneBroPlayerYDist, calCloneBroPlayerXDist) + MathHelper.PiOver2;
            if (calCloneBroRotation < 0f)
                calCloneBroRotation += MathHelper.TwoPi;
            else if (calCloneBroRotation > MathHelper.TwoPi)
                calCloneBroRotation -= MathHelper.TwoPi;

            float calCloneBroRotationSpeed = 0.15f;
            if (npc.rotation < calCloneBroRotation)
            {
                if (calCloneBroRotation - npc.rotation > MathHelper.Pi)
                    npc.rotation -= calCloneBroRotationSpeed;
                else
                    npc.rotation += calCloneBroRotationSpeed;
            }
            else if (npc.rotation > calCloneBroRotation)
            {
                if (npc.rotation - calCloneBroRotation > MathHelper.Pi)
                    npc.rotation += calCloneBroRotationSpeed;
                else
                    npc.rotation -= calCloneBroRotationSpeed;
            }

            if (npc.rotation > calCloneBroRotation - calCloneBroRotationSpeed && npc.rotation < calCloneBroRotation + calCloneBroRotationSpeed)
                npc.rotation = calCloneBroRotation;
            if (npc.rotation < 0f)
                npc.rotation += MathHelper.TwoPi;
            else if (npc.rotation > MathHelper.TwoPi)
                npc.rotation -= MathHelper.TwoPi;
            if (npc.rotation > calCloneBroRotation - calCloneBroRotationSpeed && npc.rotation < calCloneBroRotation + calCloneBroRotationSpeed)
                npc.rotation = calCloneBroRotation;

            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    despawnTimer++;
                    if (despawnTimer > 60)
                    {
                        npc.active = false;
                        despawnTimer = 0;
                    }
                    if (npc.ai[1] != 0f)
                    {
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }

                    return;
                }
            }
            else
            {
                despawnTimer = 0;
            }

            if (npc.ai[1] == 0f)
            {
                float calCloneBroProjAttackMaxSpeed = 5f;
                float calCloneBroProjAttackAccel = 0.1f;
                calCloneBroProjAttackMaxSpeed += 2f * 1;
                calCloneBroProjAttackAccel += 0.06f * 1;

                if (Main.getGoodWorld)
                {
                    calCloneBroProjAttackMaxSpeed *= 1.15f;
                    calCloneBroProjAttackAccel *= 1.15f;
                }

                int calCloneBroProjAttackDirection = 1;
                if (npc.position.X + npc.width / 2 < player.position.X + player.width)
                    calCloneBroProjAttackDirection = -1;

                Vector2 calCloneBroProjLocation = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                float calCloneBroProjTargetX = player.position.X + player.width / 2 + calCloneBroProjAttackDirection * 180 - calCloneBroProjLocation.X;
                float calCloneBroProjTargetY = player.position.Y + player.height / 2 - calCloneBroProjLocation.Y;
                float calCloneBroProjTargetDist = (float)Math.Sqrt(calCloneBroProjTargetX * calCloneBroProjTargetX + calCloneBroProjTargetY * calCloneBroProjTargetY);

                if (expertMode)
                {
                    if (calCloneBroProjTargetDist > 300f)
                        calCloneBroProjAttackMaxSpeed += 0.5f;
                    if (calCloneBroProjTargetDist > 400f)
                        calCloneBroProjAttackMaxSpeed += 0.5f;
                    if (calCloneBroProjTargetDist > 500f)
                        calCloneBroProjAttackMaxSpeed += 0.55f;
                    if (calCloneBroProjTargetDist > 600f)
                        calCloneBroProjAttackMaxSpeed += 0.55f;
                    if (calCloneBroProjTargetDist > 700f)
                        calCloneBroProjAttackMaxSpeed += 0.6f;
                    if (calCloneBroProjTargetDist > 800f)
                        calCloneBroProjAttackMaxSpeed += 0.6f;
                }

                calCloneBroProjTargetDist = calCloneBroProjAttackMaxSpeed / calCloneBroProjTargetDist;
                calCloneBroProjTargetX *= calCloneBroProjTargetDist;
                calCloneBroProjTargetY *= calCloneBroProjTargetDist;

                if (npc.velocity.X < calCloneBroProjTargetX)
                {
                    npc.velocity.X += calCloneBroProjAttackAccel;
                    if (npc.velocity.X < 0f && calCloneBroProjTargetX > 0f)
                        npc.velocity.X += calCloneBroProjAttackAccel;
                }
                else if (npc.velocity.X > calCloneBroProjTargetX)
                {
                    npc.velocity.X -= calCloneBroProjAttackAccel;
                    if (npc.velocity.X > 0f && calCloneBroProjTargetX < 0f)
                        npc.velocity.X -= calCloneBroProjAttackAccel;
                }
                if (npc.velocity.Y < calCloneBroProjTargetY)
                {
                    npc.velocity.Y += calCloneBroProjAttackAccel;
                    if (npc.velocity.Y < 0f && calCloneBroProjTargetY > 0f)
                        npc.velocity.Y += calCloneBroProjAttackAccel;
                }
                else if (npc.velocity.Y > calCloneBroProjTargetY)
                {
                    npc.velocity.Y -= calCloneBroProjAttackAccel;
                    if (npc.velocity.Y > 0f && calCloneBroProjTargetY < 0f)
                        npc.velocity.Y -= calCloneBroProjAttackAccel;
                }

                npc.ai[2] += 1f;
                if (npc.ai[2] >= 240f - (death ? 120f * (1f - lifeRatio) : 0f))
                {
                    npc.TargetClosest();
                    npc.ai[1] = 1f;
                    npc.ai[2] = 0f;
                    npc.target = 255;
                    npc.netUpdate = true;
                }

                bool fireDelay = npc.ai[2] > 120f || npc.life < npc.lifeMax * 0.9;
                if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height) && fireDelay)
                {
                    npc.localAI[2] += 1f;
                    if (npc.localAI[2] > 22f)
                    {
                        npc.localAI[2] = 0f;
                        SoundEngine.PlaySound(SoundID.Item34, npc.Center);
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[1] += 3f;
                        if (revenge)
                            npc.localAI[1] += 1f;

                        if (npc.localAI[1] > 12f)
                        {
                            npc.localAI[1] = 0f;
                            float calCloneBroProjSpeed = NPC.AnyNPCs(ModContent.NPCType<Catastrophe>()) ? 4f : 6f;
                            calCloneBroProjSpeed += 1;
                            int type = ModContent.ProjectileType<BrimstoneFire>();
                            int projectileDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 234 : CalamityWorld.death ? 176 : CalamityWorld.revenge ? 160 : Main.expertMode ? 140 : 80);
                            calCloneBroProjLocation = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                            calCloneBroProjTargetX = player.position.X + player.width / 2 - calCloneBroProjLocation.X;
                            calCloneBroProjTargetY = player.position.Y + player.height / 2 - calCloneBroProjLocation.Y;
                            calCloneBroProjTargetDist = (float)Math.Sqrt(calCloneBroProjTargetX * calCloneBroProjTargetX + calCloneBroProjTargetY * calCloneBroProjTargetY);
                            calCloneBroProjTargetDist = calCloneBroProjSpeed / calCloneBroProjTargetDist;
                            calCloneBroProjTargetX *= calCloneBroProjTargetDist;
                            calCloneBroProjTargetY *= calCloneBroProjTargetDist;
                            calCloneBroProjTargetY += npc.velocity.Y * 0.5f;
                            calCloneBroProjTargetX += npc.velocity.X * 0.5f;
                            calCloneBroProjLocation.X -= calCloneBroProjTargetX;
                            calCloneBroProjLocation.Y -= calCloneBroProjTargetY;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), calCloneBroProjLocation.X, calCloneBroProjLocation.Y, calCloneBroProjTargetX, calCloneBroProjTargetY, type, projectileDamage, 0f, Main.myPlayer, 0f, 0f);
                        }
                    }
                }
            }
            else
            {
                if (npc.ai[1] == 1f)
                {
                    SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                    npc.rotation = calCloneBroRotation;

                    float calCloneBroChargeSpeed = 14f + (death ? 4f * (1f - lifeRatio) : 0f);
                    calCloneBroChargeSpeed += 3f * 1;
                    if (expertMode)
                        calCloneBroChargeSpeed += 2f;
                    if (revenge)
                        calCloneBroChargeSpeed += 2f;
                    if (Main.getGoodWorld)
                        calCloneBroChargeSpeed *= 1.25f;

                    Vector2 calCloneBroChargeCenter = npc.Center;
                    float calCloneBroChargeTargetXDist = player.Center.X - calCloneBroChargeCenter.X;
                    float calCloneBroChargeTargetYDist = player.Center.Y - calCloneBroChargeCenter.Y;
                    float calCloneBroChargeTargetDistance = (float)Math.Sqrt(calCloneBroChargeTargetXDist * calCloneBroChargeTargetXDist + calCloneBroChargeTargetYDist * calCloneBroChargeTargetYDist);
                    calCloneBroChargeTargetDistance = calCloneBroChargeSpeed / calCloneBroChargeTargetDistance;
                    npc.velocity.X = calCloneBroChargeTargetXDist * calCloneBroChargeTargetDistance;
                    npc.velocity.Y = calCloneBroChargeTargetYDist * calCloneBroChargeTargetDistance;
                    npc.ai[1] = 2f;

                    if (Main.zenithWorld)
                    {
                        SoundEngine.PlaySound(SupremeCalamitas.BrimstoneShotSound, npc.Center);
                        int type = ModContent.ProjectileType<BrimstoneBarrage>();
                        int damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 234 : CalamityWorld.death ? 176 : CalamityWorld.revenge ? 160 : Main.expertMode ? 140 : 80);
                        int totalProjectiles = death ? 10 : revenge ? 8 : expertMode ? 6 : 4;
                        float radians = MathHelper.TwoPi / totalProjectiles;
                        float velocity = 5f;
                        Vector2 spinningPoint = new Vector2(0f, -velocity);
                        for (int k = 0; k < totalProjectiles; k++)
                        {
                            Vector2 velocity2 = spinningPoint.RotatedBy(radians * k);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity2, type, damage, 0f, Main.myPlayer, 0f, 1f);
                        }

                        for (int i = 0; i < 6; i++)
                            Dust.NewDust(npc.position + npc.velocity, npc.width, npc.height, (int)CalamityDusts.Brimstone, 0f, 0f);
                    }
                    return;
                }

                if (npc.ai[1] == 2f)
                {
                    npc.ai[2] += 1f + (death ? 0.5f * (1f - lifeRatio) : 0f);
                    if (expertMode)
                        npc.ai[2] += 0.25f;
                    if (revenge)
                        npc.ai[2] += 0.25f;

                    if (npc.ai[2] >= 75f)
                    {
                        npc.velocity.X *= 0.93f;
                        npc.velocity.Y *= 0.93f;

                        if (npc.velocity.X > -0.1 && npc.velocity.X < 0.1)
                            npc.velocity.X = 0f;
                        if (npc.velocity.Y > -0.1 && npc.velocity.Y < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

                    if (npc.ai[2] >= 105f)
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;
                        npc.target = 255;
                        npc.rotation = calCloneBroRotation;
                        npc.TargetClosest();
                        if (npc.ai[3] >= 3f)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;
                            return;
                        }
                        npc.ai[1] = 1f;
                    }
                }
            }
        }

        public static void CatastropheAI(NPC npc, Mod mod)
        {
            CalamityGlobalNPC calamityGlobalNPC = npc.Calamity();

            // Percent life remaining
            float lifeRatio = npc.life / (float)npc.lifeMax;

            CalamityGlobalNPC.catastrophe = npc.whoAmI;

            // Emit light
            Lighting.AddLight((int)((npc.position.X + npc.width / 2) / 16f), (int)((npc.position.Y + npc.height / 2) / 16f), 1f, 0f, 0f);

            bool death = CalamityWorld.death;
            bool revenge = CalamityWorld.revenge;
            bool expertMode = Main.expertMode;

            // Get a target
            if (npc.target < 0 || npc.target == Main.maxPlayers || Main.player[npc.target].dead || !Main.player[npc.target].active)
                npc.TargetClosest();

            // Despawn safety, make sure to target another player if the current player target is too far away
            if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) > CalamityGlobalNPC.CatchUpDistance200Tiles)
                npc.TargetClosest();

            Player player = Main.player[npc.target];

            float calCloneBroPlayerXDist = npc.position.X + npc.width / 2 - player.position.X - player.width / 2;
            float calCloneBroPlayerYDist = npc.position.Y + npc.height - 59f - player.position.Y - player.height / 2;
            float calCloneBroRotation = (float)Math.Atan2(calCloneBroPlayerYDist, calCloneBroPlayerXDist) + MathHelper.PiOver2;
            if (calCloneBroRotation < 0f)
                calCloneBroRotation += MathHelper.TwoPi;
            else if (calCloneBroRotation > MathHelper.TwoPi)
                calCloneBroRotation -= MathHelper.TwoPi;

            float calCloneBroRotationSpeed = 0.15f;
            if (npc.rotation < calCloneBroRotation)
            {
                if (calCloneBroRotation - npc.rotation > MathHelper.Pi)
                    npc.rotation -= calCloneBroRotationSpeed;
                else
                    npc.rotation += calCloneBroRotationSpeed;
            }
            else if (npc.rotation > calCloneBroRotation)
            {
                if (npc.rotation - calCloneBroRotation > MathHelper.Pi)
                    npc.rotation += calCloneBroRotationSpeed;
                else
                    npc.rotation -= calCloneBroRotationSpeed;
            }

            if (npc.rotation > calCloneBroRotation - calCloneBroRotationSpeed && npc.rotation < calCloneBroRotation + calCloneBroRotationSpeed)
                npc.rotation = calCloneBroRotation;
            if (npc.rotation < 0f)
                npc.rotation += MathHelper.TwoPi;
            else if (npc.rotation > MathHelper.TwoPi)
                npc.rotation -= MathHelper.TwoPi;
            if (npc.rotation > calCloneBroRotation - calCloneBroRotationSpeed && npc.rotation < calCloneBroRotation + calCloneBroRotationSpeed)
                npc.rotation = calCloneBroRotation;

            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    despawnTimer++;
                    if (npc.velocity.Y > 3f)
                        npc.velocity.Y = 3f;
                    npc.velocity.Y -= 0.1f;
                    if (npc.velocity.Y < -12f)
                        npc.velocity.Y = -12f;

                    if (despawnTimer > 60)
                    {
                        npc.active = false;
                        despawnTimer = 0;
                    }

                    if (npc.ai[1] != 0f)
                    {
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }

                    return;
                }
            }
            else
            {
                despawnTimer = 0;
            }

            if (npc.ai[1] == 0f)
            {
                float calCloneBroProjAttackMaxSpeed = 4.5f;
                float calCloneBroProjAttackAccel = 0.2f;
                calCloneBroProjAttackMaxSpeed += 2f;
                calCloneBroProjAttackAccel += 0.1f;

                if (Main.getGoodWorld)
                {
                    calCloneBroProjAttackMaxSpeed *= 1.15f;
                    calCloneBroProjAttackAccel *= 1.15f;
                }

                int calCloneBroProjAttackDirection = 1;
                if (npc.Center.X < player.Center.X)
                    calCloneBroProjAttackDirection = -1;

                Vector2 calCloneBroProjLocation = npc.Center;
                float calCloneBroProjTargetX = player.Center.X + calCloneBroProjAttackDirection * 180 - calCloneBroProjLocation.X;
                float calCloneBroProjTargetY = player.Center.Y - calCloneBroProjLocation.Y;
                float calCloneBroProjTargetDist = (float)Math.Sqrt(calCloneBroProjTargetX * calCloneBroProjTargetX + calCloneBroProjTargetY * calCloneBroProjTargetY);

                if (expertMode)
                {
                    if (calCloneBroProjTargetDist > 300f)
                        calCloneBroProjAttackMaxSpeed += 0.5f;
                    if (calCloneBroProjTargetDist > 400f)
                        calCloneBroProjAttackMaxSpeed += 0.5f;
                    if (calCloneBroProjTargetDist > 500f)
                        calCloneBroProjAttackMaxSpeed += 0.55f;
                    if (calCloneBroProjTargetDist > 600f)
                        calCloneBroProjAttackMaxSpeed += 0.55f;
                    if (calCloneBroProjTargetDist > 700f)
                        calCloneBroProjAttackMaxSpeed += 0.6f;
                    if (calCloneBroProjTargetDist > 800f)
                        calCloneBroProjAttackMaxSpeed += 0.6f;
                }

                calCloneBroProjTargetDist = calCloneBroProjAttackMaxSpeed / calCloneBroProjTargetDist;
                calCloneBroProjTargetX *= calCloneBroProjTargetDist;
                calCloneBroProjTargetY *= calCloneBroProjTargetDist;

                if (npc.velocity.X < calCloneBroProjTargetX)
                {
                    npc.velocity.X += calCloneBroProjAttackAccel;
                    if (npc.velocity.X < 0f && calCloneBroProjTargetX > 0f)
                        npc.velocity.X += calCloneBroProjAttackAccel;
                }
                else if (npc.velocity.X > calCloneBroProjTargetX)
                {
                    npc.velocity.X -= calCloneBroProjAttackAccel;
                    if (npc.velocity.X > 0f && calCloneBroProjTargetX < 0f)
                        npc.velocity.X -= calCloneBroProjAttackAccel;
                }
                if (npc.velocity.Y < calCloneBroProjTargetY)
                {
                    npc.velocity.Y += calCloneBroProjAttackAccel;
                    if (npc.velocity.Y < 0f && calCloneBroProjTargetY > 0f)
                        npc.velocity.Y += calCloneBroProjAttackAccel;
                }
                else if (npc.velocity.Y > calCloneBroProjTargetY)
                {
                    npc.velocity.Y -= calCloneBroProjAttackAccel;
                    if (npc.velocity.Y > 0f && calCloneBroProjTargetY < 0f)
                        npc.velocity.Y -= calCloneBroProjAttackAccel;
                }

                npc.ai[2] += 1f;
                if (npc.ai[2] >= 180f - (death ? 90f * (1f - lifeRatio) : 0f))
                {
                    npc.TargetClosest();
                    npc.ai[1] = 1f;
                    npc.ai[2] = 0f;
                    npc.target = 255;
                    npc.netUpdate = true;
                }

                bool fireDelay = npc.ai[2] > 120f || lifeRatio < 0.9f;
                if (Collision.CanHit(npc.position, npc.width, npc.height, player.position, player.width, player.height) && fireDelay)
                {
                    npc.localAI[2] += 1f;
                    if (npc.localAI[2] > 36f)
                    {
                        npc.localAI[2] = 0f;
                        SoundEngine.PlaySound(SoundID.Item34, npc.Center);
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.localAI[1] += 1f;
                        if (revenge)
                            npc.localAI[1] += 0.5f;

                        if (npc.localAI[1] > 50f)
                        {
                            npc.localAI[1] = 0f;
                            float calCloneBroProjSpeed = death ? 14f : 12f;
                            calCloneBroProjSpeed += 3f;
                            int type = ModContent.ProjectileType<BrimstoneBall>();
                            int damage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 192 : CalamityWorld.death ? 140 : CalamityWorld.revenge ? 128 : Main.expertMode ? 112 : 70);
                            calCloneBroProjLocation = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                            calCloneBroProjTargetX = player.position.X + player.width / 2 - calCloneBroProjLocation.X;
                            calCloneBroProjTargetY = player.position.Y + player.height / 2 - calCloneBroProjLocation.Y;
                            calCloneBroProjTargetDist = (float)Math.Sqrt(calCloneBroProjTargetX * calCloneBroProjTargetX + calCloneBroProjTargetY * calCloneBroProjTargetY);
                            calCloneBroProjTargetDist = calCloneBroProjSpeed / calCloneBroProjTargetDist;
                            calCloneBroProjTargetX *= calCloneBroProjTargetDist;
                            calCloneBroProjTargetY *= calCloneBroProjTargetDist;
                            calCloneBroProjTargetY += npc.velocity.Y * 0.5f;
                            calCloneBroProjTargetX += npc.velocity.X * 0.5f;
                            calCloneBroProjLocation.X -= calCloneBroProjTargetX;
                            calCloneBroProjLocation.Y -= calCloneBroProjTargetY;
                            Projectile.NewProjectile(npc.GetSource_FromAI(), calCloneBroProjLocation.X, calCloneBroProjLocation.Y, calCloneBroProjTargetX, calCloneBroProjTargetY, type, damage, 0f, Main.myPlayer, 0f, 0f);
                        }
                    }
                }
            }
            else
            {
                if (npc.ai[1] == 1f)
                {
                    SoundEngine.PlaySound(SoundID.Roar, npc.Center);
                    npc.rotation = calCloneBroRotation;

                    float calCloneBroChargeSpeed = (NPC.AnyNPCs(ModContent.NPCType<Cataclysm>()) ? 12f : 16f) + (death ? 4f * (1f - lifeRatio) : 0f);
                    calCloneBroChargeSpeed += 4f;
                    if (expertMode)
                        calCloneBroChargeSpeed += 2f;
                    if (revenge)
                        calCloneBroChargeSpeed += 2f;
                    if (Main.getGoodWorld)
                        calCloneBroChargeSpeed *= 1.25f;

                    Vector2 calCloneBroChargeCenter = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
                    float calCloneBroChargeTargetXDist = player.position.X + player.width / 2 - calCloneBroChargeCenter.X;
                    float calCloneBroChargeTargetYDist = player.position.Y + player.height / 2 - calCloneBroChargeCenter.Y;
                    float calCloneBroChargeTargetDistance = (float)Math.Sqrt(calCloneBroChargeTargetXDist * calCloneBroChargeTargetXDist + calCloneBroChargeTargetYDist * calCloneBroChargeTargetYDist);
                    calCloneBroChargeTargetDistance = calCloneBroChargeSpeed / calCloneBroChargeTargetDistance;
                    npc.velocity.X = calCloneBroChargeTargetXDist * calCloneBroChargeTargetDistance;
                    npc.velocity.Y = calCloneBroChargeTargetYDist * calCloneBroChargeTargetDistance;
                    npc.ai[1] = 2f;

                    if (Main.zenithWorld)
                    {
                        SoundEngine.PlaySound(SupremeCalamitas.BrimstoneShotSound, npc.Center);

                        int type = ModContent.ProjectileType<BrimstoneBarrage>();
                        int damage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 192 : CalamityWorld.death ? 140 : CalamityWorld.revenge ? 128 : Main.expertMode ? 112 : 70);
                        int totalProjectiles = death ? 10 : revenge ? 8 : expertMode ? 6 : 4;
                        float radians = MathHelper.TwoPi / totalProjectiles;
                        float velocity = 5f;
                        Vector2 spinningPoint = new Vector2(0f, -velocity);
                        for (int k = 0; k < totalProjectiles; k++)
                        {
                            Vector2 velocity2 = spinningPoint.RotatedBy(radians * k);
                            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity2, type, damage, 0f, Main.myPlayer, 0f, 1f);
                        }

                        for (int i = 0; i < 6; i++)
                            Dust.NewDust(npc.position + npc.velocity, npc.width, npc.height, (int)CalamityDusts.Brimstone, 0f, 0f);
                    }
                    return;
                }

                if (npc.ai[1] == 2f)
                {
                    npc.ai[2] += 1f + (death ? 0.5f * (1f - lifeRatio) : 0f);
                    if (expertMode)
                        npc.ai[2] += 0.25f;
                    if (revenge)
                        npc.ai[2] += 0.25f;

                    if (npc.ai[2] >= 60f) //50
                    {
                        npc.velocity.X *= 0.93f;
                        npc.velocity.Y *= 0.93f;

                        if (npc.velocity.X > -0.1 && npc.velocity.X < 0.1)
                            npc.velocity.X = 0f;
                        if (npc.velocity.Y > -0.1 && npc.velocity.Y < 0.1)
                            npc.velocity.Y = 0f;
                    }
                    else
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) - MathHelper.PiOver2;

                    if (npc.ai[2] >= 90f) //80
                    {
                        npc.ai[3] += 1f;
                        npc.ai[2] = 0f;
                        npc.TargetClosest();
                        npc.rotation = calCloneBroRotation;
                        if (npc.ai[3] >= 4f)
                        {
                            npc.ai[1] = 0f;
                            npc.ai[3] = 0f;
                            return;
                        }
                        npc.ai[1] = 1f;
                    }
                }
            }
        }
    }
}

﻿using CalamityMod.World;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkCoalescence : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        public static int fireDelay;
        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.damage = DarkMonster.MonsterDamage;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 390;
            Projectile.scale = 1f;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.Calamity().DealsDefenseDamage = true;
        }
        private int aiCounter
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            fireDelay = CalamityWorld.death ? 65 : CalamityWorld.revenge ? 80 : 120;
            Projectile.scale = 0f;
            for(int i = 0; i < 30; i++)
            {
                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, 5, 50, TwoPi / 30 * i, false);
            }
            const int pCount = 20;
            for (int i = 0; i <= pCount; i++)
            {

                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(75, 100), TwoPi / pCount * i);
                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(60, 80), TwoPi / pCount * -i - TwoPi / (pCount/2));
            }
            ScreenShakeSystem.StartShake(5f);
        }
        public override void AI()
        {
            NPC Orator = null;
            if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];          
            
            if(Orator == null)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                for (int i = 0; i <= 50; i++)
                {
                    EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                }
                Projectile.active = false;
            }
            
            Player target;
            if(Orator != null)    
                target = Main.player[Player.FindClosest(Orator.Center, Projectile.width, Projectile.height)];
            else
                target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];

            if(aiCounter == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);
                for (int i = 0; i <= 50; i++)
                {
                    Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f) * 10;
                    EmpyreanMetaball.SpawnDefaultParticle(spawnPos,  (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 4, 40 * Main.rand.NextFloat(3f, 5f));
                }
            }           
            if (aiCounter < fireDelay)
            {
                Projectile.Center = new(target.Center.X - (500 * Projectile.ai[1]), target.Center.Y + (500 * Projectile.ai[2]));
                if (aiCounter < 60)
                    Projectile.scale += 5 / 60f;
            }
            else
            {
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(20f, 25f) * Projectile.scale), Vector2.Zero, 40 * Main.rand.NextFloat(3f, 5f));
                if (aiCounter == fireDelay)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with {Volume = 10f}, Projectile.Center);
                    Projectile.velocity = new(-10 * Projectile.ai[1], 10 * Projectile.ai[2]);
                }
                Projectile.velocity += new Vector2(0.5f * Projectile.ai[1], -0.5f * Projectile.ai[2]);
                if (Projectile.ai[1] != 0)
                {
                    if (Projectile.Center.Y > target.Center.Y)
                        Projectile.velocity.Y -= 0.15f;
                    else
                        Projectile.velocity.Y += 0.15f;
                }
                else
                {
                    if (Projectile.Center.X > target.Center.X)
                        Projectile.velocity.X -= 0.15f;
                    else
                        Projectile.velocity.X += 0.15f;
                }
                if (aiCounter >= fireDelay + 68)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);                   
                    for (int i = 0; i <= 50; i++)
                    {
                        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
                    }
                    if (Main.projectile.First(p => p.type == ModContent.ProjectileType<DarkCoalescence>()).whoAmI == Projectile.whoAmI)
                    {
                        if (Orator.ai[0] < 7)
                            SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion"), Projectile.Center);
                        ScreenShakeSystem.StartShake(12.5f);
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
                        for (int i = 0; i < 24; i++)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient && Orator.ai[0] == 6 || i % 2 == 0)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0);
                            if (Main.netMode != NetmodeID.MultiplayerClient && Orator.ai[0] < 7)
                                for (int j = 0; j < 3; j++)
                                {
                                    Projectile p = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, Main.rand.NextVector2Circular(15f, 15f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.75f, 1.5f));
                                    p.timeLeft /= 2;
                                }
                        }
                        CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 4f, 16);
                        GeneralParticleHandler.SpawnParticle(pulse);
                        CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1.5f, 16);
                        GeneralParticleHandler.SpawnParticle(explosion);
                    }                    
                    Projectile.active = false;
                    EmpyreanStickyParticles.RemoveAll(p => p.Projectile == Projectile);
                }
            }
            aiCounter++;
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Vector2.Zero, Projectile.scale * 80);           
            if (Projectile.ai[1] != 0)
                EmpyreanMetaball.SpawnDefaultParticle(new(Projectile.Center.X, Projectile.Center.Y + (Main.rand.Next(-19, 19) * Projectile.scale)), Vector2.Zero, Projectile.scale * 40);
            else
                EmpyreanMetaball.SpawnDefaultParticle(new(Projectile.Center.X + (Main.rand.Next(-19, 19) * Projectile.scale), Projectile.Center.Y), Vector2.Zero, Projectile.scale * 40);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CircularHitboxCollision(Projectile.Center, projHitbox.Width / 2, targetHitbox))
                return true;
            return false;
        }
    }
}

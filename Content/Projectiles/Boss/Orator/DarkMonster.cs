﻿using CalamityMod.Graphics.Metaballs;
using CalamityMod.Projectiles.Magic;
using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkMonster : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.damage = 100;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.scale = 1f;
            Projectile.alpha = 0;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        private readonly float Acceleration = CalamityWorld.death ? 0.55f : CalamityWorld.revenge ? 0.5f : Main.expertMode ? 0.45f : 0.4f;
        private readonly int MaxSpeed = CalamityWorld.death ? 15 : CalamityWorld.revenge ? 12 : 10;
        private int aiCounter
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int SoundDelay = 120;

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && Projectile.scale >= 4f)
            {
                Projectile.ai[0] = 1;
            }
            if (Projectile.ai[0] == 1)
            {
                Projectile.hostile = false;
                Projectile.scale -= (0.01f * (1 + (aiCounter / 8)));
                if (Projectile.scale <= 1.5f)
                    Projectile.ai[0] = 2;
                if (Projectile.velocity.Length() > 0f)
                {
                    Projectile.velocity -= (Projectile.velocity).SafeNormalize(Vector2.Zero) / 5;
                    EmitGhostGas(aiCounter);
                }
                if (Projectile.velocity.Length() < 1f)
                    Projectile.velocity = Vector2.Zero;
            }
            else if (Projectile.ai[0] == 2)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                for (int i = 0; i <= 50; i++)
                {
                    EmpyreanMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                }
                for (int i = 0; i < 12; i++)
                {
                    Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i).ToRotationVector2() * 10, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                    Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i + (TwoPi/24)).ToRotationVector2() * 5, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);                    
                }
                
                for (int i = 0; i < 24; i++)
                {
                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? - 10 : 0);
                    if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && i % 3 == 0)
                        NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<DarkSpawn>());
                }
                Projectile.active = false;
            }
            else if (Projectile.ai[0] == 0)
            {
                if (SoundDelay == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop, Projectile.Center);
                    SoundDelay = 1020;
                }
                else
                    SoundDelay--;
                if (aiCounter == 0)
                    Projectile.scale = 0;
                Projectile.hostile = true;
                if (aiCounter > 60)
                {
                    if (Projectile.Center.X > target.Center.X)
                        Projectile.velocity.X -= Acceleration;
                    if (Projectile.Center.X < target.Center.X)
                        Projectile.velocity.X += Acceleration;
                    if (Projectile.Center.Y > target.Center.Y)
                        Projectile.velocity.Y -= Acceleration;
                    if (Projectile.Center.Y < target.Center.Y)
                        Projectile.velocity.Y += Acceleration;
                }
                else
                {
                    Projectile.scale += 5 / 60f;
                    
                }
                if (Projectile.velocity.Length() > MaxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;
                EmitGhostGas(aiCounter);
            }
            aiCounter++;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public void EmitGhostGas(int counter)
        {
            float gasSize = 40 * Projectile.scale;
            EmpyreanMetaball.SpawnParticle(Projectile.Center - Projectile.velocity * (2 * Projectile.scale), Vector2.Zero, gasSize);

            gasSize = 20 * Projectile.scale;
            EmpyreanMetaball.SpawnParticle(Projectile.Center - (Projectile.velocity.RotatedBy(Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
            EmpyreanMetaball.SpawnParticle(Projectile.Center - (Projectile.velocity.RotatedBy(-Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CircularHitboxCollision(Projectile.Center, projHitbox.Width / 2, targetHitbox))
                return true;
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }
    }
}

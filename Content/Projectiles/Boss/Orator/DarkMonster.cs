using CalamityMod.World;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.TheOrator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkMonster : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";

        internal static int MonsterDamage;
        private float Acceleration;
        private int MaxSpeed;
        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.damage = MonsterDamage;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.scale = 1f;
            Projectile.alpha = 0;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.Calamity().DealsDefenseDamage = true;
        }        
        private enum States
        {
            Chasing,
            Dying,
            Exploding,
        }
        private int aiCounter
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private States AIState
        {
            get => (States)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private int SoundDelay = 120;

        public override void OnSpawn(IEntitySource source)
        {
            MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
            Acceleration = CalamityWorld.death ? 0.75f : CalamityWorld.revenge ? 0.5f : Main.expertMode ? 0.45f : 0.4f;
            MaxSpeed = CalamityWorld.death ? 15 : CalamityWorld.revenge ? 12 : 10;
            Projectile.scale = 0;
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);
            ScreenShakeSystem.StartShake(5f);
            for (int i = 0; i <= 50; i++)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f) * 10;
                SpawnDefaultParticle(spawnPos, (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 4, 40 * Main.rand.NextFloat(3f, 5f));
            }
            for (int i = 0; i < 30; i++)
            {
                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, 5, 50, TwoPi / 30 * i, false);
            }
            const int pCount = 20;
            for (int i = 0; i <= pCount; i++)
            {

                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(75, 100), TwoPi / pCount * i);
                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, Main.rand.NextFloat(10, 25), Main.rand.NextFloat(60, 80), TwoPi / pCount * -i - TwoPi / (pCount / 2));
            }
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            
            if (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && AIState == States.Chasing && Projectile.scale > 4f)
                AIState = States.Dying;
            
            switch (AIState)
            {
                case States.Chasing:
                    if (aiCounter <= 60)
                        Projectile.scale += 5 / 60f;
                    if (SoundDelay == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop, Projectile.Center);
                        SoundDelay = 1020;
                    }
                    else
                        SoundDelay--;
                    Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;
                    if (Projectile.velocity.Length() > MaxSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.velocity.Length() * 0.95f);
                    
                    EmitGhostGas();
                    break;
                case States.Dying:                   
                    Projectile.scale -= (0.005f * (1 + (aiCounter / 8)));                  
                    if (Projectile.scale <= 1.5f)
                        Projectile.ai[0] = 2;                   
                    if (Projectile.velocity.Length() > 0f)
                    {
                        Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;
                        if (Projectile.velocity.Length() > (MaxSpeed * (Projectile.scale / 5)))
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (MaxSpeed * (Projectile.scale / 5));
                        EmitGhostGas();
                    }
                    if (Projectile.velocity.Length() < 1f)
                        Projectile.velocity = Vector2.Zero;
                    const float ShakeBy = 25f;
                    Projectile.position += new Vector2(Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale - 0.5f), Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2));
                    break;
                case States.Exploding:
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                    ScreenShakeSystem.StartShake(7.5f);
                    for (int i = 0; i <= 50; i++)
                    {
                        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
                    }                    
                    for (int i = 0; i < (CalamityWorld.death ? 10 : CalamityWorld.revenge ? 8 : Main.expertMode ? 7 : 6); i++)
                    {
                        NPC Orator = null;
                        if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                            Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                        if (Orator != null && Orator.ai[0] == 2 && (float)Orator.life / (float)Orator.lifeMax > 0.1f)
                            NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<DarkSpawn>());
                    }
                    
                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0);
                        for(int j = 0; j < 2; j++)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, Main.rand.NextVector2Circular(12f, 12f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.75f, 1.5f));
                    }
                    CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                    GeneralParticleHandler.SpawnParticle(pulse);
                    CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                    GeneralParticleHandler.SpawnParticle(explosion);
                    Projectile.active = false;
                    EmpyreanStickyParticles.RemoveAll(p => p.Projectile == Projectile);
                    break;
            }
            aiCounter++;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public void EmitGhostGas()
        {
            //smaller particles
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0f, 5f), 40 * (Main.rand.NextFloat(0.75f, 0.9f) * Projectile.scale));
            
            //larger trails
            float gasSize = 60 * Projectile.scale;
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - Projectile.velocity * (2 * Projectile.scale), Vector2.Zero, gasSize);

            gasSize = 40 * Projectile.scale;
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - (Projectile.velocity.RotatedBy(Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - (Projectile.velocity.RotatedBy(-Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
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

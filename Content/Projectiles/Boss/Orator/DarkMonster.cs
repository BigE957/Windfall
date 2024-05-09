using CalamityMod.Graphics.Metaballs;
using CalamityMod.Projectiles.Magic;
using CalamityMod.World;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkMonster : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        
        internal static readonly int MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
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
        }
        private readonly float Acceleration = CalamityWorld.death ? 0.55f : CalamityWorld.revenge ? 0.5f : Main.expertMode ? 0.45f : 0.4f;
        private readonly int MaxSpeed = CalamityWorld.death ? 15 : CalamityWorld.revenge ? 12 : 10;
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
            Projectile.scale = 0;
            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen with {Volume = 2f}, Projectile.Center);
            ScreenShakeSystem.SetUniversalRumble(5f);
            for (int i = 0; i <= 50; i++)
            {
                EmpyreanMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
            }
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && Projectile.scale >= 4f)
            {
                Projectile.ai[0] = 1;
            }
            switch(AIState)
            {
                case States.Chasing:
                    if (aiCounter <= 60)
                        Projectile.scale += 5 / 60f;
                    if (SoundDelay == 0)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with {Volume = 2f}, Projectile.Center);
                        SoundDelay = 1020;
                    }
                    else
                        SoundDelay--;

                    if (Projectile.Center.X > target.Center.X)
                        Projectile.velocity.X -= Acceleration;
                    if (Projectile.Center.X < target.Center.X)
                        Projectile.velocity.X += Acceleration;
                    if (Projectile.Center.Y > target.Center.Y)
                        Projectile.velocity.Y -= Acceleration;
                    if (Projectile.Center.Y < target.Center.Y)
                        Projectile.velocity.Y += Acceleration;                   
                    if (Projectile.velocity.Length() > MaxSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;
                    
                    EmitGhostGas(aiCounter);
                    break;
                case States.Dying:                   
                    Projectile.scale -= (0.01f * (1 + (aiCounter / 8)));
                    if (Projectile.scale <= 1.5f)
                        Projectile.ai[0] = 2;                   
                    if (Projectile.velocity.Length() > 0f)
                    {
                        if (Projectile.Center.X > target.Center.X)
                            Projectile.velocity.X -= Acceleration;
                        if (Projectile.Center.X < target.Center.X)
                            Projectile.velocity.X += Acceleration;
                        if (Projectile.Center.Y > target.Center.Y)
                            Projectile.velocity.Y -= Acceleration;
                        if (Projectile.Center.Y < target.Center.Y)
                            Projectile.velocity.Y += Acceleration;
                        if (Projectile.velocity.Length() > (MaxSpeed * (Projectile.scale / 5)))
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (MaxSpeed * (Projectile.scale / 5));
                        EmitGhostGas(aiCounter);
                    }
                    if (Projectile.velocity.Length() < 1f)
                        Projectile.velocity = Vector2.Zero;
                    break;
                case States.Exploding:
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                    ScreenShakeSystem.SetUniversalRumble(7.5f);
                    for (int i = 0; i <= 50; i++)
                    {
                        EmpyreanMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                    }
                    for (int i = 0; i < 12; i++)
                    {
                        Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i).ToRotationVector2() * 10, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                        Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i + (TwoPi / 24)).ToRotationVector2() * 5, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                    }

                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0);
                        NPC Orator = null;
                        if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                            Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                        if (Orator != null && i % 3 == 0 && (float)Orator.life / (float)Orator.lifeMax > 0.1f)
                            NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<DarkSpawn>());
                    }
                    Projectile.active = false;
                    break;
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

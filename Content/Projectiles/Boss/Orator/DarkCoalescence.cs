using Luminance;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkCoalescence : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
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
            Projectile.scale = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        private int aiCounter
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;            
            ScreenShakeSystem.SetUniversalRumble(5f);
            
        }
        public override void AI()
        {
            NPC Orator = null;
            if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
            /*
            if(Orator == null)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                for (int i = 0; i <= 50; i++)
                {
                    EmpyreanMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
                }
                Projectile.active = false;
            }
            */
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
                    EmpyreanMetaball.SpawnParticle(spawnPos,  (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 4, 40 * Main.rand.NextFloat(3f, 5f));
                }
            }

            if (aiCounter < 120)
            {
                Projectile.position = new(target.position.X - (500 * Projectile.ai[1]), target.position.Y + (500 * Projectile.ai[2]));
                if (aiCounter < 60)
                    Projectile.scale += 5 / 60f;
            }
            else
            {
                EmpyreanMetaball.SpawnParticle(Projectile.Center + (Main.rand.NextVector2Circular(20f, 20f) * Projectile.scale), Vector2.Zero, 40 * Main.rand.NextFloat(3f, 5f));
                if (aiCounter == 120)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with {Volume = 10f}, Projectile.Center);
                    Projectile.velocity = new(-10 * Projectile.ai[1], 10 * Projectile.ai[2]);
                }
                Projectile.velocity += new Vector2(0.5f * Projectile.ai[1], -0.5f * Projectile.ai[2]);
                if (Projectile.ai[1] != 0)
                {
                    if (Projectile.position.Y > target.position.Y)
                        Projectile.velocity.Y -= 0.15f;
                    else
                        Projectile.velocity.Y += 0.15f;
                }
                else
                {
                    if (Projectile.position.X > target.position.X)
                        Projectile.velocity.X -= 0.15f;
                    else
                        Projectile.velocity.X += 0.15f;
                }
                if (aiCounter >= 188)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                    for (int i = 0; i <= 50; i++)
                    {
                        EmpyreanMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(15f, 15f), 40 * Main.rand.NextFloat(3f, 5f));
                    }
                    if (Main.projectile.First(p => p.type == ModContent.ProjectileType<DarkCoalescence>()).whoAmI == Projectile.whoAmI)
                    {
                        ScreenShakeSystem.SetUniversalRumble(12.5f);
                        SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Projectile.Center);
                        for (int i = 0; i < 12; i++)
                        {
                            Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i).ToRotationVector2() * 10, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                            Projectile.NewProjectile(Entity.GetSource_Death(), Projectile.Center, (TwoPi / 12 * i + (TwoPi / 24)).ToRotationVector2() * 5, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 1, 0.5f);
                        }
                        for (int i = 0; i < 24; i++)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0);
                        }
                    }
                    Projectile.active = false;
                }
            }
            aiCounter++;
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
            EmpyreanMetaball.SpawnParticle(Projectile.Center, Vector2.Zero, Projectile.scale * 80);           
            if (Projectile.ai[1] != 0)
                EmpyreanMetaball.SpawnParticle(new(Projectile.Center.X, Projectile.Center.Y + (Main.rand.Next(-19, 19) * Projectile.scale)), Vector2.Zero, Projectile.scale * 40);
            else
                EmpyreanMetaball.SpawnParticle(new(Projectile.Center.X + (Main.rand.Next(-19, 19) * Projectile.scale), Projectile.Center.Y), Vector2.Zero, Projectile.scale * 40);
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

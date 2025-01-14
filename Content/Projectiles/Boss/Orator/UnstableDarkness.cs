using CalamityMod.World;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;
using static Windfall.Content.NPCs.Bosses.Orator.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class UnstableDarkness: ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
    public override void SetDefaults()
    {
        Projectile.width = 320;
        Projectile.height = 320;
        Projectile.damage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 320 : CalamityWorld.death ? 240 : CalamityWorld.revenge ? 230: Main.expertMode ? 220 : 120);
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
    private int aiCounter
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void OnSpawn(IEntitySource source)
    {
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
        if (aiCounter <= 30)
        {
            Projectile.scale += 1.5f / 30f;
            ParticleTrail();
        }
        else
        {
            Projectile.scale -= (0.004f * (1 + (aiCounter / 8)));
            if (Projectile.scale <= 0.5f)
                Explode();
            if (Projectile.velocity.Length() > 0f)
            {
                Projectile.velocity *= 0.995f;
                ParticleTrail();
            }
            if (Projectile.velocity.Length() < 1f)
                Projectile.velocity = Vector2.Zero;
            const float ShakeBy = 25f;
            Projectile.position += new Vector2(Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 3), Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 3));
        }
        aiCounter++;
    }
    private void Explode()
    {
        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
        ScreenShakeSystem.StartShake(7.5f);
        for (int i = 0; i <= 50; i++)
        {
            SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
        }
        NPC Orator = null;
        if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
            Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
        if (Main.netMode != NetmodeID.MultiplayerClient && Orator != null && (float)Orator.life / (float)Orator.lifeMax > 0.1f)
        {
            for (int i = 0; i < 24; i++)
                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center + Main.rand.NextVector2Circular(64f, 64f), Main.rand.NextVector2Circular(4f, 4f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.5f, 0.75f));
        }
        CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
        GeneralParticleHandler.SpawnParticle(pulse);
        CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
        GeneralParticleHandler.SpawnParticle(explosion);
        Projectile.active = false;
        EmpyreanStickyParticles.RemoveAll(p => p.Projectile == Projectile);
    }
    private void ParticleTrail()
    {
        //smaller particles
        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0f, 5f), 40 * (Main.rand.NextFloat(0.75f, 0.9f) * Projectile.scale));

        //larger trails
        float gasSize = 90 * Projectile.scale;
        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - Projectile.velocity * (2 * Projectile.scale), Vector2.Zero, gasSize);
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

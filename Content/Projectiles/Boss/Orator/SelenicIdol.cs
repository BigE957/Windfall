using CalamityMod.Graphics.Primitives;
using CalamityMod.World;
using ReLogic.Utilities;
using Terraria.Graphics.Shaders;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class SelenicIdol : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/GoldenMoon";

    SlotId loopingSoundSlot;

    internal static int MonsterDamage;
    private static float Acceleration;
    private static int MaxSpeed;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
        Acceleration = CalamityWorld.death ? 0.75f : CalamityWorld.revenge ? 0.5f : Main.expertMode ? 0.45f : 0.4f;
        MaxSpeed = CalamityWorld.death ? 15 : CalamityWorld.revenge ? 12 : 10;

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
        Projectile.netImportant = true;
    }        

    private int aiCounter
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private enum States
    {
        Chasing,
        Dying,
        Exploding,
    }
    private States AIState
    {
        get => (States)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }

    private ref float GoopScale => ref Projectile.ai[2];

    int deathCounter = 0;
    float rotationCounter = 0;

    public override void OnSpawn(IEntitySource source)
    {        
        Projectile.scale = 0;
        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);
        Luminance.Core.Graphics.ScreenShakeSystem.StartShake(5f);
        for (int i = 0; i <= 50; i++)
        {
            Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f) * 10;
            SpawnDefaultParticle(spawnPos, (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 4, 40 * Main.rand.NextFloat(3f, 5f));
        }
    }
    
    public override void AI()
    {
        if (aiCounter == 0 && Main.netMode == NetmodeID.MultiplayerClient)
        {
            for (int i = 0; i < 30; i++)
            {
                SpawnBorderParticle(Projectile, Vector2.Zero, 0f, 5, 50, TwoPi / 30 * i, false);
            }
            const int pCount = 20;
            for (int i = 0; i <= 20; i++)
            {
                SpawnBorderParticle(Projectile, Vector2.Zero, 1f * i, 20, Main.rand.NextFloat(80, 110), TwoPi / pCount * i);
                //SpawnBorderParticle(Projectile, Vector2.Zero, 0.5f * i, 15, Main.rand.NextFloat(60, 80), TwoPi / pCount * -i - TwoPi / (pCount / 2));
            }
        }

        Player target;
        if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            target = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())].As<TheOrator>().target;                           
        else
        {
            target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (AIState == States.Chasing && Projectile.scale > 0.9f)
                AIState = States.Dying;
        }
        if(!NPC.AnyNPCs(ModContent.NPCType<OratorHand>()) || NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) == -1 || Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())].ai[0] == (int)TheOrator.States.PhaseChange)
            if (AIState == States.Chasing && Projectile.scale > 0.9f)
                AIState = States.Dying;
        
        switch (AIState)
        {
            case States.Chasing:
                if (aiCounter <= 60)
                {
                    float lerp = aiCounter / 60f;
                    Projectile.scale = CircOutEasing(lerp, 1);
                    GoopScale = 1 - SineInEasing(lerp, 1);
                    SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(48f, 48f) * Projectile.scale), Main.rand.NextVector2Circular(18, 18) + Projectile.velocity, 200 * Main.rand.NextFloat(0.75f, 0.9f) * (1 - lerp));
                }
                
                Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;
                if (Projectile.velocity.Length() > MaxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.velocity.Length() * 0.95f);

                #region Looping Sound
                if (!SoundEngine.TryGetActiveSound(loopingSoundSlot, out var activeSound))
                {
                    // if it isn't, play the sound and remember the SlotId
                    var tracker = new ProjectileAudioTracker(Projectile);
                    loopingSoundSlot = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop, Projectile.position, soundInstance => {
                        // This example is inlined, see ActiveSoundShowcaseProjectile.cs for other approaches
                        soundInstance.Position = Projectile.position;
                        return tracker.IsActiveAndInGame();
                    });
                }
                #endregion
                break;
            case States.Dying:
                float lerpValue = deathCounter / 90f;

                Projectile.scale = Lerp(1f, 0.5f, lerpValue);
                GoopScale = lerpValue;

                if (lerpValue >= 1f)
                    Projectile.ai[0] = 2;                   
                if (Projectile.velocity.Length() > 0f)
                {
                    Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;
                    if (Projectile.velocity.Length() > (MaxSpeed * Projectile.scale))
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (MaxSpeed * Projectile.scale);
                }
                if (Projectile.velocity.Length() < 1f)
                    Projectile.velocity = Vector2.Zero;

                #region Death Shake
                float ShakeBy = Lerp(0f, 18f, lerpValue);
                Vector2 shakeOffset = new(Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2), Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2));
                Projectile.position += shakeOffset;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                    Projectile.oldPos[i] += shakeOffset;
                #endregion

                SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(12, 12) * (lerpValue + 0.5f), 180 * (Main.rand.NextFloat(0.75f, 0.9f)));
                SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(18, 18) * (lerpValue + 0.5f), 90 * (Main.rand.NextFloat(0.75f, 0.9f)));

                deathCounter++;
                break;
            case States.Exploding:
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                Luminance.Core.Graphics.ScreenShakeSystem.StartShake(7.5f);
                for (int i = 0; i <= 50; i++)
                    SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
                NPC Orator = null;
                if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                    Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                if (Main.netMode != NetmodeID.MultiplayerClient && Orator != null && Orator.ai[0] == 2 && (float)Orator.life / (float)Orator.lifeMax > 0.1f)
                {
                    for (int i = 0; i < (CalamityWorld.death ? 10 : CalamityWorld.revenge ? 8 : Main.expertMode ? 7 : 6); i++)
                        NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<ShadowHand>());

                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0, i % 2 == 0 ? 1 : 0);
                        for (int j = 0; j < 2; j++)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, Main.rand.NextVector2Circular(12f, 12f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.75f, 1.5f));
                    }
                }
                Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                GeneralParticleHandler.SpawnParticle(pulse);
                Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                GeneralParticleHandler.SpawnParticle(explosion);
                Projectile.active = false;
                EmpyreanStickyParticles.RemoveAll(p => p.ProjectileIndex == Projectile.whoAmI);
                Projectile.netUpdate = true;
                break;
        }
        aiCounter++;
        Projectile.rotation = Projectile.velocity.ToRotation();
        rotationCounter += Projectile.velocity.X / 200f;
    }
    
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CircularHitboxCollision(Projectile.Center, projHitbox.Width / 2, targetHitbox))
            return true;
        return false;
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SwordSlashTexture"));

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"]), 50);

        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/BorderAura1").Value;

        Color[] colors = [
            Color.Gold, 
            Color.Goldenrod,
            Color.DarkGoldenrod,
            Color.Goldenrod,
        ];

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.25f, colors), Main.GlobalTimeWrappedHourly * 0.25f, tex.Size() * 0.5f, ((Projectile.scale * 0.375f) + (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.015f)) * (1 - GoopScale), 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        tex = TextureAssets.Projectile[Type].Value;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale, 0);

        return false;
    }

    internal Color ColorFunction(float completionRatio)
    {
        float averageRotation = Projectile.oldRot.Take(20).Average(angle => MathHelper.WrapAngle(angle) + MathHelper.Pi);
        float opacity = Projectile.Opacity;
        opacity *= (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D);

        if (deathCounter > 0)
            opacity = Clamp(Lerp(opacity, -0.5f, deathCounter / 90f), 0f, 1f);

        float rotationAdjusted = MathHelper.WrapAngle(Projectile.rotation) + MathHelper.Pi;
        float oldRotationAdjusted = MathHelper.WrapAngle(Projectile.oldRot[1]) + MathHelper.Pi;

        return Color.Lerp(Color.Goldenrod, Color.DarkGoldenrod * completionRatio, MathHelper.Clamp(completionRatio * 0.8f, 0f, 1f)) * opacity;
    }

    internal float WidthFunction(float completionRatio) => 200f * (1f - completionRatio) * 0.8f * Projectile.scale;

    public override void PostDraw(Color lightColor)
    {
        if (GoopScale != 0)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Metaballs/BasicCircle").Value;

            Vector2[] offsets = [new(70, -35), new(-78, 48), new(-0, 80), new(78, 0), new(34, 70), new(-44, -70)];

            for (int i = 0; i < offsets.Length; i++)
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offsets[i].RotatedBy(rotationCounter) * Projectile.scale, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale * 4f * GoopScale, 0);
        }
    }
}

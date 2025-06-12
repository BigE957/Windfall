using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.World;
using Luminance.Core.Graphics;
using ReLogic.Utilities;
using Terraria.Graphics.Shaders;
using Windfall.Common.Interfaces;
using Windfall.Common.Systems;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Skies;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class SelenicIdol : ModProjectile, IEmpyreanDissolve
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/GoldenMoon";

    SlotId loopingSoundSlot;

    internal static int MonsterDamage;
    private static float Acceleration;
    private static int MaxSpeed;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
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
        Projectile.hide = true;

        sampleOffset = Main.rand.NextVector2Circular(240, 240);
    }        

    private int Time
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
    Vector2 offset = Vector2.Zero;
    public float DissolveIntensity { get => Projectile.ai[2]; set => Projectile.ai[2] = value; }
    public Vector2 sampleOffset { get => offset; set => offset = value; }

    int deathCounter = 0;
    float rotationCounter = 0;

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
        DissolveIntensity = 1;
    }
    
    public override void AI()
    {
        Player target = Main.LocalPlayer;
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
                if (Time <= 60)
                {
                    float lerp = Clamp(Time / 60f, 0f, 1f);
                    Projectile.scale = CircOutEasing(lerp);
                    DissolveIntensity = SineInEasing(lerp);
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

                if(DissolveIntensity >= 0.75f && Main.rand.NextBool(3))
                {
                    Vector2 flakeDir = (-Projectile.velocity).SafeNormalize(Vector2.UnitX);
                    Vector2 spawnPos = Projectile.Center + (Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f) * Projectile.scale);
                    SpawnFlakeParticle(spawnPos, flakeDir * Main.rand.NextFloat(2f, 4f), Main.rand.NextFloat(0.01f, 0.1f), Main.rand.NextFloat(0.66f, 1f), flakeDir.ToRotation());
                }

                break;
            case States.Dying:
                if(deathCounter == 0)
                    sampleOffset = Main.rand.NextVector2Circular(240, 240);
                float lerpValue = Clamp(deathCounter / 150f, 0f, 1f);

                Projectile.scale = Lerp(1f, 0.5f, lerpValue);
                DissolveIntensity = 1 - Clamp(lerpValue * 1.5f, 0f, 1f);

                if (lerpValue >= 1f)
                    AIState = States.Exploding;

                if (Projectile.velocity.Length() > 0f)
                {
                    Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;
                    if (Projectile.velocity.Length() > (MaxSpeed * Projectile.scale))
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (MaxSpeed * Projectile.scale);
                }
                if (Projectile.velocity.Length() < 1f)
                    Projectile.velocity = Vector2.Zero;

                #region Death Shake
                float ShakeBy = Lerp(0f, 12f, lerpValue);
                Vector2 shakeOffset = new(Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2), Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2));
                Projectile.position += shakeOffset;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                    Projectile.oldPos[i] += shakeOffset;
                #endregion

                if (DissolveIntensity < 0.5f)
                {
                    SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(12, 12) * (lerpValue + 0.5f), 180 * (Main.rand.NextFloat(0.75f, 0.9f)));
                    SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(18, 18) * (lerpValue + 0.5f), 90 * (Main.rand.NextFloat(0.75f, 0.9f)));
                }
                else if(Main.rand.NextBool(4))
                {
                    Vector2 flakeDir = (-Projectile.velocity).SafeNormalize(Vector2.UnitX);
                    Vector2 spawnPos = Projectile.Center + (Main.rand.NextVector2Circular(Projectile.width / 2f, Projectile.height / 2f) * Projectile.scale);
                    SpawnFlakeParticle(spawnPos, (Projectile.Center + (flakeDir * 72) - spawnPos).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.25f, 1f), Main.rand.NextFloat(0.01f, 0.1f), Main.rand.NextFloat(0.5f, 2f), flakeDir.ToRotation());
                }

                    deathCounter++;
                break;
            case States.Exploding:
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                ScreenShakeSystem.StartShake(7.5f);
                for (int i = 0; i <= 50; i++)
                    SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
                NPC Orator = null;
                if (NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                    Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                if (Main.netMode != NetmodeID.MultiplayerClient && Orator != null && Orator.ai[0] == 2 && (float)Orator.life / (float)Orator.lifeMax > 0.1f)
                {
                    for (int i = 0; i < ShadowGrasp.MaxHands; i++)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowGrasp>(), TheOrator.BoltDamage, 0);

                    for (int i = 0; i < 24; i++)
                    {
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, i % 2 == 0 ? -10 : 0, i % 2 == 0 ? 1 : 0);
                        for (int j = 0; j < 2; j++)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, Main.rand.NextVector2Circular(12f, 12f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.75f, 1.5f));
                    }
                }
                PulseRing pulse = new(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                GeneralParticleHandler.SpawnParticle(pulse);
                DetailedExplosion explosion = new(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                GeneralParticleHandler.SpawnParticle(explosion);
                Projectile.active = false;
                Projectile.netUpdate = true;
                break;
        }
        Time++;
        Projectile.rotation = Projectile.velocity.ToRotation();
        rotationCounter += 0.01f;
    }
    
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CircularHitboxCollision(Projectile.Center, projHitbox.Width / 2, targetHitbox))
            return true;
        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"].SetTexture(LoadSystem.SwordSlash);

        CalamityMod.Graphics.Primitives.PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"]), 40);

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Texture2D tex = OratorSky.MoonBloom.Value;

        Color[] colors = [
            Color.Gold,
            Color.Goldenrod,
            Color.DarkGoldenrod,
            Color.Goldenrod,
        ];

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, LerpColors(Main.GlobalTimeWrappedHourly * 0.25f, colors), Main.GlobalTimeWrappedHourly * 0.25f, tex.Size() * 0.5f, ((Projectile.scale * 0.825f) + (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.025f)) * DissolveIntensity, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        tex = TextureAssets.Projectile[Type].Value;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale, 0);

        return false;
    }

    internal Color ColorFunction(float completionRatio)
    {
        float opacity = Projectile.Opacity;
        bool dying = deathCounter > 0;
        if (!dying)
            opacity -= 1 - DissolveIntensity;

        opacity *= (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D);

        if (dying)
            opacity = Clamp(Lerp(opacity, -0.5f, deathCounter / 90f), 0f, 1f);

        return Color.Lerp(Color.Gold, new(170, 100, 30), (completionRatio ) * 3f) * opacity * (Projectile.velocity.Length() / MaxSpeed);
    }

    internal float WidthFunction(float completionRatio) => 200f * (1f - completionRatio) * 0.8f * Projectile.scale;

    public void DrawOverlay(SpriteBatch sb)
    {
        Texture2D tex = LoadSystem.Circle.Value;
        Vector2 offset = Projectile.velocity;
        if (Main.gamePaused)
            offset = Vector2.Zero;
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + offset, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale * 5.05f, 0, 0);
    }
}

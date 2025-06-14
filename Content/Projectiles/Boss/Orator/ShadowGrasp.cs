using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.World;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator;
public class ShadowGrasp : ModProjectile
{
    public override string Texture => "Windfall/Assets/NPCs/Enemies/ShadowHand";

    public static Asset<Texture2D> Details;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 9;

        if (!Main.dedServ)
            Details = ModContent.Request<Texture2D>(Texture + "Details");
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 72;
        Projectile.hostile = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 2500;
        Projectile.tileCollide = false;
        Projectile.scale = 1.25f;

        Projectile.Calamity().DealsDefenseDamage = true;
    }

    int Damage = 0;

    internal enum AIState
    {
        Spawning,
        OnBoss,
        Attacking,
    }
    internal AIState CurrentAI
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }

    private enum Poses
    {
        Default,
        Fist,
        Flat,
        FingerGun,
        OK,
        Palm
    }
    private Poses Pose = Poses.Default;

    private int Time = 0;

    private static TheOrator Orator
    {
        get
        {
            int oratorIndex = NPC.FindFirstNPC(ModContent.NPCType<TheOrator>());
            if (oratorIndex == -1)
                return null;
            return Main.npc[oratorIndex].As<TheOrator>();
        }
    }

    public static List<Projectile> hands => [.. Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<ShadowGrasp>())];

    public static int MaxHands => (CalamityWorld.death ? 7 : CalamityWorld.revenge ? 5 : Main.expertMode ? 3 : 2) * 2 + 4;

    private int handID = -1;
    private int partnerIndex = -1;
    private bool MainHand => Projectile.whoAmI < partnerIndex;
    private int HandSide => MainHand ? 1 : -1;
    private Vector2 storedPos = Vector2.Zero;
    private Vector2 attackDir = Vector2.Zero;

    public override void OnSpawn(IEntitySource source)
    {
        Damage = Projectile.damage;
        Projectile.damage = 0;
        Projectile.velocity = Main.rand.NextFloat(0, TwoPi).ToRotationVector2() * Main.rand.Next(10, 15);
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.direction = 1;// Math.Sign(Projectile.velocity.X);
        FindFrame();
        for (int i = 0; i <= 20; i++)
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(7f, 7f), 40 * Main.rand.NextFloat(1.5f, 2.3f));
    }

    public override bool PreAI()
    {
        if (CurrentAI == AIState.Attacking)
        {
            if (Main.projectile[partnerIndex].As<ShadowGrasp>().CurrentAI != AIState.Attacking)
            {
                Main.projectile[partnerIndex].As<ShadowGrasp>().CurrentAI = AIState.Attacking;
                Time = 0;
                Main.projectile[partnerIndex].As<ShadowGrasp>().Time = 0;
            }
        }

        return true;
    }

    public override void AI()
    {
        if(Orator == null)
        {
            Projectile.active = false;
            return;
        }

        switch (CurrentAI)
        {
            case AIState.Spawning:
                Projectile.velocity *= 0.98f;
                Projectile.timeLeft = 2500;

                Dust dust = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * Main.rand.NextFloat(-16, 16) + new Vector2(-54, 0).RotatedBy(Projectile.rotation), DustID.RainbowTorch);
                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                dust.noGravity = true;
                dust.color = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);

                if (Time == 0)
                {
                    handID = hands.IndexOf(Projectile);
                }
                if (Time == 1)
                {
                    if (handID >= MaxHands - 4)
                    {
                        switch (handID - (MaxHands - 4))
                        {
                            case 0:
                            case 1:
                                partnerIndex = hands[handID + 2].whoAmI;
                                break;
                            case 2:
                            case 3:
                                partnerIndex = hands[handID - 2].whoAmI;
                                break;
                        }
                    }
                    else
                    {
                        bool frontHalf = handID < (MaxHands - 4) / 2;
                        partnerIndex = hands[handID + ((MaxHands - 4) / (frontHalf ? 2 : -2))].whoAmI;
                    }
                }

                if (Time > 120)
                {
                    CurrentAI = AIState.OnBoss;
                    storedPos = Projectile.Center;
                    Time = -1;
                }
                break;
            case AIState.OnBoss:
                Projectile.timeLeft = 240;
                Vector2 goalPosition = Projectile.Center;

                if (handID >= MaxHands - 4)
                {
                    Pose = Poses.OK;
                    float dist = 196;
                    float circleTransform = (float)Math.Sin(Time / 64f);
                    Vector2 circling = new((float)Math.Sin(Time / 16f) * (16 - (circleTransform * 12f)), (float)Math.Cos(Time / 16f) * (16 + (circleTransform * 12f)));
                    switch(handID - (MaxHands - 4))
                    {
                        case 0:
                            goalPosition = Orator.NPC.Center + new Vector2(dist, dist) + (circling * new Vector2(1, 1));
                            Projectile.rotation = Pi - PiOver4 - circleTransform;
                            Projectile.direction = 1;
                            break;
                        case 1:
                            goalPosition = Orator.NPC.Center + new Vector2(-dist, dist) + (circling * new Vector2(-1, 1));
                            Projectile.rotation = PiOver4 + circleTransform;
                            Projectile.direction = -1;
                            break;
                        case 2:
                            goalPosition = Orator.NPC.Center + new Vector2(-dist, -dist) + (circling * new Vector2(-1, -1));
                            Projectile.rotation = -PiOver4 - circleTransform;
                            Projectile.direction = 1;
                            break;
                        case 3:
                            goalPosition = Orator.NPC.Center + new Vector2(dist, -dist) + (circling * new Vector2(1, -1));
                            Projectile.rotation = Pi + PiOver4 + circleTransform;
                            Projectile.direction = -1;
                            break;
                    }
                }
                else
                {
                    Pose = handID % 2 == 0 ? Poses.Palm : Poses.Flat;
                    float dir = (TwoPi / (MaxHands - 4) * (handID - 4)) + Time / 24f;
                    Vector2 dirVec = dir.ToRotationVector2();
                    goalPosition = Orator.NPC.Center + dirVec * (160 + ((float)Math.Cos(Time / 24f + (handID % 2 == 0 ? Pi : 0)) * 32f));
                    Projectile.rotation = dir;
                    //Projectile.direction = Math.Sign(dirVec.X);
                }

                if (Time < 30)
                    Projectile.Center = Vector2.Lerp(storedPos, goalPosition, CircOutEasing(Time / 30f));
                else
                    Projectile.Center = goalPosition;

                break;
            case AIState.Attacking:
                Pose = Poses.Fist;
                if (Time == 0)
                    storedPos = Orator.target.velocity.SafeNormalize(Vector2.UnitX * Orator.target.direction);

                goalPosition = (Orator.target.Center + Orator.target.velocity * 3f) + (storedPos * 240 * HandSide);

                if (Time < 30)
                {
                    attackDir = Projectile.DirectionTo(Orator.target.Center);
                    Projectile.rotation = attackDir.ToRotation();
                    Vector2 toGoal = (goalPosition - Projectile.Center);
                    Projectile.velocity = toGoal / 10f;
                }
                else
                {
                    //Dash
                    Projectile.damage = Damage;
                    int reelbackTIme = 24;
                    if (Time < 30 + reelbackTIme)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, reelbackTIme, Time - 60, true);
                        float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = attackDir * -reelBackSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    }
                    else
                    {
                        if (Time == 30 + reelbackTIme)
                            Projectile.velocity = attackDir * 64f;
                        if (Time > 60 || MainHand && Projectile.Hitbox.Intersects(Main.projectile[partnerIndex].Hitbox))
                        {
                            Projectile.velocity = Vector2.Zero;
                            Projectile.active = false;

                            Main.projectile[partnerIndex].velocity = Vector2.Zero;
                            Main.projectile[partnerIndex].active = false;

                            Explode();
                        }
                    }
                }

                break;
        }

        FindFrame();

        Projectile.spriteDirection = Projectile.direction;

        Time++;
    }

    private void Explode()
    {
        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch with { Volume = 0.5f }, Projectile.Center);
        ScreenShakeSystem.StartShake(2.5f);

        for (int i = 0; i <= 50; i++)
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));

        if (Main.netMode != NetmodeID.MultiplayerClient && Orator != null && (float)Orator.NPC.life / (float)Orator.NPC.lifeMax > 0.1f)
        {
            for (int i = 0; i < 24; i++)
                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center + Main.rand.NextVector2Circular(64f, 64f), Main.rand.NextVector2Circular(4f, 4f), ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 0, Main.rand.NextFloat(0.5f, 0.75f));

            //bool color = Main.rand.NextBool();
            //for(int i = 0; i < 4; i++)
            //    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 4 * i + (Orator.target.Center - Projectile.Center).ToRotation()).ToRotationVector2(), ModContent.ProjectileType<DarkBolt>(), TheOrator.BoltDamage, 0f, -1, 0, 10, color ? 1 : 0);
        }

        CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
        GeneralParticleHandler.SpawnParticle(pulse);
        CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
        GeneralParticleHandler.SpawnParticle(explosion);
    }

    internal int frameX = 0;

    private void FindFrame()
    {
        frameX = (int)Pose;
        if (Pose != 0)
        {
            Projectile.frame = 0;
            return;
        }

        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 8)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame >= Main.projFrames[Type] - 1)
                Projectile.frame = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor) => false;

    public static void DrawSelf(Projectile p)
    {
        Rectangle frame = TextureAssets.Projectile[p.type].Frame(6, 9, p.As<ShadowGrasp>().frameX, p.frame);

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (p.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(TextureAssets.Projectile[p.type].Value, p.Center - Main.screenPosition, frame, Color.White, p.rotation, frame.Size() * 0.5f, p.scale, spriteEffects, 0);
    }

    public override void PostDraw(Color lightColor)
    {
        if (lightColor != EmpyreanMetaball.BorderColor)
            return;

        Texture2D texture = Details.Value;
        Rectangle frame = Details.Frame(6, 9, frameX, Projectile.frame);

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, EmpyreanMetaball.BorderColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, spriteEffects, 0);
    }
}

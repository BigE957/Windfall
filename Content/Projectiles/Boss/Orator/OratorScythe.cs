using CalamityMod.NPCs.TownNPCs;
using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class OratorScythe : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaThrow";
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 90;
        Projectile.scale = 1f;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = int.MaxValue;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.netImportant = true;
    }

    private int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    private enum BehaviorType
    {
        Chase,
        SawThrow,
    }
    private BehaviorType behavior
    {
        get => (BehaviorType)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    Vector2 ToTarget = Vector2.Zero;

    public override void AI()
    {
        Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
        NPC orator = null;
        if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
        {
            orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];

            if (orator.As<TheOrator>().AIState != TheOrator.States.DarkFlight && orator.As<TheOrator>().AIState != TheOrator.States.DarkCrush)
                behavior = (BehaviorType)4;
        }
        else
        {
            if(CalamityUtils.ManhattanDistance(Projectile.Center, target.Center) > 800f)
                Projectile.active = false;
            return;
        }
        
        switch(behavior)
        {
            case BehaviorType.Chase:
                Projectile.rotation += 0.01f * (5 + Projectile.velocity.Length());
                if (Time <= 30)
                {
                    float reelBackSpeedExponent = 2.6f;
                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time, true);
                    float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                    Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                }
                else
                {
                    if (Time == 31)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -40;
                    else
                    {
                        Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), CalamityWorld.death ? 0.0015f : 0.00125f * (Time - 30));
                        Projectile.velocity *= CalamityWorld.death ? 0.97f : 0.975f;
                        if (Projectile.velocity.LengthSquared() < 25)
                            Time = 0;
                    }
                }
                break;
            case BehaviorType.SawThrow:
                Projectile border = Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<OratorBorder>());
                float radius = border.As<OratorBorder>().Radius;
                if (Time >= 0)
                {
                    Projectile.rotation += 0.01f * (5 + Projectile.velocity.Length());
                    if (Time == 0)
                        ToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                    int delay = 30;
                    if (Time <= delay)
                    {
                        Projectile.hostile = false;

                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, delay, Time, true);
                        float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = ToTarget * -reelBackSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    }
                    else
                    {
                        Projectile.hostile = true;
                        if (Time == delay + 1)
                            Projectile.velocity = ToTarget * 40;
                        //Projectile.velocity *= CalamityWorld.death ? 0.97f : 0.975f;
                        if ((border.Center - Projectile.Center).Length() > radius && Time >= 48)
                            Time = -75;

                    }
                }
                else
                {
                    Projectile.hostile = true;
                    if (Time > -30)
                    {
                        Projectile.velocity = (orator.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Clamp(Projectile.velocity.Length() * 1.05f, 0f, 30f);
                        Projectile.rotation += 0.01f * (5 + Projectile.velocity.Length());
                        if ((Projectile.Center - orator.Center).LengthSquared() < 256)
                            Time = -1;
                    }
                    else
                    {
                        Projectile.rotation += 0.33f;

                        Vector2 toBorder = (border.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                        Projectile.Center = border.Center - toBorder * (radius - 64);
                       
                        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Projectile.Center - border.Center).SafeNormalize(Vector2.Zero) * 48f + toBorder.RotatedBy(PiOver2) * Main.rand.NextFloat(-48f, 48f), toBorder.RotatedBy(Main.rand.NextFloat(-PiOver4 / 2f, PiOver4 / 2f)) * Main.rand.NextFloat(4, 10), Main.rand.NextFloat(30f, 60f));
                        if (Main.netMode != NetmodeID.MultiplayerClient && Time % 6 == 0)
                            Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center + (Projectile.Center - border.Center).SafeNormalize(Vector2.Zero) * 72f, (border.Center - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-PiOver2, PiOver2)) * 6f, ModContent.ProjectileType<DarkGlob>(), TheOrator.GlobDamage, 0f, -1, 2, 0.5f);
                    }

                }
                break;
            default:
                Projectile.velocity = Projectile.velocity.RotateTowards((orator.Center - Projectile.Center).ToRotation(), 0.09f).SafeNormalize(Vector2.Zero) * Clamp(Projectile.velocity.Length() * 1.05f, 0f, 30f);
                if (Projectile.Hitbox.Intersects(orator.Hitbox))
                    Projectile.active = false;
                break;
        }

        //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() / 3f);
        Time++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White * Projectile.Opacity, 1);
        Main.EntitySpriteDraw(tex, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        return false;
    }
}

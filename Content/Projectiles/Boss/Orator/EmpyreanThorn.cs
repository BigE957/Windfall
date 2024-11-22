using Terraria;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class EmpyreanThorn : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/EmpyreanThorn";
    public ref float InitialScale => ref Projectile.ai[2];
    public ref float StabVelocity => ref Projectile.ai[1];
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
    }

    public override void SetDefaults()
    {
        Main.projFrames[Projectile.type] = 6;
        Projectile.width = 200;
        Projectile.height = 32;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
        Projectile.frame = Main.rand.Next(5);
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 420;
        Projectile.penetrate = -1;
        Projectile.damage = 100;
    }
    int aiCounter = 0;
    Vector2 initialPoint = Vector2.Zero;
    public override void OnSpawn(IEntitySource source)
    {
        if (Projectile.ai[0] != -1)
        {
            Projectile.timeLeft = (int)Projectile.ai[0] + 180;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity = Vector2.Zero;
            Projectile.scale = InitialScale + Main.rand.NextFloat(-0.5f, 0.5f);
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
            initialPoint = Projectile.Center + (Projectile.rotation.ToRotationVector2() * 64f * Projectile.scale);
        }
    }

    public override void AI()
    {
        if (Projectile.ai[0] == -1)
        {
            if (Projectile.velocity.Length() >= 1f && Projectile.timeLeft > 60)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 2;
            }
            else
                Projectile.velocity = Vector2.Zero;

            if (Projectile.timeLeft <= 60)
            {
                Projectile.Opacity -= 0.1f;
                Projectile.velocity -= Projectile.rotation.ToRotationVector2() * 4f;
            }
        }
        else
        {
            int delay = (int)Projectile.ai[0];
            
            if (aiCounter < delay + 30)
                EmpyreanMetaball.SpawnDefaultParticle(initialPoint, Projectile.rotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f)) * Main.rand.NextFloat(16f, 18f), 40f);
            if(aiCounter >= delay)
            {
                if(aiCounter == delay)
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * StabVelocity;
                if (aiCounter < delay + 60)
                    Projectile.velocity *= 0.9f;
                else
                    Projectile.velocity -= Projectile.rotation.ToRotationVector2() * 0.15f;
            }
            aiCounter++;
        }
    }

    public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle frame = tex.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

        Color drawColor = Color.White;
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 v = Projectile.rotation.ToRotationVector2();
        Vector2 lineStart = Projectile.Center - (v * Projectile.width * 0.5f);
        Vector2 lineEnd = Projectile.Center + (v * Projectile.width * 0.5f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, Projectile.height, ref collisionPoint);
    }

    public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI, List<int> overWiresUI)
    {
        if(Projectile.ai[0] == -1)
            drawCacheProjsBehindNPCsAndTiles.Add(index);
    }
}


namespace Windfall.Content.Projectiles.Boss.Orator;

public class DarkBolt : ModProjectile
{
    public override string Texture => "Windfall/Assets/Projectiles/Boss/NailShot";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.damage = 100;
        Projectile.hostile = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 250;
        Projectile.tileCollide = false;
        Projectile.Calamity().DealsDefenseDamage = true;
    }
    private int aiCounter
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    private float Velocity
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    public Color drawColor = Color.White;
    Vector2 DirectionalVelocity = Vector2.Zero;
    public override void OnSpawn(IEntitySource source)
    {
        DirectionalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
        if(drawColor == Color.White)
            drawColor = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);
    }
    public override void AI()
    {
        Projectile.rotation = DirectionalVelocity.ToRotation();
        Projectile.velocity = DirectionalVelocity.SafeNormalize(Vector2.UnitX) * (Velocity / 2);
        Velocity += 1f;
        aiCounter++;

        if (Velocity > 5 && Main.rand.NextBool(3))
        {
            Vector2 position = new(Projectile.position.X + 39, Projectile.Center.Y);
            Vector2 rotation = Projectile.rotation.ToRotationVector2();
            rotation *= -8;
            Dust dust = Dust.NewDustPerfect(position + rotation, DustID.RainbowTorch);
            dust.scale = Main.rand.NextFloat(1f, 2f);
            dust.noGravity = true;
            dust.color = drawColor;
        }
        Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Location = new(hitbox.Location.X + 39, hitbox.Center.Y);
        Vector2 rotation = Projectile.rotation.ToRotationVector2();
        rotation *= 39;           
        hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
    }
    public override bool PreDraw(ref Color lightColor)
    {
        CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor * 0.75f, 2);
        return false;
    }
}

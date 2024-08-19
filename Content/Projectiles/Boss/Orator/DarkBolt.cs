namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkBolt : ModProjectile
    {
        public override string Texture => "Windfall/Assets/Projectiles/Boss/NailShot";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.damage = 100;
            Projectile.hostile = true;
            Projectile.penetrate = 3;
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
        Vector2 DirectionalVelocity = Vector2.Zero;
        public override void AI()
        {
            
            if (aiCounter == 0)
                DirectionalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.rotation = DirectionalVelocity.ToRotation();
            Projectile.spriteDirection = (int)Projectile.rotation;
            Projectile.velocity = DirectionalVelocity.SafeNormalize(Vector2.UnitX) * (Velocity / 2);
            Velocity += 1f;
            aiCounter++;

            Vector2 position = new(Projectile.position.X + 39, Projectile.Center.Y);
            Vector2 rotation = Projectile.rotation.ToRotationVector2();
            rotation *= -8;
            Dust dust = Dust.NewDustPerfect(position + rotation, DustID.Terra);
            dust.scale = Main.rand.NextFloat(1f, 2f);
            dust.noGravity = true;
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Location = new(hitbox.Location.X + 39, hitbox.Center.Y);
            Vector2 rotation = Projectile.rotation.ToRotationVector2();
            rotation *= 39;           
            hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
        }
    }
}

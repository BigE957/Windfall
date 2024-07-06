using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkGlob : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.damage = TheOrator.GlobDamage;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 390;
            Projectile.scale = 0f;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        private float MaxSize
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                if (Projectile.timeLeft > 30)
                    if (Projectile.scale < MaxSize)
                        Projectile.scale += MaxSize/30;
                    else
                        Projectile.scale = MaxSize;                
                if (Projectile.scale < MaxSize)
                    Projectile.hostile = false;
                else
                    Projectile.hostile = true;

                if (Projectile.velocity.Length() > 0.5f)
                {
                    Projectile.velocity -= Projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.2f;
                    if (Projectile.hostile)
                        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + Projectile.velocity, Vector2.Zero, Projectile.scale * 60);
                }
                else
                {
                    Projectile.velocity = Vector2.Zero;                    
                    if (Projectile.timeLeft < 30)
                        Projectile.scale -= MaxSize / 30;
                }
            }
            else
            {
                if (MaxSize > 1.5f)
                    Projectile.scale = MaxSize;
                if (Projectile.scale < MaxSize)
                {
                    Projectile.scale += 1 / 60f;
                    Projectile.timeLeft = 500;
                    Projectile.hostile = false;
                }
                else
                    Projectile.hostile = true;
                Projectile.velocity.Y += CalamityWorld.death ? 0.3f : CalamityWorld.revenge ? 0.25f : Main.expertMode ? 0.2f : 0.15f;
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center , Vector2.Zero, Projectile.scale * 60);
            }
            Projectile.Hitbox = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y, (int)(70 * MaxSize), (int)(70 * MaxSize));
            Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = Color.White;
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
            return false;
        }       
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CircularHitboxCollision(Projectile.Center, (projHitbox.Width / 2) * (Projectile.scale / MaxSize), targetHitbox))
                return true;
            return false;
        }
        
    }
}

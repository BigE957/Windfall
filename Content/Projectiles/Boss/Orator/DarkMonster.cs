using CalamityMod.Graphics.Metaballs;
using CalamityMod.Projectiles.Magic;
using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class DarkMonster : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";
        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 320;
            Projectile.damage = 100;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.scale = 1f;
            Projectile.alpha = 0;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }
        private readonly float Acceleration = CalamityWorld.death ? 0.55f : CalamityWorld.revenge ? 0.5f : Main.expertMode ? 0.45f : 0.4f;
        private readonly int MaxSpeed = CalamityWorld.death ? 15 : CalamityWorld.revenge ? 12 : 10;
        private int aiCounter = 0;
        public override void AI()
        {
            bool dying = (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) || Projectile.ai[0] == 1);
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (dying)
            {
                Projectile.hostile = false;
                Projectile.scale -= 0.1f;
                if (Projectile.scale <= 0f)
                    Projectile.active = false;
            }
            else
            {
                if (aiCounter == 0)
                    Projectile.scale = 0;
                Projectile.hostile = true;
            }
            if (aiCounter > 60)
            {
                if (Projectile.Center.X > target.Center.X)
                    Projectile.velocity.X -= Acceleration;
                if (Projectile.Center.X < target.Center.X)
                    Projectile.velocity.X += Acceleration;
                if (Projectile.Center.Y > target.Center.Y)
                    Projectile.velocity.Y -= Acceleration;
                if (Projectile.Center.Y < target.Center.Y)
                    Projectile.velocity.Y += Acceleration;
                
            }
            else
            {
                Projectile.scale += 5 / 60f;
                Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) / 5;
            }
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;
            aiCounter++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            EmitGhostGas(aiCounter);
        }
        public void EmitGhostGas(int counter)
        {
            float gasSize = 40 * Projectile.scale;
            EmpyreanMetaball.SpawnParticle(Projectile.Center - Projectile.velocity * (2 * Projectile.scale), Vector2.Zero, gasSize);

            gasSize = 20 * Projectile.scale;
            EmpyreanMetaball.SpawnParticle(Projectile.Center - (Projectile.velocity.RotatedBy(Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
            EmpyreanMetaball.SpawnParticle(Projectile.Center - (Projectile.velocity.RotatedBy(-Pi / 4) * (2 * Projectile.scale)), Vector2.Zero, gasSize);
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
}

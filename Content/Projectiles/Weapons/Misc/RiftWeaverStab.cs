using CalamityMod.Projectiles.BaseProjectiles;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.Weapons.Misc
{
    public class RiftWeaverStab : BaseShortswordProjectile
    {
        public override string Texture => "Windfall/Assets/Items/Weapons/Misc/RiftWeaverProj";
        public override float FadeInDuration => 4f;
        public override float FadeOutDuration => 0f;
        public override float TotalDuration => 12f;
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(24); // This sets width and height to the same value (important when projectiles can rotate)
            Projectile.aiStyle = -1; // Use our own AI to customize how it behaves, if you don't want that, keep this at ProjAIStyleID.ShortSword. You would still need to use the code in SetVisualOffsets() though
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Default;
            Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this
            Projectile.extraUpdates = 1; // Update 1+extraUpdates times per tick
            Projectile.timeLeft = 360; // This value does not matter since we manually kill it earlier, it just has to be higher than the duration we use in AI
            Projectile.hide = true; // Important when used alongside player.heldProj. "Hidden" projectiles have special draw conditions
            Projectile.timeLeft = 360;
        }

        public override void SetVisualOffsets()
        {
            const int HalfSpriteWidth = 54 / 2;
            const int HalfSpriteHeight = 54 / 2;

            int HalfProjWidth = Projectile.width / 2;
            int HalfProjHeight = Projectile.height / 2;

            DrawOriginOffsetX = 0;
            DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
            DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
        }

        public override void ExtraBehavior()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 12), DustID.Mythril);
                dust.scale = Main.rand.NextFloat(0.15f, 0.6f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.5f;
            }

            float armPointingDirection = ((Owner.Calamity().mouseWorld - Owner.MountedCenter).ToRotation());
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armPointingDirection - MathHelper.PiOver2);
            Owner.heldProj = Projectile.whoAmI;

            Projectile.Center = Projectile.Center + new Vector2(0, 1.5f);
        }
        private static NPC Target = null;
        private static int hitCount = 0;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Target == null || Target != target)
            {
                hitCount = 0;
                Target = target;
            }
            hitCount++;
            if (hitCount >= 5)
            {
                hitCount = 0;
                Owner.velocity = (target.Center - Owner.Center).SafeNormalize(Vector2.Zero) * -12f;
                if(Math.Abs(Owner.velocity.Y) < 8f)
                    Owner.velocity.Y = -8;
                if (Owner.velocity.Y > 0)
                    Owner.velocity.Y *= -1;
                Owner.velocity.X /= 2f;
                if (target.type == ModContent.NPCType<PortalMole>())
                    target.life = 0;
            }
            else if (Owner.velocity.Y != 0)
            {
                Owner.velocity.X /= 2f;
                Owner.velocity.Y = -4f;
            }
        }
    }
}

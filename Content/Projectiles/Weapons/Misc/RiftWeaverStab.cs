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
        private bool SpacialDamage 
        {  
            get => Projectile.ai[2] != 0; 
            set => Projectile.ai[2] = value ? 1 : 0;
        }
        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(24); // This sets width and height to the same value (important when projectiles can rotate)
            Projectile.aiStyle = -1; // Use our own AI to customize how it behaves, if you don't want that, keep this at ProjAIStyleID.ShortSword. You would still need to use the code in SetVisualOffsets() though
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.scale = 1f;
            Projectile.DamageType = SpacialDamage ? DamageClass.Default : DamageClass.Melee;
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
        public override bool? CanHitNPC(NPC target) => target.type == ModContent.NPCType<PortalMole>() || !SpacialDamage;
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
                bool colorCombo = Main.rand.NextBool();
                Particle pulse = new PulseRing(target.Center + (Owner.Center - target.Center) / 7f, Vector2.Zero, colorCombo ? Color.Cyan : Color.LimeGreen, 0f, 0.5f, 30);
                GeneralParticleHandler.SpawnParticle(pulse);
                pulse = new PulseRing(target.Center + (Owner.Center - target.Center) / 7f, Vector2.Zero, colorCombo ? Color.LimeGreen : Color.Cyan, 0f, 0.3f, 30);
                GeneralParticleHandler.SpawnParticle(pulse);

                Owner.velocity.Y = -12;
                Owner.velocity.X = target.Center.X > Owner.Center.X ? -4 : 4;

                if (target.type == ModContent.NPCType<PortalMole>())
                    target.StrikeInstantKill();

                hitCount = 0;
            }
            else
            {
                Particle pulse = new PulseRing(target.Center + (Owner.Center - target.Center) / 7f, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan : Color.LimeGreen, 0f, 0.3f, 30);
                GeneralParticleHandler.SpawnParticle(pulse);
                if (Owner.velocity.Y != 0)
                {
                    Owner.velocity.X /= 2f;
                    if (Owner.velocity.Y / 2f < -4f)
                        Owner.velocity.Y /= 2f;
                    else
                        Owner.velocity.Y = -4f;
                }
            }
            Projectile.damage = 0;
        }
    }
}

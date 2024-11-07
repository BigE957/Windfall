using CalamityMod.Projectiles.Summon;
using CalamityMod.World;
using Luminance.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator
{
    public class OratorScythe : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/Rogue/CelestusMiniScythe";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.scale = 2f;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            behavior = BehaviorType.Chase;
        }
        private int Time = 0;
        private enum BehaviorType
        {
            Chase,
            Circle,
        }
        private BehaviorType behavior = BehaviorType.Chase;
        public override void AI()
        {
            Projectile.rotation += 0.01f * (5 + Projectile.velocity.Length());
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            NPC orator = null;
            if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            {
                orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                target = orator.As<TheOrator>().target;

                if (orator.As<TheOrator>().AIState == TheOrator.States.DarkTides || orator.As<TheOrator>().AIState == TheOrator.States.Defeat)
                    behavior = (BehaviorType)4;
            }
            /*
            else
            {
                if(CalamityUtils.ManhattanDistance(Projectile.Center, target.Center) > 800f)
                    Projectile.active = false;
                return;
            }
            */
            bool attackBool = false;
            
            switch(behavior)
            {
                case BehaviorType.Chase:
                    if(Time <= 30)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time, true);
                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    }
                    else
                    {
                        if (Time == 31)
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -40;
                        else
                        {
                            Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), CalamityWorld.death ? 0.00175f : 0.0015f * (Time - 30));
                            Projectile.velocity *= CalamityWorld.death ? 0.97f : 0.975f;
                            if (Projectile.velocity.LengthSquared() < 25)
                                Time = 0;
                        }
                    }
                    break;
                case BehaviorType.Circle:
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
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White * Projectile.Opacity, 2);
            Main.EntitySpriteDraw(tex, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, Projectile.Size * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
        }
    }
}

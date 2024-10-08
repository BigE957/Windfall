using CalamityMod.Projectiles.Summon;
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
    public class OratorJavelin : ModProjectile
    {
        public new static string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "Windfall/Assets/Projectiles/Boss/OratorJavelin";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 420;
            Projectile.penetrate = -1;
            Projectile.Opacity = 0f;
        }
        public ref float Angle => ref Projectile.ai[1];
        private Color drawColor = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);
        private Vector2 centerPosition = Vector2.Zero;
        public override void OnSpawn(IEntitySource source)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            {
                NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                target = orator.As<TheOrator>().target;

            }
            Angle = (target.Center - Projectile.Center).ToRotation();
            Angle -= PiOver2;
            if (Main.rand.NextBool())
                Angle += Pi;
            Angle += Main.rand.NextFloat(-Pi / 4, Pi / 4);
            Projectile.ai[2] = Main.rand.Next(3);
            centerPosition = Projectile.Center;
            Projectile.netUpdate = true;
        }
        private int Time = 0;
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            {
                NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                centerPosition = orator.Center;
                target = orator.As<TheOrator>().target;
            }
            if (Projectile.Opacity < 1f)
                Projectile.Opacity += 0.05f;
            if (Time < 120)
            {                
                Projectile.Center = centerPosition + Angle.ToRotationVector2() * 150f;
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                Angle += 0.105f * ((120 - Time) / 120f);
            }
            else
            {
                if(Time - 120 < 30)
                {
                    float reelBackSpeedExponent = 2.6f;
                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time - 120, true);
                    float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                    Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                }
                else if(Time - 120 == 30)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -60;
                }
                else
                {
                    Projectile.velocity *= 0.985f;
                }
            }
            //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() / 3f);
            Time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D WhiteOutTexture = ModContent.Request<Texture2D>(Texture + "WhiteOut").Value;
            Color color = Color.Black;
            switch(Projectile.ai[2])
            {
                case 0:
                    color = new(253, 189, 53);
                    break;
                case 1:
                    color = new(255, 133, 187);
                    break;
                case 2:
                    color = new(220, 216, 155);
                    break;
            }

            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], color * Projectile.Opacity, 2, texture: WhiteOutTexture);
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = tex.Frame(1, 3, 0, (int)Projectile.ai[2]);

            Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            /*
            if (Projectile.timeLeft > 90)
                return;

            Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.ai[2] == 0 ? 0 : 1)).Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float ratio = 0f;
            if (Projectile.timeLeft <= 90)
                ratio = (90 - Projectile.timeLeft) / 60f;
            ratio = Clamp(ratio, 0f, 1f);
            Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, WhiteOutTexture.Frame(), Color.White, Projectile.rotation, WhiteOutTexture.Frame().Size() * 0.5f, Projectile.scale * ratio, SpriteEffects.None);
            */        
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 10; i++)
            {
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 20f));
            }
        }
    }
}

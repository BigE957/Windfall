using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Summon;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 420;
            Projectile.penetrate = -1;
            Projectile.Opacity = 0f;
            Projectile.damage = 50;
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
        private Player impaleTarget = null;
        private int initialTargetDir = 0;
        private Vector2 impaleOffset = Vector2.Zero;
        private float impaleRotation = 0f;
        private bool haveImpaled = false;
        public override void AI()
        {
            if (Projectile.Opacity < 1f)
                Projectile.Opacity += 0.05f;

            if (haveImpaled)
            {
                //Main.NewText("Have Impaled");

                if (impaleTarget != null)
                {
                    //Main.NewText("Impaling");
                    
                    if (impaleTarget.active && !impaleTarget.dead)
                    {
                        impaleTarget.AddBuff(ModContent.BuffType<WhisperingDeath>(), 2);
                        Projectile.Center = impaleTarget.Center + new Vector2(impaleOffset.X * (impaleTarget.direction != initialTargetDir ? -1 : 1), impaleOffset.Y + impaleTarget.gfxOffY);
                        //Projectile.velocity = impaleTarget.velocity;
                        Projectile.rotation =  (impaleTarget.direction != initialTargetDir ? (new Vector2(-impaleRotation.ToRotationVector2().X, impaleRotation.ToRotationVector2().Y)).ToRotation() : impaleRotation);
                    }
                    else
                    {
                        impaleTarget = null;
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(1f, 2f);
                    }
                }
                else
                {
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    Projectile.velocity.Y -= 0.03f;
                }
            }
            else
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
                if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
                {
                    NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                    centerPosition = orator.Center;
                    target = orator.As<TheOrator>().target;
                }

                if (Time < 120)
                {
                    Projectile.Center = centerPosition + Angle.ToRotationVector2() * 150f;
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    Angle += 0.105f * ((120 - Time) / 120f);
                }
                else
                {
                    if (Time - 120 < 30)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time - 120, true);
                        float reelBackSpeed = MathHelper.Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                        Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    }
                    else if (Time - 120 == 30)
                    {
                        Projectile.hostile = true;
                        Projectile.tileCollide = true;
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -60;
                    }
                    else
                    {
                        Projectile.velocity *= 0.985f;
                    }
                }
            }
            if (Projectile.timeLeft <= 90)
            {
                float ratio = 0f;
                ratio = (90 - Projectile.timeLeft) / 60f;
                ratio = Clamp(ratio, 0f, 1f);
                Vector2 spawnPos = Projectile.Center - (Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-64f, 64f));
                if (!Main.tile[spawnPos.ToTileCoordinates()].IsTileSolid())
                    EmpyreanMetaball.SpawnDefaultParticle(spawnPos, Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextFloat(10f, 20f));

            }
            //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() / 3f);
            Time++;           
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            {
                NPC orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())];
                target = orator.As<TheOrator>().target;
            }

            if ((target.Center - Projectile.Center).Length() > 700f || PointInTriangle(Projectile.Hitbox.Center(), target.Center, target.Center + ((Projectile.rotation + Pi).ToRotationVector2().RotatedBy(PiOver4) * 1000f), target.Center + ((Projectile.rotation + Pi).ToRotationVector2().RotatedBy(-PiOver4) * 1000f)))
            {
                Projectile.velocity = oldVelocity;
                return false;
            }
            Projectile.damage = 0;
            Projectile.velocity = Vector2.Zero;
            for (int i = 0; i < 64; i++)
            {
                Projectile.Center -= Projectile.rotation.ToRotationVector2();
                Rectangle hitbox = new((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                ModifyDamageHitbox(ref hitbox);
                if (!Main.tile[hitbox.Center.ToVector2().ToTileCoordinates()].IsTileSolid())
                {
                    Projectile.Center += Projectile.rotation.ToRotationVector2();
                    hitbox = new((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                    ModifyDamageHitbox(ref hitbox);

                    Projectile.Center = hitbox.Center() + ((Projectile.Center - hitbox.Center())/2f);
                }
                
            }
            Projectile.timeLeft = 180;
            return false;
        }

        private static float sign(Vector2 p1, Vector2 p2, Vector2 p3) => (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);

        private static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.Center += (Projectile.velocity);
            Projectile.velocity = Vector2.Zero;
            Projectile.damage = 0;
            Projectile.timeLeft = 240;
            impaleTarget = target;
            initialTargetDir = target.direction;
            impaleOffset = target.Center - Projectile.Center;
            impaleRotation = Projectile.rotation;
            haveImpaled = true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 16; i++)
            {
                Vector2 spawnPos = Projectile.Center - (Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(-64f, 64f));
                if (!Main.tile[spawnPos.ToTileCoordinates()].IsTileSolid())
                    EmpyreanMetaball.SpawnDefaultParticle(spawnPos, Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextFloat(10f, 20f));
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 rotation = Projectile.rotation.ToRotationVector2() * 40f;
            hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if ((Time < 150 || Projectile.velocity.LengthSquared() > 0f) && impaleTarget == null)
            {
                Texture2D WhiteOutTexture = ModContent.Request<Texture2D>(Texture + "WhiteOut").Value;
                Color color = Color.Black;
                switch (Projectile.ai[2])
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
            }
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = tex.Frame(1, 3, 0, (int)Projectile.ai[2]);

            SpriteEffects effects = SpriteEffects.None;
            //if (impaleTarget != null && impaleTarget.direction != initialTargetDir)
            //    effects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, effects);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI, List<int> overWiresUI)
        {
            if (Time >= 150)
            {
                Projectile.hide = true;
                drawCacheProjsBehindNPCsAndTiles.Add(index);
            }
        }
    }
}

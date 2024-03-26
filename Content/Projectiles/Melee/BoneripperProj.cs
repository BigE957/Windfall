using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Melee;
using Terraria.Enums;

namespace Windfall.Content.Projectiles.Melee
{
    public class BoneripperProj : ModProjectile
    {
        internal int boneripperHits = 0;
        internal int boneripperCharge = 0;
        public enum AIState
        {
            Spinning,
            Throwing,
        }
        AIState state = AIState.Spinning;
        public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Boneripper";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 90;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.reuseDelay = 2;
            if (state == AIState.Spinning)
            {
                if (!player.channel)
                {
                    if (boneripperCharge > 0)
                    {
                        Projectile.ai[0] = 0f;
                        Projectile.alpha = 0;
                        Projectile.hide = false;
                        Projectile.timeLeft = 200 + 400 * boneripperCharge;
                        Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 15f;
                        boneripperHits = 0;
                        state = AIState.Throwing;
                        return;
                    }
                    else
                    {
                        Projectile.Kill();
                        player.reuseDelay = 2;
                        return;
                    }

                }
                if (player.dead)
                {
                    Projectile.Kill();
                    player.reuseDelay = 2;
                    return;
                }
                float num = 50f;
                int num2 = Math.Sign(Projectile.velocity.X);
                Projectile.velocity = new Vector2(num2, 0f);
                if (Projectile.ai[0] == 0f)
                {
                    Projectile.rotation = new Vector2(num2, 0f - player.gravDir).ToRotation() + MathHelper.ToRadians(135f);
                    if (Projectile.velocity.X < 0f)
                    {
                        Projectile.rotation -= MathF.PI / 2f;
                    }
                }

                Projectile.ai[0] += 1f;
                Projectile.rotation += MathF.PI * 4f / num * num2;
                int num3 = (player.SafeDirectionTo(Main.MouseWorld).X > 0f).ToDirectionInt();
                if (Projectile.ai[0] % num > num * 0.5f && num3 != Projectile.velocity.X)
                {
                    player.ChangeDir(num3);
                    Projectile.velocity = Vector2.UnitX * num3;
                    Projectile.rotation -= MathF.PI;
                    Projectile.netUpdate = true;
                }

                SpawnDust(num2);
                VisibilityAndLight();
                PositionAndRotation(player);
            }
            else if (state == AIState.Throwing)
            {
                // Boomerang rotation
                Projectile.rotation += 0.4f * Projectile.direction;

                // Boomerang sound
                if (Projectile.soundDelay == 0)
                {
                    Projectile.soundDelay = 8;
                    SoundEngine.PlaySound(SoundID.Item7, Projectile.position);
                }
                if (Projectile.ai[0] == 0f)
                {
                    //enemy tracking if above charge 1
                    if (boneripperCharge > 1)
                        Projectile.velocity = (Projectile.velocity.SafeNormalize(Vector2.UnitX) + (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) / 5f) * 15;

                    // Returns after some number of frames in the air
                    if (Projectile.timeLeft < 400 + 200 * boneripperCharge - 40 || boneripperCharge >= 2 && boneripperHits > 5 + 5 * boneripperCharge)
                        Projectile.ai[0] = 1f;
                }
                else if (Projectile.ai[0] == 1f)
                {
                    float returnSpeed = 14f;
                    float acceleration = 0.6f;
                    Vector2 playerVec = player.Center - Projectile.Center;
                    float dist = playerVec.Length();
                    // Delete the projectile if it's excessively far away.
                    if (dist > 3000f)
                        Projectile.Kill();

                    playerVec.Normalize();
                    playerVec *= returnSpeed;

                    // Home back in on the player.
                    if (Projectile.velocity.X < playerVec.X)
                    {
                        Projectile.velocity.X += acceleration;
                        if (Projectile.velocity.X < 0f && playerVec.X > 0f)
                            Projectile.velocity.X += acceleration;
                    }
                    else if (Projectile.velocity.X > playerVec.X)
                    {
                        Projectile.velocity.X -= acceleration;
                        if (Projectile.velocity.X > 0f && playerVec.X < 0f)
                            Projectile.velocity.X -= acceleration;
                    }
                    if (Projectile.velocity.Y < playerVec.Y)
                    {
                        Projectile.velocity.Y += acceleration;
                        if (Projectile.velocity.Y < 0f && playerVec.Y > 0f)
                            Projectile.velocity.Y += acceleration;
                    }
                    else if (Projectile.velocity.Y > playerVec.Y)
                    {
                        Projectile.velocity.Y -= acceleration;
                        if (Projectile.velocity.Y > 0f && playerVec.Y < 0f)
                            Projectile.velocity.Y -= acceleration;
                    }

                    // Delete the projectile if it touches its owner.
                    if (Main.myPlayer == Projectile.owner)
                        if (Projectile.Hitbox.Intersects(player.Hitbox))
                            Projectile.Kill();
                }
                int num2 = Math.Sign(Projectile.velocity.X);
                SpawnDust(num2);
            }
        }
        private void SpawnDust(int direction)
        {
            float num = Projectile.rotation - MathF.PI / 4f * direction;
            Vector2 vector = Projectile.Center + (num + (direction == -1 ? MathF.PI : 0f)).ToRotationVector2() * 30f;
            Vector2 vector2 = num.ToRotationVector2();
            Vector2 vector3 = vector2.RotatedBy(MathF.PI / 2f * Projectile.spriteDirection);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(vector - new Vector2(5f), 10, 10, (int)CalamityDusts.Brimstone, Projectile.velocity.X, Projectile.velocity.Y, 150);
                dust.velocity = Projectile.SafeDirectionTo(dust.position) * 0.1f + dust.velocity * 0.1f;
            }

            for (int i = 0; i < 2; i++)
            {
                float num2 = 1f;
                float num3 = 1f;
                switch (i)
                {
                    case 1:
                        num3 = -1f;
                        break;
                    case 2:
                        num3 = 1.25f;
                        num2 = 0.5f;
                        break;
                    case 3:
                        num3 = -1.25f;
                        num2 = 0.5f;
                        break;
                }

                if (!Main.rand.NextBool(6))
                {
                    Dust dust2 = Dust.NewDustDirect(Projectile.position, 0, 0, (int)CalamityDusts.Brimstone, 0f, 0f, 100);
                    dust2.position = Projectile.Center + vector2 * (60f + Main.rand.NextFloat() * 20f) * num3;
                    dust2.velocity = vector3 * (4f + 4f * Main.rand.NextFloat()) * num3 * num2;
                    dust2.noGravity = true;
                    dust2.noLight = true;
                    dust2.scale = 0.5f;
                    if (Main.rand.NextBool(4))
                    {
                        dust2.noGravity = false;
                    }
                }
            }
        }

        private void PositionAndRotation(Player player)
        {
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
            Vector2 zero = Vector2.Zero;
            if (state == AIState.Spinning)
            {
                Projectile.Center = vector + zero;
            }
            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = player.itemAnimation = 2;
            player.itemRotation = MathHelper.WrapAngle(Projectile.rotation);
        }
        private void VisibilityAndLight()
        {
            Lighting.AddLight(Projectile.Center, 1.45f, 1.22f, 0.58f);
            Projectile.alpha -= 128;
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
        }
        public static readonly SoundStyle ChargeSound = new("CalamityMod/Sounds/Custom/Yharon/YharonFireball2");
        public static readonly SoundStyle MaxChargeSound = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneFireblastImpact");
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Colliding(Projectile.Hitbox, target.Hitbox))
            {
                boneripperHits++;
                if (state == AIState.Spinning)
                {
                    if (boneripperHits == 10)
                    {
                        if (boneripperCharge < 3)
                        {
                            boneripperCharge++;
                            if (boneripperCharge == 3)
                            {
                                SoundEngine.PlaySound(MaxChargeSound, Projectile.Center);
                            }
                            else
                            {
                                SoundEngine.PlaySound(ChargeSound, Projectile.Center);
                            }
                        }
                        boneripperHits = 0;
                    }
                    target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120 + 60 * (boneripperCharge + 1));
                }
                else
                {
                    if (boneripperCharge > 1)
                    {
                        if (boneripperCharge == 3)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<BrimlanceHellfireExplosion>(), (int)(Projectile.damage * 0.35), 0f);
                        target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
                    }
                }
            }
        }
        public override void CutTiles()
        {
            float num = 60f;
            float f = Projectile.rotation - MathF.PI / 4f * Math.Sign(Projectile.velocity.X);
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Utils.PlotTileLine(Projectile.Center + f.ToRotationVector2() * (0f - num), Projectile.Center + f.ToRotationVector2() * num, Projectile.width * Projectile.scale, DelegateMethods.CutTiles);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }

            //float f = Projectile.rotation - MathF.PI / 4f * (float)Math.Sign(Projectile.velocity.X);
            //float collisionPoint = 0f;
            //float num = 110f;
            //if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + f.ToRotationVector2() * (0f - num), Projectile.Center + f.ToRotationVector2() * num, 23f * Projectile.scale, ref collisionPoint))
            //{
            //    return true;
            //}

            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (state == AIState.Spinning)
            {
                Texture2D value = ModContent.Request<Texture2D>(Texture).Value;
                Vector2 vector = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Rectangle value2 = new(0, 0, value.Width, value.Height);
                Vector2 vector2 = value.Size() / 2f;
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (Projectile.spriteDirection == -1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }
                Main.EntitySpriteDraw(value, vector, (Rectangle?)value2, lightColor, Projectile.rotation, vector2, Projectile.scale, spriteEffects, 0);
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}

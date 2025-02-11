using CalamityMod.Items;
using Terraria.Enums;

namespace Windfall.Content.Items.Weapons.Melee;
public class Apotelesma : ModItem, ILocalizedModType
{
    internal int ApotelesmaCharge = 0;
    internal int HitCounter = 0;

    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/ApotelesmaItem";

    public override void SetDefaults()
    {
        Item.damage = 275;
        Item.knockBack = 7.5f;
        Item.useAnimation = Item.useTime = 25;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.noMelee = true;
        Item.channel = true;
        Item.autoReuse = true;
        Item.shootSpeed = 14f;
        Item.shoot = ModContent.ProjectileType<ApotelesmaProj>();
        Item.width = 180;
        Item.height = 180;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
    }

    public override bool CanUseItem(Player player)
    {
        switch (ApotelesmaProj.state)
        {
            case ApotelesmaProj.AIState.DownSlice:
                ApotelesmaProj.state = ApotelesmaProj.AIState.UpSlice;
                break;
            case ApotelesmaProj.AIState.UpSlice:
                ApotelesmaProj.state = ApotelesmaProj.AIState.Spin;
                break;
            case ApotelesmaProj.AIState.Spin:
                ApotelesmaProj.state = ApotelesmaProj.AIState.DownSlice;
                break;
        }
        return base.CanUseItem(player);
    }

    public override float UseAnimationMultiplier(Player player) => UseTimeMultiplier(player);

    public override float UseTimeMultiplier(Player player)
    {
        return ApotelesmaProj.state switch
        {
            ApotelesmaProj.AIState.DownSlice => 1f,
            ApotelesmaProj.AIState.UpSlice => 1f,
            ApotelesmaProj.AIState.Spin => 2f,
            _ => 1f,
        };
    }

    internal Tuple<Vector2, Vector2, int>[] GemData = new Tuple<Vector2, Vector2, int>[5];

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (ApotelesmaCharge == 0)
            return;

        Player myPlayer = Main.LocalPlayer;

        if (myPlayer.ActiveItem() != Item || !myPlayer.active || myPlayer.dead)
            return;

        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        for(int i = 0; i < ApotelesmaCharge; i++)
        {
            float offsetValue = i - ApotelesmaCharge / 2;
            if (ApotelesmaCharge % 2 == 0)
            {
                if (offsetValue >= 0)
                    offsetValue++;
                offsetValue *= 0.5f;
            }

            Vector2 goalPos = new(24 * offsetValue, 12 * Math.Abs(offsetValue) - 48);
            goalPos += myPlayer.MountedCenter + myPlayer.gfxOffY * Vector2.UnitY;

            int counter = GemData[i].Item3;
            if (counter < 0)
                counter = 0;
            if (counter < 30)
                counter++;
            else
                counter = 30;
            GemData[i] = new(GemData[i].Item1, goalPos, counter);

            Vector2 currentPos;

            if (counter < 30f)
                currentPos = Vector2.Lerp(GemData[i].Item1, GemData[i].Item2, SineOutEasing(counter / 30f, 1));
            else
                currentPos = GemData[i].Item2;
            currentPos.Y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + (offsetValue * 24)) * 4f;
            currentPos.X += (float)Math.Sin(Main.GlobalTimeWrappedHourly + (offsetValue * 24)) * 6f;
            currentPos -= myPlayer.velocity;

            float rotation = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.5f + (offsetValue * 24)) * PiOver4 * 0.34f;

            scale = 1.05f + 0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.5f + (offsetValue * 24));

            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/GemTechGreenGem").Value;

            //Main.spriteBatch.Draw(screwOutlineTex, position, null, Color.Lerp(Color.GreenYellow, Color.White, (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.5f + 0.5f) * outlineOpacity, rotation, screwOutlineTex.Size() / 2f, scale, 0, 0);
            Main.spriteBatch.Draw(tex, currentPos - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2f, scale, 0, 0);
        }

        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }
}

public class ApotelesmaProj : ModProjectile
{
    public enum AIState
    {
        DownSlice,
        UpSlice,
        Spin,
        Throw,
        Rush,
    }
    internal static AIState state = AIState.DownSlice;

    private ref float Time => ref Projectile.ai[2];

    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 190;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = state == AIState.Spin ? 12 : 24;
    }

    public override void AI()
    {
        //Main.NewText(Projectile.whoAmI.ToString() + ": " + Time.ToString());
        //Projectile.Hitbox.Width *= Projectile.scale;
        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;

        if (player.dead)
        {
            Projectile.Kill();
            return;
        }

        if (Time >= 0)
        {
            switch (state)
            {
                case AIState.DownSlice:
                    if (Time == 0)
                    {
                        scytheSlice = true;
                    }
                    if (!scytheSlice)
                    {
                        Projectile.active = false;
                    }
                    break;
                case AIState.UpSlice:
                    if (Time == 0)
                    {
                        scytheSlice = true;
                    }
                    if (!scytheSlice)
                    {
                        Projectile.active = false;
                    }
                    break;
                case AIState.Spin:
                    if (Time == 0)
                    {
                        scytheSpin = true;
                    }
                    if (!scytheSpin)
                    {
                        Projectile.active = false;
                    }
                    break;
            }
        }
        if (state != AIState.Throw)
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);

        if (scytheSpin)
            DoScytheSpin(player);
        if (scytheSlice)
            DoScytheSlice(player, state == AIState.UpSlice);

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //if (!target.IsAnEnemy())
        //    return;

        Player owner = Main.player[Projectile.owner];
        if (owner.HeldItem.type != ModContent.ItemType<Apotelesma>())
            return;
        ((Apotelesma)owner.HeldItem.ModItem).HitCounter++;
        if (((Apotelesma)owner.HeldItem.ModItem).HitCounter >= 6)
        {
            ((Apotelesma)owner.HeldItem.ModItem).HitCounter = 0;
            int charge = ((Apotelesma)owner.HeldItem.ModItem).ApotelesmaCharge;
            if (charge < 5)
            {
                ((Apotelesma)owner.HeldItem.ModItem).GemData[charge] = new(target.Center, Vector2.Zero, 0);
                for(int i = 0; i < charge; i++)
                {
                    Tuple<Vector2, Vector2, int> data = ((Apotelesma)owner.HeldItem.ModItem).GemData[i];
                    ((Apotelesma)owner.HeldItem.ModItem).GemData[i] = new(data.Item2, data.Item2, 0);
                }
                ((Apotelesma)owner.HeldItem.ModItem).ApotelesmaCharge++;
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
        if (!scytheSpin && !scytheSlice)
            return false;

        if (projHitbox.Intersects(targetHitbox))
            return true;

        float f = Projectile.rotation - MathF.PI / 4f * (float)Math.Sign(Projectile.velocity.X);
        float collisionPoint = 0f;
        float num = 110f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + f.ToRotationVector2() * (0f - num), Projectile.Center + f.ToRotationVector2() * num, 23f * Projectile.scale, ref collisionPoint))
        {
            return true;
        }

        return base.Colliding(projHitbox, targetHitbox);
    }

    private bool scytheSpin = false;
    Particle spinSmear = null;
    Particle spinHandle = null;
    float spinDirection = 0;
    float spinRotation = 0;
    int spinCounter = 0;
    const int spinDuration = 48;
    private void DoScytheSpin(Player player)
    {
        if (spinCounter == 0)
        {
            SoundEngine.PlaySound(SoundID.Item71, player.Center);
            spinDirection = (player.velocity.SafeNormalize(Vector2.UnitX * player.direction) * -1).ToRotation();
            spinDirection += PiOver2 + (Pi / 4);
        }

        spinRotation = Lerp(spinDirection, spinDirection + Pi * 4f, SineOutEasing(spinCounter / (float)spinDuration, 1));

        if (spinCounter > (spinDuration / 2))
        {
            //if (spinSmear != null)
            {
                if (spinCounter < spinDuration)
                {
                    /*
                    spinSmear.Rotation = spinDirection.ToRotation() + MathHelper.PiOver2;
                    spinSmear.Time = 0;
                    spinSmear.Position = player.Center;
                    spinSmear.Scale = 1.9f;
                    spinSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, spinCounter / 40f);
                    spinSmear.Color.A = (byte)(255 - 255 * MathHelper.Clamp((spinCounter - 15) / 15f, 0f, 1f));
                    */
                }
                else
                {
                    //if (spinSmear != null)
                    //    GeneralParticleHandler.RemoveParticle(spinSmear);
                    spinSmear = null;

                    //if (spinHandle != null)
                    //    GeneralParticleHandler.RemoveParticle(spinHandle);
                    spinHandle = null;

                    scytheSpin = false;
                    spinCounter = 0;
                    return;
                }
            }
        }
        else if (false)
        {
            if (spinHandle == null)
            {
                spinHandle = new SemiCircularSmearVFX(player.Center, Color.LimeGreen, spinRotation, 1.5f, Vector2.One);
                GeneralParticleHandler.SpawnParticle(spinHandle);
            }
            if (spinSmear == null)
            {
                spinSmear = new CircularSmearVFX(player.Center, Color.LimeGreen, spinRotation, 1.5f);
                GeneralParticleHandler.SpawnParticle(spinSmear);
                spinSmear.Color.A = 0;
            }
            else
            {
                spinSmear.Rotation = spinRotation + MathHelper.PiOver2;
                spinSmear.Time = 0;
                spinSmear.Position = player.Center;
                spinSmear.Scale = 1.9f;
                spinSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, spinCounter / 40f);
                spinSmear.Color.A = (byte)(255 * MathHelper.Clamp((spinCounter / 10f), 0f, 1f));
            }
        }
        if (spinSmear != null)
        {
            int dustStyle = Main.rand.NextBool() ? 66 : 263;
            Vector2 speed = spinRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (spinRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(150f, 180f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
            dust.noGravity = true;
            dust.color = dust.type == dustStyle ? Color.LightGreen : default;

            if (spinHandle != null)
            {
                spinHandle.Rotation = spinSmear.Rotation + (Pi / 8f);
                spinHandle.Time = 0;
                spinHandle.Position = player.Center;
                spinHandle.Scale = spinSmear.Scale / 1.8f;
                spinHandle.Color = Color.Gold;
                spinHandle.Color.A = (byte)(spinSmear.Color.A);
            }
        }
        spinCounter++;
    }

    private bool scytheSlice = false;
    Particle sliceSmear = null;
    Particle sliceHandle = null;
    float sliceDirection = 0;
    float sliceRotation = 0;
    int sliceCounter = 0;
    const int sliceDuration = 24;

    private void DoScytheSlice(Player player, bool up)
    {
        if (sliceCounter == 0)
        {
            SoundEngine.PlaySound(SoundID.Item71, player.Center);
            sliceDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction).ToRotation();
            sliceDirection += (-Pi + PiOver4);
        }
        sliceRotation = Lerp(sliceDirection, sliceDirection + (up ? -(PiOver2 + Pi) : PiOver2 + Pi), SineOutEasing(sliceCounter / (float)sliceDuration, 1));
        //sliceDirection = sliceDirection.RotatedBy(Clamp(1f - (sliceCounter / 60f), 0.5f, 1f) * PiOver4 * (up ? -0.35f : 0.35f));
        //sliceDirection.Normalize();

        //Update the variables of the smear                      
        if (sliceCounter > (sliceDuration / 2))
        {
            //if (sliceSmear != null)
            {
                if (sliceCounter < sliceDuration)
                {
                    /*
                    sliceSmear.Rotation = rotation + PiOver2;
                    sliceSmear.Time = 0;
                    sliceSmear.Position = player.Center;
                    sliceSmear.Scale = 1.5f;
                    sliceSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, sliceCounter / 20f);
                    sliceSmear.Color.A = (byte)(255 - 255 * Clamp(((sliceCounter - 8) / 8f), 0f, 1f));
                    */
                }
                else
                {
                    //if (sliceSmear != null)
                    //    GeneralParticleHandler.RemoveParticle(sliceSmear);
                    sliceSmear = null;


                    //if (sliceHandle != null)
                    //    GeneralParticleHandler.RemoveParticle(sliceHandle);
                    sliceHandle = null;

                    scytheSlice = false;
                    sliceCounter = 0;
                    return;
                }
            }
            if (sliceCounter >= 120)
                sliceCounter = -1;
        }
        else if (false)
        {
            if (sliceHandle == null)
            {
                sliceHandle = new SemiCircularSmearVFX(player.Center, Color.LimeGreen, sliceRotation, 1.5f, Vector2.One);
                GeneralParticleHandler.SpawnParticle(sliceHandle);
            }
            if (sliceSmear == null)
            {
                sliceSmear = new CircularSmearVFX(player.Center, Color.LimeGreen, sliceRotation, 1.5f);
                GeneralParticleHandler.SpawnParticle(sliceSmear);
                sliceSmear.Color.A = 0;
            }
            else
            {
                sliceSmear.Rotation = sliceRotation + MathHelper.PiOver2;
                sliceSmear.Time = 0;
                sliceSmear.Position = player.Center;
                sliceSmear.Scale = 1.5f;
                sliceSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, sliceCounter / 20f);
                sliceSmear.Color.A = (byte)(255 * MathHelper.Clamp((sliceCounter / 4f), 0f, 1f));
            }
        }
        if (sliceSmear != null)
        {
            int dustStyle = Main.rand.NextBool() ? 66 : 263;
            Vector2 speed = sliceRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (sliceRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(110f, 150f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
            dust.noGravity = true;
            dust.color = dust.type == dustStyle ? Color.LightGreen : default;

            if (sliceHandle != null)
            {
                sliceHandle.Rotation = sliceSmear.Rotation + (Pi / 8f);
                sliceHandle.Time = 0;
                sliceHandle.Position = player.Center;
                sliceHandle.Scale = sliceSmear.Scale / 1.8f;
                sliceHandle.Color = Color.Gold;
                sliceHandle.Color.A = sliceSmear.Color.A;
            }
        }
        sliceCounter++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (scytheSpin || scytheSlice)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float rotation = (scytheSlice ? sliceRotation : spinRotation);
            rotation += PiOver2;
            float scaleMult = 1f;
            if (scytheSlice)
            {
                if (sliceCounter < sliceDuration / 2)
                    scaleMult = Lerp(0.75f, 1f, sliceCounter / 8f);
                else
                    scaleMult = Lerp(1f, 0.75f, (sliceCounter - 8) / 8f);
            }
            else
            {
                if (spinCounter <= 5)
                    scaleMult = Lerp(0.75f, 1f, spinCounter / 5f);
                else if (spinCounter >= spinDuration - 10)
                    scaleMult = Lerp(1f, 0.5f, (spinCounter - (spinDuration - 10)) / 10f);
            }

            SpriteEffects effects = SpriteEffects.None;
            if (state == AIState.DownSlice || state == AIState.Spin)
                effects = SpriteEffects.FlipHorizontally;

            float fadeLerp = (scytheSlice ? (sliceCounter - (sliceDuration - 4)) / 4f : (spinCounter - (spinDuration - 4)) / 4f);
            if (fadeLerp < 0)
                fadeLerp = 0;
            fadeLerp = 1 - fadeLerp;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * fadeLerp, rotation, new(58, 66), 1.5f * scaleMult, effects);
        }

        return false;
    }
}

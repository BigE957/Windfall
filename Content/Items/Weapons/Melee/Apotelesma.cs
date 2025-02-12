using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.World;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Items.Weapons.Melee;
public class Apotelesma : ModItem, ILocalizedModType
{
    internal int ApotelesmaCharge = 0;
    internal int HitCounter = 0;
    internal enum AIState
    {
        UpSlice,
        DownSlice,
        Spin,
        Throw,
        Rush,
    }
    internal AIState State = AIState.UpSlice;

    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/ApotelesmaItem";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }

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
        Item.width = Item.height = 180;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        //Updates the swing state before anything else, to ensure the state can accurately be referred to in all cases
        if(Main.projectile.Any(p => p.active && p.type == Item.shoot && p.As<ApotelesmaProj>().State == (float)AIState.Throw))
            return false;

        if (player.altFunctionUse == 2)
        {
            if (ApotelesmaCharge == 5)
            {
                State = AIState.Rush;
                ApotelesmaCharge = 0;
            }
            else if(ApotelesmaCharge > 0)
            {
                State = AIState.Throw;
                for (int i = 0; i < ApotelesmaCharge; i++)
                {
                    GemData[i] = new(GemData[i].Item2, GemData[i].Item1, 0);
                }
                ApotelesmaCharge--;
                
            }
            else
                return false;
            return base.CanUseItem(player);
        }

        State = State switch
        {
            AIState.UpSlice => AIState.DownSlice,
            AIState.DownSlice => AIState.Spin,
            _ => AIState.UpSlice,
        };

        return base.CanUseItem(player);
    }

    public override float UseAnimationMultiplier(Player player) => UseTimeMultiplier(player);

    public override float UseTimeMultiplier(Player player)
    {
        return State switch
        {
            AIState.Spin => 2f,
            _ => 1f,
        };
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if(State == AIState.Rush)
        {
            SoundEngine.PlaySound(SoundID.Item71, player.Center);
            SoundEngine.PlaySound(TheOrator.Dash);

            Particle pulse = new DirectionalPulseRing(player.Center, velocity.SafeNormalize(Vector2.Zero) * -1.5f, new(117, 255, 159), new Vector2(0.5f, 2f), velocity.ToRotation(), 0f, 1f, 24);
            GeneralParticleHandler.SpawnParticle(pulse);
        }

        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, (float)State, velocity.ToRotation());
        return false;
    }

    internal Tuple<Vector2, Vector2, int>[] GemData = new Tuple<Vector2, Vector2, int>[5];

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (ApotelesmaCharge == 0 && State == AIState.UpSlice)
            return;

        Player myPlayer = Main.LocalPlayer;

        if (myPlayer.ActiveItem() != Item || !myPlayer.active || myPlayer.dead)
        {
            ApotelesmaCharge = 0;
            State = AIState.UpSlice;
            return;
        }

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
            goalPos.Y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + (offsetValue * 24)) * 4f;
            goalPos.X += (float)Math.Sin(Main.GlobalTimeWrappedHourly + (offsetValue * 24)) * 6f;
            goalPos -= myPlayer.velocity;

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
    internal ref float State => ref Projectile.ai[0];
    private ref float Time => ref Projectile.ai[2];

    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 270;
        Projectile.scale = (Apotelesma.AIState)State == Apotelesma.AIState.Throw ? 0.75f : 1f;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;

        Projectile.usesLocalNPCImmunity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.localNPCHitCooldown = ((Apotelesma.AIState)State == Apotelesma.AIState.Spin || (Apotelesma.AIState)State == Apotelesma.AIState.Throw) ? 12 : 24;

        if ((Apotelesma.AIState)State == Apotelesma.AIState.Throw)
        {
            SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
            Time = 31;
            Projectile.velocity = (Main.player[Projectile.owner].Center - Main.MouseWorld).SafeNormalize(Vector2.UnitX);
        }
    }

    public override void AI()
    {
        //Main.NewText(Projectile.whoAmI.ToString() + ": " + Time.ToString());
        Player owner = Main.player[Projectile.owner];

        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }

        owner.heldProj = Projectile.whoAmI;

        if (Time >= 0)
        {
            switch ((Apotelesma.AIState)State)
            {
                case Apotelesma.AIState.UpSlice:
                    if (Time == 0)
                    {
                        scytheSlice = true;
                    }
                    if (!scytheSlice)
                    {
                        Projectile.active = false;
                    }
                    break;
                case Apotelesma.AIState.DownSlice:
                    if (Time == 0)
                    {
                        scytheSlice = true;
                    }
                    if (!scytheSlice)
                    {
                        Projectile.active = false;
                    }
                    break;
                case Apotelesma.AIState.Spin:
                    if (Time == 0)
                    {
                        scytheSpin = true;
                    }
                    if (!scytheSpin)
                    {
                        Projectile.active = false;
                    }
                    break;
                case Apotelesma.AIState.Throw:
                    Projectile.rotation += 0.01f * (5 + Projectile.velocity.Length());
                    if (Projectile.timeLeft > 60)
                    {                        
                        NPC target = Main.npc[(int)Projectile.ai[1]];
                        if (Time <= 30)
                        {
                            Projectile.ai[1] = Projectile.Center.ClosestNPCAt(1100f, bossPriority: true).whoAmI;
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time, true);
                            float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                        }
                        else
                        {
                            if (Time == 31)
                            {
                                SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
                                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -40;
                            }
                            else
                            {
                                Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), CalamityWorld.death ? 0.0015f : 0.00125f * (Time - 30));
                                Projectile.velocity *= 0.95f;
                                if (Projectile.velocity.LengthSquared() < 25)
                                    Time = 0;
                            }
                        }
                    }
                    else
                    {
                        Projectile.velocity = Projectile.velocity.RotateTowards((owner.Center - Projectile.Center).ToRotation(), 0.09f).SafeNormalize(Vector2.Zero) * Clamp(Projectile.velocity.Length() * 1.05f, 0f, 30f);
                        if (Projectile.Hitbox.Intersects(owner.Hitbox))
                            Projectile.active = false;
                    }

                    if (Projectile.velocity.LengthSquared() > 64)
                    {
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(64, 64), DustID.Terra, Main.rand.NextVector2Circular(4, 4), Scale: Main.rand.NextFloat(0.7f, 1.4f));
                        dust.noGravity = true;
                    }

                    break;
                case Apotelesma.AIState.Rush:
                    Vector2 rushDirection = Projectile.ai[1].ToRotationVector2();

                    if (Time < 40)
                    {
                        owner.velocity = rushDirection * 72;
                        for (int i = 0; i < Time; i++)
                            owner.velocity *= 0.925f;
                    }

                    float rotation = 0.3f;

                    if (Time > 40)
                        rotation = Lerp(0.3f, 0, (Time - 40) / 16);

                    if (Time == 4 || Time == 12 || Time == 20)
                    {
                        Particle pulse = new DirectionalPulseRing(owner.Center, rushDirection * -1.5f, new(117, 255, 159), new Vector2(0.5f, 2f), rushDirection.ToRotation(), 0f, 0.75f, 48);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }

                    if(Time % 4 == 0 && Time < 32)
                    {
                        Color colorA = Main.rand.NextBool(3) ? Color.Gold : new(117, 255, 159);
                        Color colorB = Main.rand.NextBool(3) ? new(117, 255, 159) : Color.Gold;

                        Particle spark = new SparkParticle(Projectile.Center + rushDirection.RotatedBy(PiOver2) * 96, -(rushDirection.RotatedBy(-PiOver4 / 4)), false, 12, 1.5f, colorA);
                        GeneralParticleHandler.SpawnParticle(spark);

                        spark = new SparkParticle(Projectile.Center + rushDirection.RotatedBy(-PiOver2) * 96, -(rushDirection.RotatedBy(PiOver4 / 4)), false, 12, 1.5f, colorB);
                        GeneralParticleHandler.SpawnParticle(spark);

                        if(Time % 8 == 0)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), owner.Center, rushDirection.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<RushBolt>(), Projectile.damage, 0f, -1, 0, -20, Time % 16 == 0 ? 1 : 0);
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), owner.Center, rushDirection.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<RushBolt>(), Projectile.damage, 0f, -1, 0, -20, Time % 16 == 0 ? 0 : 1);
                        }
                    }

                    Projectile.rotation += rotation;

                    if (Time > 56)
                    {
                        //owner.velocity = Vector2.Zero;
                        Projectile.active = false;
                    }

                    break;
            }
        }
        if ((Apotelesma.AIState)State != Apotelesma.AIState.Throw)
            Projectile.Center = owner.RotatedRelativePoint(owner.MountedCenter, reverseRotation: true);

        if (scytheSpin)
            DoScytheSpin(owner);
        if (scytheSlice)
            DoScytheSlice(owner, (Apotelesma.AIState)State == Apotelesma.AIState.DownSlice);

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //if (!target.IsAnEnemy())
        //    return;

        if ((Apotelesma.AIState)State == Apotelesma.AIState.Throw || (Apotelesma.AIState)State == Apotelesma.AIState.Rush)
            return;

        Player owner = Main.player[Projectile.owner];
        if (owner.HeldItem.type != ModContent.ItemType<Apotelesma>())
            return;
        int counter = ((Apotelesma)owner.HeldItem.ModItem).HitCounter++;
        if (counter >= 6)
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
        if (!scytheSpin && !scytheSlice && (Apotelesma.AIState)State != Apotelesma.AIState.Throw)
            return false;

        if (projHitbox.Intersects(targetHitbox))
            return true;

        return base.Colliding(projHitbox, targetHitbox);
    }

    private bool scytheSpin = false;
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

        if (spinCounter >= spinDuration)
        {
            scytheSpin = false;
            spinCounter = 0;
            return;
        }

        if (Main.rand.NextBool())
        {
            Vector2 speed = spinRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (spinRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(80f, 100f)), DustID.Terra, speed * (State == (float)Apotelesma.AIState.DownSlice ? -3 : 3), Scale: Main.rand.NextFloat(1.25f, 2f));
            dust.noGravity = true;

            dust = Dust.NewDustPerfect(player.Center + (spinRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * -Main.rand.NextFloat(80f, 100f)), DustID.Terra, speed * -(State == (float)Apotelesma.AIState.DownSlice ? -3 : 3), Scale: Main.rand.NextFloat(1.25f, 2f));
            dust.noGravity = true;
        }

        spinCounter++;
    }

    private bool scytheSlice = false;
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
            sliceDirection -= PiOver2;
        }
        sliceRotation = Lerp(sliceDirection, sliceDirection + (up ? -(PiOver2 + Pi) : PiOver2 + Pi), SineOutEasing(sliceCounter / ((float)sliceDuration - 4), 1));
        //sliceDirection = sliceDirection.RotatedBy(Clamp(1f - (sliceCounter / 60f), 0.5f, 1f) * PiOver4 * (up ? -0.35f : 0.35f));
        //sliceDirection.Normalize();

        //Update the variables of the smear                      
        if (sliceCounter >= sliceDuration)
        {
            scytheSlice = false;
            sliceCounter = 0;
            return;
        }

        if (Main.rand.NextBool())
        {
            Vector2 speed = sliceRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (sliceRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(80f, 100f)), DustID.Terra, speed * (State == (float)Apotelesma.AIState.DownSlice ? -3 : 3), Scale: Main.rand.NextFloat(0.9f, 1.8f));
            dust.noGravity = true;

            dust = Dust.NewDustPerfect(player.Center + (sliceRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * -Main.rand.NextFloat(80f, 100f)), DustID.Terra, speed * -(State == (float)Apotelesma.AIState.DownSlice ? -3 : 3), Scale: Main.rand.NextFloat(0.9f, 1.8f));
            dust.noGravity = true;
        }

        sliceCounter++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (scytheSpin || scytheSlice)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float rotation = (scytheSlice ? sliceRotation : scytheSpin ? spinRotation : Projectile.rotation);
            rotation += PiOver2;
            float scaleMult = 1f;
            if (scytheSlice)
            {
                if (sliceCounter < sliceDuration / 2)
                    scaleMult = Lerp(0.75f, 1f, SineOutEasing(sliceCounter / 8f, 1));
                else if(sliceCounter >= sliceDuration - 10)
                    scaleMult = Lerp(1f, 0.5f, SineOutEasing((sliceCounter - (sliceDuration - 10)) / 10f, 1));
            }
            else if (scytheSpin)
            {
                if (spinCounter <= 8)
                    scaleMult = Lerp(0.5f, 1f, SineOutEasing(spinCounter / 8f, 1));
                else if (spinCounter >= spinDuration - 8)
                    scaleMult = Lerp(1f, 0.5f, SineOutEasing((spinCounter - (spinDuration - 8)) / 8f, 1));
            }

            SpriteEffects effects = SpriteEffects.None;
            if ((Apotelesma.AIState)State == Apotelesma.AIState.UpSlice || (Apotelesma.AIState)State == Apotelesma.AIState.Spin)
                effects = SpriteEffects.FlipHorizontally;

            float fadeLerp = 1f;
            if ((Apotelesma.AIState)State != Apotelesma.AIState.Throw)
            {
                fadeLerp = (scytheSlice ? (sliceCounter - (sliceDuration - 4)) / 4f : (spinCounter - (spinDuration - 4)) / 4f);
                if (fadeLerp < 0)
                    fadeLerp = 0;
                fadeLerp = 1 - fadeLerp;
            }

            Texture2D slash = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/ApotelesmaSlash").Value;
            Texture2D slashWhiteout = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/ApotelesmaSlashWhiteOut").Value;

            float slashFade = 1f;
            if (scytheSlice)
            {
                if (sliceCounter < sliceDuration / 2)
                    slashFade = Lerp(0f, 1f, SineOutEasing(sliceCounter / 18f, 1));
                else if (sliceCounter >= sliceDuration - 10)
                    slashFade = Lerp(1f, 0f, SineInEasing((sliceCounter - (sliceDuration - 10)) / 10f, 1));
            }
            else if (scytheSpin)
            {
                if (spinCounter <= 18)
                    slashFade = Lerp(0f, 1f, SineOutEasing(spinCounter / 18f, 1));
                else if (spinCounter >= spinDuration - 16)
                    slashFade = Lerp(1f, 0f, SineInEasing((spinCounter - (spinDuration - 16)) / 16f, 1));
            }

            Vector2 dir = Vector2.UnitX.RotatedBy(rotation + ((Apotelesma.AIState)State == Apotelesma.AIState.DownSlice ? -PiOver4 : PiOver4));

            //Main.EntitySpriteDraw(slashWhiteout, Projectile.Center - Main.screenPosition + dir * 108 * scaleMult, null, Color.SeaGreen * slashFade, rotation + (state == AIState.UpSlice ? (-PiOver4) : (Pi + PiOver4)), new(slash.Size().X * 0.5f, 72), 1.5f * scaleMult, state == AIState.UpSlice ? 0 : SpriteEffects.FlipHorizontally);
            //Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition + dir * 100 * scaleMult, null, Color.White * slashFade, rotation + (state == AIState.UpSlice ? (-PiOver4) : (Pi + PiOver4)), new(slash.Size().X * 0.5f, 72), 1.5f * scaleMult, state == AIState.UpSlice ? 0 : SpriteEffects.FlipHorizontally);

            //Main.EntitySpriteDraw(slashWhiteout, Projectile.Center - Main.screenPosition + dir * -108 * scaleMult, null, Color.SeaGreen * slashFade, rotation + (state == AIState.UpSlice ? (-PiOver4 + Pi) : (PiOver4)), new(slash.Size().X * 0.5f, 72), 1.5f * scaleMult, state == AIState.UpSlice ? 0 : SpriteEffects.FlipHorizontally);
            //Main.EntitySpriteDraw(slash, Projectile.Center - Main.screenPosition + dir * -100 * scaleMult, null, Color.White * slashFade, rotation + (state == AIState.UpSlice ? (-PiOver4 + Pi) : (PiOver4)), new(slash.Size().X * 0.5f, 72), 1.5f * scaleMult, state == AIState.UpSlice ? 0 : SpriteEffects.FlipHorizontally);
            
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * fadeLerp, rotation, tex.Size() * 0.5f, 1.5f * scaleMult, effects);
        }
        else if((Apotelesma.AIState)State == Apotelesma.AIState.Throw)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/ApotelesmaItem").Value;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, 1f, 0);
        }
        else if((Apotelesma.AIState)State == Apotelesma.AIState.Rush && Time < 64)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            float scale = 1.5f;
            float opacity = 1f;

            if (Time < 3)
            {
                opacity = Time / 3f;
                scale = Lerp(0.5f, 1.5f, Time / 3f);
            }
            if (Time > 54)
            {
                if(Time > 56)
                    opacity = 1 - (Time - 56) / 8f;
                scale = Lerp(1.5f, 0.75f, (Time - 54) / 10f);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, tex.Size() * 0.5f, scale, 0);
        }

        return false;
    }
}

public class RushBolt : ModProjectile
{
    public override string Texture => "Windfall/Assets/Projectiles/Boss/NailShot";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.damage = 100;
        Projectile.friendly = true;
        Projectile.penetrate = 20;
        Projectile.timeLeft = 360;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
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
    private enum myColor
    {
        Green,
        Orange
    }
    private myColor MyColor
    {
        get => Projectile.ai[2] == 0 ? myColor.Green : myColor.Orange;
        set => Projectile.ai[2] = (int)value;
    }
    Vector2 DirectionalVelocity = Vector2.Zero;
    public override void OnSpawn(IEntitySource source)
    {
        DirectionalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX);
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        NPC target = owner.Center.ClosestNPCAt(900f, bossPriority: true);

        Projectile.rotation = DirectionalVelocity.ToRotation();
        Projectile.velocity = DirectionalVelocity.SafeNormalize(Vector2.UnitX) * (Velocity / 2);

        if(target != null && Projectile.penetrate > 17)
            DirectionalVelocity = DirectionalVelocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), 0.01f * (1 + aiCounter / 10f));
        
        if(Velocity < 64)
            Velocity += 1f;

        if (Projectile.penetrate < 17)
        {
            Projectile.damage = 0;
            if ((owner.Center - Projectile.Center).Length() > 1200f)
            Projectile.active = false;
        }

        aiCounter++;

        if (Velocity > 5 && Main.rand.NextBool(4))
        {
            Vector2 position = new Vector2(Projectile.position.X, Projectile.Center.Y) + (Vector2.UnitY.RotatedBy(Projectile.rotation) * Main.rand.NextFloat(-16f, 16f));

            Particle spark = new SparkParticle(position, Projectile.velocity.RotatedByRandom(PiOver4) * -0.5f, false, 12, Main.rand.NextFloat(0.25f, 1f), ColorFunction(0));
            GeneralParticleHandler.SpawnParticle(spark);
        }
        Lighting.AddLight(Projectile.Center, ColorFunction(0).ToVector3());
    }
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Location = new(hitbox.Location.X + 39, hitbox.Center.Y);
        Vector2 rotation = Projectile.rotation.ToRotationVector2();
        rotation *= 39;
        hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
    }

    internal Color ColorFunction(float completionRatio)
    {
        Color colorA = MyColor == myColor.Green ? Color.LimeGreen : Color.Orange;
        Color colorB = MyColor == myColor.Green ? Color.GreenYellow : Color.Goldenrod;

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.1f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
    }

    internal float WidthFunction(float completionRatio)
    {
        float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
        float maxWidth = Projectile.Opacity * Projectile.width * 2f;

        return Lerp(0f, maxWidth, expansionCompletion);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Velocity > 2)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/" + (MyColor == myColor.Green ? "ScarletDevilStreak" : "FabstaffStreak")));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 30);
        }

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 origin = texture.Size() * 0.5f;

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Main.EntitySpriteDraw(texture, drawPos - Vector2.UnitX.RotatedBy(Projectile.rotation) * (MyColor == myColor.Green ? 48 : 28) + Vector2.UnitY * (MyColor == myColor.Green ? 2 : 0), texture.Frame(), ColorFunction(0) * 0.6f, Projectile.rotation, origin, new Vector2(MyColor == myColor.Green ? 3 : 2f, 1) * Projectile.scale * 0.33f, SpriteEffects.None, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        texture = (MyColor == myColor.Green ? TextureAssets.Projectile[Type] : ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/MagicShot")).Value;
        origin = texture.Size();
        origin.Y *= 0.5f;
        Main.EntitySpriteDraw(texture, drawPos, texture.Frame(), Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(DirectionalVelocity.X);
        writer.Write(DirectionalVelocity.Y);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        DirectionalVelocity = reader.ReadVector2();
    }
}

using CalamityMod.Items;
using Terraria.Enums;
using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Items.Weapons.Melee;
public class Apotelesma : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma";

    public override void SetDefaults()
    {
        Item.damage = 275;
        Item.knockBack = 7.5f;
        Item.useAnimation = Item.useTime = 20;
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

    public override bool CanShoot(Player player) => base.CanShoot(player);//!Main.projectile.Any(p => p.type == Item.shoot && p.owner == player.whoAmI);

    public override float UseAnimationMultiplier(Player player) => UseTimeMultiplier(player);

    public override float UseTimeMultiplier(Player player)
    {
        switch (ApotelesmaProj.state)
        {
            case ApotelesmaProj.AIState.DownSlice:
                return 1.25f;
            case ApotelesmaProj.AIState.UpSlice:
                return 2f;
            case ApotelesmaProj.AIState.Spin:
                return 1.25f;
            default:
                return 1f;
        }
    }
}

public class ApotelesmaProj : ModProjectile
{
    internal int ApotelesmaCharge = 0;

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
        Projectile.width = Projectile.height = 180;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.hide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI()
    {
        //Main.NewText(Projectile.whoAmI.ToString() + ": " + Time.ToString());

        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;
        
        if (player.dead)
        {
            Projectile.Kill();
            return;
        }

        if (Time >= 0)
        {
            if(Time == 0)
            {
                switch(state)
                {
                    case AIState.DownSlice:
                        state = AIState.UpSlice;
                        break;
                    case AIState.UpSlice:
                        state = AIState.Spin;
                        break;
                    case AIState.Spin:
                        state = AIState.DownSlice;
                        break;
                }
            }
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
                    if (!scytheSpin || Time > 24)
                    {
                        Projectile.active = false;
                    }
                    break;
            }
        }
        if(state != AIState.Throw)
        {
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
        }

        if (scytheSpin)
            DoScytheSpin(player);
        if (scytheSlice)
            DoScytheSlice(player, state == AIState.UpSlice);

        Time++;
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

        return base.Colliding(projHitbox, targetHitbox);
    }

    private bool scytheSpin = false;
    Particle spinSmear = null;
    Particle spinHandle = null;
    Vector2 spinDirection = Vector2.Zero;
    int spinCounter = 0;
    private void DoScytheSpin(Player player)
    {
        if (spinCounter == 0)
        {
            SoundEngine.PlaySound(SoundID.Item71, player.Center);
            spinDirection = player.velocity.SafeNormalize(Vector2.UnitX * player.direction) * -1;
            spinDirection.Normalize();
            spinDirection.RotatedBy(PiOver2 + (Pi / 4));
        }

        spinDirection = spinDirection.RotatedBy(MathHelper.Clamp(1f - (spinCounter / 60f), 0.5f, 1f) * MathHelper.PiOver4 * 0.50f);
        spinDirection.Normalize();

        if (spinCounter > 15)
        {
            if (spinSmear != null)
            {
                if (spinCounter < 30)
                {
                    spinSmear.Rotation = spinDirection.ToRotation() + MathHelper.PiOver2;
                    spinSmear.Time = 0;
                    spinSmear.Position = player.Center;
                    spinSmear.Scale = 1.9f;
                    spinSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, spinCounter / 40f);
                    spinSmear.Color.A = (byte)(255 - 255 * MathHelper.Clamp((spinCounter - 15) / 15f, 0f, 1f));
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
        else
        {
            if (spinHandle == null)
            {
                spinHandle = new SemiCircularSmearVFX(player.Center, Color.LimeGreen, spinDirection.ToRotation(), 1.5f, Vector2.One);
                GeneralParticleHandler.SpawnParticle(spinHandle);
            }
            if (spinSmear == null)
            {
                spinSmear = new CircularSmearVFX(player.Center, Color.LimeGreen, spinDirection.ToRotation(), 1.5f);
                GeneralParticleHandler.SpawnParticle(spinSmear);
                spinSmear.Color.A = 0;
            }
            else
            {
                spinSmear.Rotation = spinDirection.ToRotation() + MathHelper.PiOver2;
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
            Vector2 speed = spinDirection.RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (spinDirection.RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(150f, 180f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
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
    Vector2 sliceDirection = Vector2.Zero;
    int sliceCounter = 0;
    private void DoScytheSlice(Player player, bool up)
    {
        if (sliceCounter == 0)
        {
            SoundEngine.PlaySound(SoundID.Item71, player.Center);
            sliceDirection = player.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 1;
            sliceDirection.Normalize();
            sliceDirection = sliceDirection.RotatedBy(-Pi + PiOver4);
        }

        sliceDirection = sliceDirection.RotatedBy(Clamp(1f - (sliceCounter / 60f), 0.5f, 1f) * PiOver4 * (up ? -0.35f : 0.35f));
        sliceDirection.Normalize();

        //Update the variables of the smear                      
        if (sliceCounter > 8)
        {
            if (sliceSmear != null)
            {
                if (sliceCounter < 16)
                {
                    sliceSmear.Rotation = sliceDirection.ToRotation() + PiOver2;
                    sliceSmear.Time = 0;
                    sliceSmear.Position = player.Center;
                    sliceSmear.Scale = 1.5f;
                    sliceSmear.Color = Color.Lerp(Color.LimeGreen, Color.Teal, sliceCounter / 20f);
                    sliceSmear.Color.A = (byte)(255 - 255 * Clamp(((sliceCounter - 8) / 8f), 0f, 1f));
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
        else
        {
            if (sliceHandle == null)
            {
                sliceHandle = new SemiCircularSmearVFX(player.Center, Color.LimeGreen, spinDirection.ToRotation(), 1.5f, Vector2.One);
                GeneralParticleHandler.SpawnParticle(sliceHandle);
            }
            if (sliceSmear == null)
            {
                sliceSmear = new CircularSmearVFX(player.Center, Color.LimeGreen, sliceDirection.ToRotation(), 1.5f);
                GeneralParticleHandler.SpawnParticle(sliceSmear);
                sliceSmear.Color.A = 0;
            }
            else
            {
                sliceSmear.Rotation = sliceDirection.ToRotation() + MathHelper.PiOver2;
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
            Vector2 speed = sliceDirection.RotatedBy(PiOver2) * Main.rand.NextFloat(1f, 2f);
            Dust dust = Dust.NewDustPerfect(player.Center + (sliceDirection.RotatedBy(Main.rand.NextFloat(-Pi / 6, Pi / 6)) * Main.rand.NextFloat(110f, 150f)), Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
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
        if (scytheSpin ||scytheSlice)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float rotation = (scytheSlice ? sliceDirection : spinDirection).ToRotation();
            rotation += PiOver2;
            float scaleMult = 1f;
            if (scytheSlice)
            {
                if (sliceCounter < 8)
                    scaleMult = Lerp(0.75f, 1f, sliceCounter / 8f);
                else
                    scaleMult = Lerp(1f, 0.75f, (sliceCounter - 8) / 8f);
            }
            else
            {
                if (spinCounter <= 5)
                    scaleMult = Lerp(0.75f, 1f, spinCounter / 5f);
                else if (spinCounter >= 20)
                    scaleMult = Lerp(1f, 0.25f, (spinCounter - 20) / 10f);
            }

            SpriteEffects effects = SpriteEffects.None;
            if (state == AIState.DownSlice || state == AIState.Spin)
                effects = SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() * 0.5f, 2.5f * scaleMult, effects);
        }

        return false;
    }
}

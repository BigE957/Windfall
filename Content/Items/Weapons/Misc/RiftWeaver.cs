using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Luminance.Common.VerletIntergration;
using Luminance.Core.Graphics;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Items.Weapons.Misc;

public class RiftWeaver : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Misc";
    public override string Texture => "Windfall/Assets/Items/Weapons/Misc/RiftWeaver";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 250;
        Item.crit = 2;
        Item.knockBack = 0.2f;
        Item.useAnimation = Item.useTime = 18;
        Item.DamageType = DamageClass.Default;
        Item.noMelee = true;
        Item.autoReuse = true;
        Item.shootSpeed = 3.3f;
        Item.shoot = ModContent.ProjectileType<RiftWeaverStab>();

        Item.width = 54;
        Item.height = 54;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noUseGraphic = true;
        Item.UseSound = null;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2)
        {
            if (!Main.projectile.Any(p => p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<RiftWeaverThrow>()))
                Projectile.NewProjectile(source, player.Center - (Vector2.UnitY * player.height / 2.5f) - (Vector2.UnitX * player.width / 2f * player.direction), Vector2.Zero, ModContent.ProjectileType<RiftWeaverThrow>(), damage, knockback * 1.5f, ai2: 1);
            else
                return false;
        }
        else if (!AnyProjectiles(ModContent.ProjectileType<RiftWeaverThrow>()))
        {
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.6f }, player.Center);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai2: 1);
            return false;
        }
        else
        {
            Projectile weaver = Main.projectile.First(p => p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<RiftWeaverThrow>());
            if (weaver.ai[2] > 0)
                weaver.ai[2] = 190;
            else if (weaver.ai[1] < 45)
            {
                weaver.ai[1] = 45;
                if (weaver.velocity.LengthSquared() < 256)
                    weaver.velocity = (player.Center - weaver.Center).SafeNormalize(Vector2.Zero) * 16f;
            }
        }
        return false;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture+"Glow", AssetRequestMode.ImmediateLoad).Value;
        spriteBatch.Draw
        (
            texture,
            Item.Center - Main.screenPosition,
            new Rectangle(0, 0, texture.Width, texture.Height),
            Color.White,
            rotation,
            texture.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}

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
            CalamityMod.Particles.Particle pulse = new PulseRing(target.Center + (Owner.Center - target.Center) / 7f, Vector2.Zero, colorCombo ? Color.Cyan : Color.LimeGreen, 0f, 0.5f, 30);
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
            CalamityMod.Particles.Particle pulse = new PulseRing(target.Center + (Owner.Center - target.Center) / 7f, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan : Color.LimeGreen, 0f, 0.3f, 30);
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

public class RiftWeaverThrow : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Weapons/Misc/RiftWeaverProj";
    private readonly List<VerletSegment> NeedleString = [];
    private bool SpacialDamage
    {
        get => Projectile.ai[2] != 0;
        set => Projectile.ai[2] = value ? 1 : 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = 70;
        Projectile.height = 20;
        Projectile.DamageType = SpacialDamage ? DamageClass.Default : DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = int.MaxValue;
        Projectile.tileCollide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }
    public override void OnSpawn(IEntitySource source)
    {
        damage = Projectile.damage;
        Projectile.damage = 0;
        if (NeedleString.Count == 0)
        {
            for (int i = 0; i < 16; i++)
                NeedleString.Add(new((Projectile.Center - new Vector2(24, -32)) - (Vector2.UnitY * (-12 * i)), Vector2.Zero, i == 0));
        }
    }
    private int damage = 0;
    private int aiCounter
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }
    private int impaleCounter
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = (float)value;
    }
    private bool impaled = false;
    private NPC Target = null;
    private Vector2 positionDiff = Vector2.Zero;

    public enum AIState
    {
        Aiming,
        Throwing,
    }
    AIState state = AIState.Aiming;
    float charge = 0f;
    int chargeCounter = 0;
    bool fullChargeThrow = false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        //Main.NewText(impaleCounter);
        switch (state)
        {
            case AIState.Aiming:
                Projectile.Center = owner.Center - (Vector2.UnitY * owner.height / 2.5f) - (Vector2.UnitX * owner.width / 2f * owner.direction);
                Projectile.rotation = (owner.Calamity().mouseWorld - Projectile.Center).ToRotation();
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, (Pi - (Pi / 3f)) * owner.direction);
                owner.heldProj = Projectile.whoAmI;
                if (!Main.mouseRight)
                {
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, owner.Center);
                    if (charge >= 0.985f)
                        fullChargeThrow = true;
                    int count = 8;
                    for (int i = 1; i < count; i++)
                    {
                        CalamityMod.Particles.Particle particle = new GlowOrbParticle(Projectile.Center + (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * (620f * charge * (i / (float)count)), (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.01f + owner.velocity, false, 8 * i, 0.5f, i % 2 == 0 ? Color.Cyan : Color.LimeGreen);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }

                    Projectile.damage = (int)(damage * 1.5f * charge);
                    if (fullChargeThrow)
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * 120f;
                    else
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * (80f * charge);
                    state = AIState.Throwing;
                }
                else
                {
                    int count = 8;
                    for (int i = 1; i < count; i++)
                    {
                        CalamityMod.Particles.Particle particle = new GlowOrbParticle(Projectile.Center + (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * (620f * charge * (i / (float)count)), (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.01f + owner.velocity, false, 4, 0.5f, i % 2 == 0 ? Color.Cyan : Color.LimeGreen);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }

                    chargeCounter++;
                    charge = Clamp(ExpOutEasing(chargeCounter / 120f), 0f, 1f);
                }
                break;

            case AIState.Throwing:
                if (impaled)
                {
                    if (impaleCounter < 180)
                    {
                        if (Target != null && Target.active)
                        {
                            Projectile.Center = Target.Center + positionDiff;
                            Projectile.velocity = Target.velocity;
                        }
                        else if (Target != null)
                        {
                            impaled = false;
                            Target = null;
                            aiCounter = fullChargeThrow ? 30 : 45;
                            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                        }
                    }
                    else
                    {
                        if (impaleCounter != 180 && (Target == null || Target.active))
                        {
                            if (Target != null)
                            {
                                owner.velocity = ((Projectile.Center - owner.Center) * 0.055f).ClampLength(16f, 24f);
                                owner.velocity.X *= 0.8f;
                                owner.velocity.Y *= 1.2f;
                            }
                            else
                                owner.velocity = ((Projectile.Center - owner.Center) * 0.05f).ClampLength(20f, 20f);
                        }
                        impaled = false;
                        Target = null;
                        aiCounter = fullChargeThrow ? 30 : 45;
                        Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                    }
                    impaleCounter++;
                }
                else
                {
                    int timeOut = fullChargeThrow ? 25 : 40;
                    if (aiCounter < timeOut && Projectile.velocity.Length() > 5f && (fullChargeThrow || Main.rand.NextBool(3)))
                    {
                        for (int i = 0; i < (fullChargeThrow ? 2 : 1); i++)
                        {
                            Vector2 particleSpeed = Projectile.velocity.SafeNormalize(Vector2.Zero) * -1 * Clamp(Projectile.velocity.Length() / 6f, 4f, 10f);
                            CalamityMod.Particles.Particle speedline = new SparkParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), particleSpeed, false, 30, 1f, Main.rand.NextBool() ? Color.Cyan : Color.LimeGreen);
                            GeneralParticleHandler.SpawnParticle(speedline);
                        }
                    }
                    if (aiCounter < timeOut && (owner.Center - Projectile.Center).Length() < 600f)
                    {
                        if (fullChargeThrow)
                            Projectile.velocity *= 0.78f;
                        else
                            Projectile.velocity *= 0.85f;
                    }
                    else
                    {
                        Projectile.tileCollide = false;
                        if (Projectile.damage > 0)
                            Projectile.damage = 0;
                        if (aiCounter < timeOut)
                            aiCounter = timeOut;
                        if (Projectile.velocity.Length() < 64f)
                        {
                            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                            if (fullChargeThrow)
                                Projectile.velocity *= 1.15f;
                            else
                                Projectile.velocity *= 1.1f;
                        }
                        else
                            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 64f;
                        if ((bool)Colliding(Projectile.Hitbox, owner.Hitbox) || (Projectile.Center - owner.Center).LengthSquared() < 16f)
                            Projectile.active = false;
                    }
                    Projectile.rotation = (owner.Center - Projectile.Center).ToRotation() + Pi;
                    aiCounter++;
                }
                break;
        }
        VerletSimulations.RopeVerletSimulation(NeedleString, Projectile.Center - (Vector2.UnitX * 32f).RotatedBy(Projectile.rotation), 200f, new(), owner.Center);
    }
    public override bool? CanHitNPC(NPC target) => target.type == ModContent.NPCType<PortalMole>() || !SpacialDamage;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        bool colorCombo = Main.rand.NextBool();
        CalamityMod.Particles.Particle pulse = new PulseRing(target.Center + (Projectile.Center - target.Center) / 7f, Vector2.Zero, colorCombo ? Color.Cyan : Color.LimeGreen, 0f, 0.5f, 30);
        GeneralParticleHandler.SpawnParticle(pulse);
        pulse = new PulseRing(target.Center + (Projectile.Center - target.Center) / 7f, Vector2.Zero, colorCombo ? Color.LimeGreen : Color.Cyan, 0f, 0.3f, 30);
        GeneralParticleHandler.SpawnParticle(pulse);

        if (!impaled)
        {
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            if (Target == null)
            {
                impaled = true;
                Target = target;
                positionDiff = Projectile.Center - Target.Center;
                Projectile.velocity = Vector2.Zero;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        impaled = true;
        Projectile.velocity = Vector2.Zero;
        Projectile.tileCollide = false;
        return false;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CalamityUtils.RotatingHitboxCollision(Projectile, targetHitbox.TopLeft(), targetHitbox.Size()))
            return true;
        Projectile.Center -= Projectile.velocity / 2f;
        if (CalamityUtils.RotatingHitboxCollision(Projectile, targetHitbox.TopLeft(), targetHitbox.Size()))
            return true;
        Projectile.Center += Projectile.velocity / 2f;
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2[] segmentPositions = [.. NeedleString.Select(x => x.Position)];

        PrimitiveRenderer.RenderTrail(segmentPositions, new(widthFunction, colorFunction));

        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(tex, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + PiOver4, tex.Frame().Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }

    private readonly PrimitiveSettings.VertexWidthFunction widthFunction = WidthFunction;
    private static float WidthFunction(float interpolent) => 1.6f;

    private readonly PrimitiveSettings.VertexColorFunction colorFunction = ColorFunction;
    private static Color ColorFunction(float interpolent)
    {
        return Color.Lerp(Color.Cyan, Color.LimeGreen, interpolent * 2f) * (0.9f - interpolent);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }
}


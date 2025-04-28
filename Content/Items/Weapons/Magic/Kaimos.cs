using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using Terraria.Graphics.Shaders;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.GlobalNPCs;

namespace Windfall.Content.Items.Weapons.Magic;
public class Kaimos : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Magic";
    public override string Texture => "Windfall/Assets/Items/Weapons/Magic/Kaimos";

    public static int BaseDamage = 128;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 26;
        Item.damage = BaseDamage;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 60;
        Item.useTime = 60;
        Item.knockBack = 8f;
        Item.mana = 18;
        Item.UseSound = SoundID.DD2_MonkStaffSwing;
        Item.autoReuse = true;
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
        Item.shoot = ModContent.ProjectileType<KaimosHoldout>();
        Item.shootSpeed = 12f;
        Item.DamageType = DamageClass.Magic;
        Item.channel = true;
    }

    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        int lineCount = 0;
        float longestLength = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            string[] strings = lines[i].Text.Split('\n');
            lineCount += strings.Length;
            for (int j = 0; j < strings.Length; j++)
            {
                float measuredLength = FontAssets.MouseText.Value.MeasureString(strings[j]).X;
                if (measuredLength > longestLength)
                {
                    longestLength = measuredLength;
                }
            }
        }

        int xOffset = 15;
        int yOffset = 9;


        Rectangle box = new(x - xOffset, y - yOffset, (int)(longestLength) - yOffset * 7, lineCount * 28 + yOffset + (int)Math.Ceiling(yOffset / 2f));

        Utils.DrawInvBG(Main.spriteBatch, box, new Color(0, 150, 50) * 0.5f);
        for (int i = box.Y; i < box.Y + box.Height; i++)
        {
            float value = Utils.GetLerpValue(box.Y, box.Y + box.Height, i);
            Main.spriteBatch.DrawLineBetter(new Vector2(box.X, i) + Main.screenPosition, new Vector2(box.X + box.Width, i) + Main.screenPosition, Color.Lerp(new Color(0, 150, 50) * 0.5f, new Color(0, 64, 44) * 0.5f, value), 1f);
        }

        Main.spriteBatch.DrawLineBetter(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetter(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetter(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetter(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetter(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetter(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetter(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetter(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/JadeCrescent").Value;
        Rectangle frame = tex.Frame(verticalFrames: 2, frameY: 1);
        for (int i = 0; i < 4; i++)
        {
            float rotation = 0f;
            Vector2 position = Vector2.Zero;

            switch (i)
            {
                case 0:
                    position = box.TopLeft() + new Vector2(-2, -2);
                    rotation = PiOver2;
                    break;
                case 1:
                    position = box.TopRight() + new Vector2(2, -2);
                    rotation = Pi;
                    break;
                case 2:
                    position = box.BottomLeft() + new Vector2(-2, 2);
                    break;
                case 3:
                    position = box.BottomRight() + new Vector2(2, 2);
                    rotation = -PiOver2;
                    break;
            }

            Main.spriteBatch.Draw(tex, position, frame, Color.White, rotation, frame.Size() * 0.5f, 1f, 0, 0);
        }

        return true;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        TooltipLine line = tooltips.Last();
        if (!Main.keyState.IsKeyDown(Keys.LeftShift))
        {
            if (line != null)
                line.Text = GetWindfallTextValue("Items.Lore.OratorDrops.Unlocked");
            return;
        }
        tooltips.RemoveRange(1, tooltips.Count - 3);
        tooltips.RemoveAt(2);
        tooltips.Add(new(Windfall.Instance, "LoreTab", GetWindfallTextValue(LocalizationCategory + "." + Name + ".Lore")));
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, player.altFunctionUse);
        return false;
    }
}

public class KaimosHoldout : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "Windfall/Assets/Items/Weapons/Magic/Kaimos";

    public override void SetDefaults()
    {
        Projectile.width = 34;
        Projectile.height = 62;
        //Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.ignoreWater = true;
    }

    float Time = 0;
    float rotationOffset = 0f;
    float fireStrength = 0;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];

        bool canHold = !CalamityUtils.CantUseHoldout(owner, false);

        Vector2 toMouse = (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * owner.direction);

        owner.heldProj = Projectile.whoAmI;

        Projectile.velocity = Vector2.Zero;
        Projectile.Center = owner.MountedCenter - new Vector2(0, 26);
        if (canHold && (owner.channel || owner.Calamity().mouseRight))
        {
            if (Projectile.ai[0] == 2)
            {
                fireStrength = 24f;
                if(Time < 120f)
                    fireStrength = Lerp(1f, 24f, Time / 120f);
            }
            else
            {
                if (Time % (owner.manaSick ? 30 : 15) == 0 && owner.CheckMana(owner.ActiveItem(), 20))
                {
                    owner.CheckMana(owner.ActiveItem(), 20, true);
                    ++fireStrength;

                    Particle pulse = new DirectionalPulseRing(Projectile.Center + toMouse * 28 + owner.velocity, toMouse * 4f, (Time % (owner.manaSick ? 60 : 30)) == 0 ? Color.MediumSeaGreen : Color.Orange, new(0.5f, 1f), toMouse.ToRotation(), 0f, 0.5f, 16);
                    GeneralParticleHandler.SpawnParticle(pulse);
                    SoundEngine.PlaySound(SoundID.Item17 with { Pitch = (fireStrength >= 30f ? 1f : fireStrength / 30f), PitchVariance = 0 }, Projectile.Center);
                }
            }
        }
        else if (fireStrength > 0)
        {
            Vector2 spawnPos = Projectile.Center + toMouse * 36 + owner.velocity;

            if (Projectile.ai[0] == 2)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, toMouse * fireStrength, ModContent.ProjectileType<PotGlob>(), (int)(Projectile.damage * Lerp(1f, 2f, fireStrength / 24f)), Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(0.66f, 1f), 1, -1);
                fireStrength = 0;
            }
            else
            {
                if (Time % 8 == 0)
                {
                    for (int i = 0; i < 10; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(spawnPos, toMouse.RotatedBy(Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(0.1f, 3f), Main.rand.NextFloat(40, 80));
                    SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);

                    rotationOffset = Main.rand.NextFloat(-PiOver4 / 8f, PiOver4 / 8f);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, toMouse.RotatedBy(rotationOffset) * Main.rand.NextFloat(12f, 20f), ModContent.ProjectileType<PotGlob>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(0.33f, 0.66f), 2, -1);
                    fireStrength--;
                }
            }
        }
        else
        {
            Projectile.active = false;
            return;
        }

        Projectile.direction = Math.Sign(toMouse.X);
        Projectile.spriteDirection = Projectile.direction;
        Projectile.rotation = toMouse.ToRotation() - (rotationOffset * Projectile.direction);

        owner.ChangeDir(Projectile.direction);
        owner.itemTime = 2;
        owner.itemAnimation = 2;

        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Pi);
        owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Pi);

        Time++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation + PiOver2, new(16, 42), Projectile.scale, spriteEffects);
        return false;
    }
}

public class PotGlob : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    private ref float Size => ref Projectile.ai[0];

    public enum TrailType
    {
        None = -1,
        Implied,
        Shader,
        Particle
    }
    private TrailType Trail
    {
        get => (TrailType)Projectile.ai[1];
        set => Projectile.ai[1] = (int)value;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = Size;

        if (Trail == TrailType.Shader)
            Projectile.tileCollide = true;
    }

    public ref float TargetIndex => ref Projectile.ai[2];
    public float WhoAmIAttachedTo = -1;
    private float TargetInitialAngle = 0f;
    private float StabAngle = 0f;
    private Vector2 StabOffset = Vector2.Zero;

    public override void AI()
    {
        if (Trail == TrailType.Particle)
        {
            if (TargetIndex == -1)
                Projectile.velocity *= 0.966f;
            else
            {
                if (TargetIndex != -1)
                {
                    NPC target = Main.npc[(int)TargetIndex];

                    if (target != null && target.active)
                    {
                        Projectile.Center = target.Center + StabOffset.RotatedBy(target.rotation - TargetInitialAngle);
                        Projectile.rotation = StabAngle + (target.rotation - TargetInitialAngle);
                        Projectile.velocity = Vector2.Zero;
                    }
                    else
                        TargetIndex = -1;

                    target.Calamity().marked = 2;
                }
            }

            if (Projectile.velocity.LengthSquared() > 5f)
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Vector2.Zero, Projectile.scale * 48);
        }
        else
        {
            if(WhoAmIAttachedTo == -1)
                Projectile.velocity.Y += 0.5f;
            else
            {
                if (Size <= 1f)
                    Projectile.velocity *= 0.95f;
                else
                {
                    Vector2 goalPos;
                    NPC target = Projectile.Center.ClosestNPCAt(1100f, bossPriority: true);
                    if (target == null)
                        goalPos = Main.player[Projectile.owner].Calamity().mouseWorld;
                    else
                        goalPos = target.Center;

                    Projectile.rotation = Projectile.velocity.ToRotation();
                    Projectile.velocity += (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) / 10f * (Size - 1);
                }

                foreach(Projectile p in Main.projectile.Where(p => p.active && p.type == Type && p.owner == Projectile.owner && p.whoAmI != Projectile.whoAmI))
                {
                    PotGlob glob = p.As<PotGlob>();
                    if (glob.WhoAmIAttachedTo >= 0)
                        continue;
                    
                    if ((p.Center - Projectile.Center).Length() <= p.width * glob.Size + Projectile.width * Size)
                    {
                        if (glob.Trail == TrailType.Shader)
                        {
                            Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                            GeneralParticleHandler.SpawnParticle(pulse);
                            Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                            GeneralParticleHandler.SpawnParticle(explosion);

                            for (int i = 0; i < Size * 3 + glob.Size * 3; i++)
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(Size * 3f, Size * 3f), ModContent.ProjectileType<PotGlob>(), (int)Main.player[Projectile.owner].GetDamage(DamageClass.Magic).ApplyTo(Kaimos.BaseDamage), Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(0.33f, 0.66f), 2, -1);

                            Projectile.active = false;
                            p.active = false;
                            break;
                        }
                        else
                        {
                            Size += glob.Size / 3f;
                            p.active = false;
                            Projectile.timeLeft = 600;
                        }
                    }
                }

                if(Size >= 4f)
                {
                    Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                    GeneralParticleHandler.SpawnParticle(pulse);
                    Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                    GeneralParticleHandler.SpawnParticle(explosion);

                    for (int i = 0; i < Size * 3; i++)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(Size * 1.5f, Size * 1.5f), ModContent.ProjectileType<PotGlob>(), (int)Main.player[Projectile.owner].GetDamage(DamageClass.Magic).ApplyTo(Kaimos.BaseDamage), Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(0.33f, 0.66f), 2, -1);
                    
                    Projectile.active = false;
                    //target.StrikeNPC(target.CalculateHitInfo(Projectile.damage * damageMult, 0, true, Projectile.knockBack * 2f, Projectile.DamageType));
                }
            }
            if(WhoAmIAttachedTo == -2)
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - Projectile.rotation.ToRotationVector2() * Size * 10, Projectile.velocity * -0.5f, Projectile.scale * 48);
        }
        
        if (Projectile.timeLeft <= 30)
            Projectile.scale = Lerp(0f, Size, Projectile.timeLeft / 30f);
        if (Projectile.scale < Size)
            Projectile.scale += 0.05f;
        else
            Projectile.scale = Size;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CircularHitboxCollision(Projectile.Center, (projHitbox.Width / 2) * Projectile.scale, targetHitbox))
            return true;
        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Trail == TrailType.Particle)
        {
            TargetIndex = target.whoAmI;
            WhoAmIAttachedTo = WindfallGlobalNPC.GetWormHead((int)TargetIndex);
            TargetInitialAngle = target.rotation;
            StabAngle = Projectile.rotation;
            StabOffset = Projectile.Center - target.Center;
            Projectile.friendly = false;

            Projectile.timeLeft = 900;
        }
        else
        {
            int whoDidIHit = target.whoAmI;
            whoDidIHit = WindfallGlobalNPC.GetWormHead(whoDidIHit);

            Projectile[] stuckOrbs = Main.projectile.Where(p => p.active && p.type == Type && p.ai[1] == 2 && p.As<PotGlob>().WhoAmIAttachedTo == whoDidIHit && p.owner == Projectile.owner).ToArray();
            if (stuckOrbs.Length > 0)
            {
                int damageMult = stuckOrbs.Length;
                foreach (Projectile orb in stuckOrbs)
                    orb.active = false;

                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);

                Luminance.Core.Graphics.ScreenShakeSystem.StartShake(5f);
                
                for (int i = 0; i <= 50; i++)
                    EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
               
                Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                GeneralParticleHandler.SpawnParticle(pulse);
                Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                GeneralParticleHandler.SpawnParticle(explosion);

                target.StrikeNPC(target.CalculateHitInfo(Projectile.damage * damageMult, 0, true, Projectile.knockBack * 2f, Projectile.DamageType));

                Player owner = Main.player[Projectile.owner];

                owner.statMana += damageMult * 20;
                if (owner.statMana > owner.statManaMax2)
                    owner.statMana = owner.statManaMax2;

                if (WhoAmIAttachedTo != -2)
                    Projectile.active = false;
                else
                {
                    Size *= 0.8f;
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * Size * 3f;
                    if (Size < 1.25f)
                        Projectile.active = false;
                    else
                        Projectile.timeLeft = 600;
                }
            }
            else if(WhoAmIAttachedTo != -2)
            {
                Projectile.tileCollide = false;
                WhoAmIAttachedTo = -2;
                Projectile.velocity *= -1;
                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
                    Projectile.oldPos[i] = Projectile.position;
                Projectile.timeLeft = 600;
            }
        }
    }

    private Color ColorFunction(float completionRatio)
    {
        Color colorA = Color.Lerp(Color.LimeGreen, Color.Orange, EmpyreanMetaball.BorderLerpValue);
        Color colorB = Color.Lerp(Color.GreenYellow, Color.Goldenrod, EmpyreanMetaball.BorderLerpValue);

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
        float fadeOpacity = Utils.GetLerpValue(1f, 0f, completionRatio + 0.1f, true) * Projectile.Opacity;

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
    }

    private float WidthFunction(float completionRatio)
    {
        float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
        float maxWidth = Projectile.Opacity * Projectile.scale * 180f;

        return Lerp(0f, maxWidth, expansionCompletion);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Trail == TrailType.Shader && Projectile.velocity.LengthSquared() >= 9 && WhoAmIAttachedTo != -2)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 20);
        }

        Main.spriteBatch.UseBlendState(BlendState.Additive);
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(tex, drawPos, tex.Frame(), ColorFunction(0) * 0.75f, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 0.8f, SpriteEffects.None, 0);
        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 origin = tex.Size() * 0.5f;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
    }
}
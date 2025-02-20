using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using Microsoft.Xna.Framework.Input;
using SteelSeries.GameSense.DeviceZone;
using System.Collections.ObjectModel;
using Terraria.Graphics.Shaders;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Items.Weapons.Magic;
public class Kaimos : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Magic";
    public override string Texture => "Windfall/Assets/Items/Weapons/Magic/Kaimos";

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 26;
        Item.damage = 128;
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

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
}

public class KaimosHoldout : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "Windfall/Assets/Items/Weapons/Magic/Kaimos";
    public static Asset<Texture2D> Metaballs;

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
            Metaballs = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Magic/KaimosMetaballs");
    }

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
    bool fireSuccess = false;
    int projectilesToBeFired = 0;

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];

        bool canHold = !CalamityUtils.CantUseHoldout(owner);

        Vector2 toMouse = (owner.Calamity().mouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX * owner.direction);

        owner.heldProj = Projectile.whoAmI;

        Projectile.velocity = Vector2.Zero;
        Projectile.Center = owner.MountedCenter - new Vector2(0, 26);

        if (canHold)
        {
            if (owner.CheckMana(owner.ActiveItem(), 20) && Time % 15 == 0)
            {
                owner.CheckMana(owner.ActiveItem(), 20, true);
                Main.NewText(++projectilesToBeFired);
            }
        }
        else if (projectilesToBeFired > 0)
        {
            if (Time % 10 == 0)
            {
                Vector2 spawnPos = Projectile.Center + toMouse * 32 + owner.velocity;

                for (int i = 0; i < 10; i++)
                    EmpyreanMetaball.SpawnDefaultParticle(spawnPos, toMouse.RotatedBy(Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(0.1f, 3f), Main.rand.NextFloat(40, 80));

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, toMouse.RotatedBy(Main.rand.NextFloat(-PiOver4 / 4f, PiOver4 / 4f)) * Main.rand.NextFloat(12f, 20f), ModContent.ProjectileType<PotGlob>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Main.rand.NextFloat(0.5f, 1f), 2);

                projectilesToBeFired--;
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

    public override void PostDraw(Color lightColor)
    {

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
        Projectile.penetrate = 3;
        Projectile.timeLeft = 390;
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
        Projectile.width = (int)(Projectile.width * Size);
        Projectile.height = (int)(Projectile.height * Size);
    }

    public override void AI()
    {
        Projectile.velocity *= 0.966f;

        if (Trail == TrailType.Particle && Projectile.velocity.LengthSquared() > 5f)
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Vector2.Zero, Projectile.scale * 48);
        /*
        foreach(Projectile p in Main.projectile.Where(p => p.active && p.type == Type && p.owner == Projectile.owner && p.whoAmI != Projectile.whoAmI))
        {
            if ((p.Center - Projectile.Center).Length() < (12 * Projectile.scale) + (12 * p.scale))
            {
                if (p.ai[0] == Size)
                {
                    if (p.velocity.LengthSquared() > Projectile.velocity.LengthSquared())
                    {
                        p.ai[0] += Size / 2f;
                        Projectile.active = false;
                        p.timeLeft = 390;
                    }
                    else
                    {
                        Size += p.ai[0] / 2f;
                        p.active = false;
                        Projectile.timeLeft = 390;
                    }
                }
                else
                {
                    if (p.ai[0] > Size)
                    {
                        p.ai[0] += Size / 2f;
                        Projectile.active = false;
                        p.timeLeft = 390;
                    }
                    else
                    {
                        Size += p.ai[0] / 2f;
                        p.active = false;
                        Projectile.timeLeft = 390;
                    }
                }
            }
        }
        */
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
        if (Trail == TrailType.Shader)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 20);
        }

        Main.spriteBatch.UseBlendState(BlendState.Additive);
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 drawPos = Projectile.Center - Main.screenPosition - (Projectile.ai[0] == 0 ? Vector2.Zero : Projectile.velocity.SafeNormalize(Vector2.UnitX) * (16 * Projectile.scale));
        Main.EntitySpriteDraw(tex, drawPos, tex.Frame(), ColorFunction(0) * 0.75f, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale * 0.8f, SpriteEffects.None, 0);
        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 origin = tex.Size() * 0.5f;
        Vector2 drawPos = Projectile.Center - Main.screenPosition - (Projectile.ai[0] == 0 ? Vector2.Zero : Projectile.velocity.SafeNormalize(Vector2.UnitX) * (16 * Projectile.scale));
        Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None);
    }
}
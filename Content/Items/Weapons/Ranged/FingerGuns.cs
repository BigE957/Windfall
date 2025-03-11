using CalamityMod.Items;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;

namespace Windfall.Content.Items.Weapons.Ranged;
public class FingerGuns : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";
    public override string Texture => "Windfall/Assets/Items/Weapons/Ranged/FingerGuns/FingerGunsItem";

    public int HitCounter = 0;

    public override void SetDefaults()
    {
        Item.damage = 275;
        Item.knockBack = 7.5f;
        Item.useAnimation = Item.useTime = 12;
        Item.DamageType = DamageClass.Ranged;
        Item.noMelee = true;
        Item.channel = true;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<FingerBolt>();
        Item.shootSpeed = 48f;
        Item.width = 48;
        Item.height = 34;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
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

        Rectangle box = new(x - xOffset, y - yOffset, (int)(longestLength) - yOffset * 6, lineCount * 28 + yOffset + (int)Math.Ceiling(yOffset / 2f));

        Utils.DrawInvBG(Main.spriteBatch, box, new Color(0, 150, 50) * 0.5f);
        for (int i = box.Y; i < box.Y + box.Height; i++)
        {
            float value = Utils.GetLerpValue(box.Y, box.Y + box.Height, i);
            Main.spriteBatch.DrawLineBetter(new Vector2(box.X, i) + Main.screenPosition, new Vector2(box.X + box.Width, i) + Main.screenPosition, Color.Lerp(new Color(0, 150, 50) * 0.5f, new Color(0, 64, 44), value), 1f);
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

    public override void UpdateInventory(Player player)
    {
        if (player.ActiveItem() != Item)
            HitCounter = 0;
    }
}

public class FingerBolt : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Ranged";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    private ref float State => ref Projectile.ai[0];

    private ref float direction => ref Projectile.ai[1];

    private ref float Time => ref Projectile.ai[2];


    private enum myColor
    {
        Green,
        Orange
    }
    private myColor MyColor = myColor.Green;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        //Projectile.damage = 100;
        Projectile.friendly = true;
        Projectile.penetrate = 20;
        Projectile.timeLeft = 200;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (State == 0)
            Projectile.timeLeft += 40;

        direction = Projectile.velocity.SafeNormalize(Vector2.Zero).ToRotation();

        float RotationOffset = Pi * 0.025f;
        direction = direction + Main.rand.NextFloat(-RotationOffset, RotationOffset);

        MyColor = Projectile.whoAmI % 2 == 0 ? myColor.Orange : myColor.Green;
        CalamityMod.Particles.Particle pulse = new DirectionalPulseRing(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 20, Projectile.velocity.SafeNormalize(Vector2.Zero) * 4f, MyColor == myColor.Green ? Color.MediumSeaGreen : Color.Orange, new(0.5f, 1f), Projectile.velocity.ToRotation(), 0f, 0.5f, 16);
        GeneralParticleHandler.SpawnParticle(pulse);

        Time = 0;

        Projectile.netUpdate = true;
    }
    int localHitCount = 0;

    public override void AI()
    {
        float aberrationPower = (float)(Math.Pow(AperiodicSin(Main.GlobalTimeWrappedHourly * 32f), 2f) * 0.4f);
        ManagedScreenFilter aberrationShader = ShaderManager.GetFilter("Windfall.ChromaticAberrationShader");
        aberrationShader.TrySetParameter("splitIntensity", aberrationPower);
        aberrationShader.TrySetParameter("impactPoint", Vector2.One * 0.5f);
        aberrationShader.Activate();

        if (State == 0)
        {
            if (Time < 60)
            {
                float lerpVal = Clamp(Time / 90f, 0f, 1f);
                Projectile.velocity = direction.ToRotationVector2() * Lerp(24f, 1f, ExpOutEasing(lerpVal, 1));
            }
            else if (localHitCount < 3 && Projectile.timeLeft > 60)
            {
                if (Time == 60)
                    Projectile.damage /= 2;

                Vector2 goalPos;
                NPC target = Projectile.Center.ClosestNPCAt(1100f, bossPriority: true);
                if (target == null)
                    goalPos = Main.player[Projectile.owner].Calamity().mouseWorld;
                else
                    goalPos = target.Center;

                float lerpVal = Clamp((Time - 60) / 45f, 0f, 1f);
                Projectile.velocity = Projectile.velocity.SafeNormalize(direction.ToRotationVector2()).RotateTowards((goalPos - Projectile.Center).ToRotation(), Lerp(0f, 0.25f, lerpVal)) * Lerp(1f, 24f, SineInEasing(lerpVal, 1));
            }
        }
        else
        {
            if (localHitCount < 2 && Projectile.timeLeft > 60)
            {
                Vector2 goalPos;
                NPC target = Projectile.Center.ClosestNPCAt(1100f, bossPriority: true);
                if (target == null)
                    goalPos = Main.player[Projectile.owner].Calamity().mouseWorld;
                else
                    goalPos = target.Center;

                float lerpVal = Clamp(Time / 60f, 0f, 1f);
                Projectile.velocity = Projectile.velocity.SafeNormalize(direction.ToRotationVector2()).RotateTowards((goalPos - Projectile.Center).ToRotation(), Lerp(0f, 0.25f, lerpVal)) * Lerp(12f, 24f, SineOutEasing(lerpVal, 1));
            }
        }
        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (++localHitCount == (State == 0 ? 3 : 2))
            Projectile.damage = 0;

        //if (!target.IsAnEnemy())
        //    return;

        if (State == 1)
            return;

        Player owner = Main.player[Projectile.owner];

        if (owner.ActiveItem().type != ModContent.ItemType<FingerGuns>())
            return;

        int gunType = ModContent.ProjectileType<FingerlingGun>();

        if (Main.projectile.Where(p => p.active && p.type == gunType && p.owner == owner.whoAmI).Count() >= 7)
            return;

        FingerGuns item = owner.ActiveItem().ModItem as FingerGuns;
        if(++item.HitCounter >= 16)
        {
            Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(48 + target.width, 48 + target.height);
            Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center + spawnOffset, spawnOffset / 3f, gunType, owner.ActiveItem().damage, 0f, Projectile.owner);

            item.HitCounter = 0;
        }
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
        ManagedShader shader = ShaderManager.GetShader("Windfall.GenericFlameTrail");
        shader.SetTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/" + (MyColor == myColor.Green ? "ScarletDevilStreak" : "FabstaffStreak")), 1, SamplerState.LinearWrap);
        PrimitiveSettings settings = new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, Shader: shader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 30);
        /*
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
        Vector2 origin = texture.Size() * 0.5f;

        
        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Main.EntitySpriteDraw(texture, drawPos - Vector2.UnitX.RotatedBy(Projectile.rotation) * (MyColor == myColor.Green ? 48 : 28) + Vector2.UnitY * (MyColor == myColor.Green ? 2 : 0), texture.Frame(), ColorFunction(0) * 0.6f, Projectile.rotation, origin, new Vector2(MyColor == myColor.Green ? 3 : 2f, 1) * Projectile.scale * 0.33f, SpriteEffects.None, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);
        */
        return false;
    }
}

public class FingerlingGun : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Ranged";
    public override string Texture => "Windfall/Assets/Items/Weapons/Ranged/FingerGuns/FingerGunsHands";

    float rotationOffset = 0f;

    internal enum AIState
    {
        Idle,
        Attacking
    }
    internal AIState State
    {
        get => (AIState)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    private ref float Time => ref Projectile.ai[2];

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.damage = 0;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.tileCollide = false;

        Projectile.localAI[0] = Main.rand.NextBool().ToInt();
    }    

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];

        Vector2 goalPos = owner.Center + new Vector2(-48 * owner.direction, -48);

        float speed = (goalPos - Projectile.Center).Length() / 12f;

        Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;

        foreach (Projectile hand in Main.projectile.Where(p => p.active && p.type == Type && p.owner == owner.whoAmI && p.whoAmI != Projectile.whoAmI))
        {
            Vector2 toHand = (Projectile.Center - hand.Center);
            float force = Lerp(8f, 0f, Clamp(toHand.Length() / 56f, 0f, 1f));

            Projectile.velocity += toHand.SafeNormalize(Vector2.UnitX * -owner.direction) * force;
        }

        Vector2 toMouse = (owner.Calamity().mouseWorld - Projectile.Center);

        switch (State)
        {
            case AIState.Idle:
                if (rotationOffset != 0)
                {
                    rotationOffset = Lerp(PiOver2, 0f, SineOutEasing(Time / 24f, 1));
                    if(rotationOffset == 0)
                        Time = 0;
                    else
                        Time++;
                }
                else
                {
                    if (Time != 0 && Time % 30 == 0 && Main.rand.NextBool())
                    {
                        State = AIState.Attacking;
                        Time = 0;
                        Projectile.netUpdate = true;
                    }
                    else
                        Time++;
                }
                break;
            case AIState.Attacking:
                if (rotationOffset != PiOver2)
                {
                    if (Time == 0)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + toMouse.SafeNormalize(Projectile.rotation.ToRotationVector2()) * 6f, toMouse * 16f + Projectile.velocity, ModContent.ProjectileType<FingerBolt>(), Projectile.damage / 2, 0, Projectile.owner, 1);
                    if(Time >= 1)
                        rotationOffset = Lerp(0, PiOver2, SineOutEasing((Time - 1) / 8f, 1));
                    Time++;
                }
                else
                {
                    Time = 0;
                    State = AIState.Idle;
                }
                break;
        }

        Projectile.spriteDirection = toMouse.X.DirectionalSign();

        Projectile.rotation = toMouse.ToRotation() - (rotationOffset * Projectile.spriteDirection);
        
        Projectile.timeLeft = 120;

        if (owner.ActiveItem().type != ModContent.ItemType<FingerGuns>())
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(2, 1, (int)Projectile.localAI[0]);

        SpriteEffects effects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            effects = SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, effects);
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Ranged/FingerGuns/FingerGunsMetaball").Value;

        SpriteEffects effects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            effects = SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, effects);
    }
}

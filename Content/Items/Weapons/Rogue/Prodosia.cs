using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Items.Weapons.Rogue;
public class Prodosia : RogueWeapon, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Rogue";
    public override string Texture => "Windfall/Assets/Items/Weapons/Rogue/ExodiumSpear";

    public override void SetDefaults()
    {
        Item.damage = 250;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 1f;
        Item.UseSound = null;
        Item.autoReuse = true;
        Item.maxStack = 1;
        Item.shoot = ModContent.ProjectileType<GoldenTrinket>();
        Item.shootSpeed = 6f;
        Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
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

    public override float StealthVelocityMultiplier => 2f;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable())
        {
            player.Calamity().ConsumeStealthByAttacking();

            int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GoldenJavelin>(), damage, knockback, player.whoAmI);
            if (p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }
        player.Calamity().ConsumeStealthByAttacking();

        return true;
    }
}

public class GoldenTrinket : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/HandRings";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 12;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = 1;
        Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float AfterImageOpacity => ref Projectile.ai[1];

    private enum States
    {
        Held,
        Thrown,
        Bounce,
        Seek,
        Die
    }
    private States State = States.Held;

    int SeekingHitCounter = 0;
    float throwStrength = 0f;

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.velocity = Vector2.Zero;
        Projectile.localAI[0] = Main.rand.Next(3);
        Projectile.localAI[1] = Main.rand.Next(4);
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];

        switch(State)
        {
            case States.Held:
                owner.heldProj = Projectile.whoAmI;
                owner.reuseDelay = 4;
                
                float angle = (PiOver2 + PiOver4) * -owner.direction;
                throwStrength = 1f;
                if (Time < 30f)
                {
                    throwStrength = CircOutEasing(Time / 30f, 1);
                    angle = 0f.AngleLerp((PiOver2 + PiOver4) * -owner.direction, throwStrength);
                }

                Projectile.Center = owner.MountedCenter + (Vector2.UnitX * owner.direction).RotatedBy(angle) * 24;

                angle -= PiOver2 * owner.direction;
                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, angle);

                Vector2 currentCenter = Projectile.Center;
                Vector2 plannedVelocity = (owner.Calamity().mouseWorld - owner.Center).SafeNormalize(Vector2.UnitX * owner.direction) * 10 * throwStrength;
                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        currentCenter += plannedVelocity * 2f;
                        plannedVelocity.Y += 0.4f;
                    }

                    Particle particle = new GlowOrbParticle(currentCenter, Vector2.Zero, false, 2, 0.5f, (i % 2 == 0 ? Color.Cyan : Color.LimeGreen) with {A = (byte)(255 * throwStrength) });
                    GeneralParticleHandler.SpawnParticle(particle);
                }

                if (!owner.channel)
                {
                    State = States.Thrown;
                    Time = 0;
                    return;
                }
                break;
            case States.Thrown:
                if (Time == 0)
                {
                    Projectile.tileCollide = true;
                    SoundEngine.PlaySound(SoundID.Item1, owner.Center);
                    Projectile.velocity = (owner.Calamity().mouseWorld - owner.Center).SafeNormalize(Vector2.UnitX * owner.direction) * 10 * throwStrength;
                }

                bool colliding = false;
                foreach (Projectile spear in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<GoldenJavelin>() && p.owner == Projectile.owner && (int)p.As<GoldenJavelin>().State == 1))
                {
                    float collisionPoint = 0;
                    colliding = Collision.CheckAABBvLineCollision(new(Projectile.Hitbox.X, Projectile.Hitbox.Y), Projectile.Hitbox.Size(), spear.Center + (spear.rotation.ToRotationVector2() * spear.width / 2f), spear.Center - (spear.rotation.ToRotationVector2() * spear.width / 2f), spear.height, ref collisionPoint);

                    if(colliding)
                    {
                        float sign = Math.Sign(Projectile.velocity.X);

                        spear.As<GoldenJavelin>().State++;
                        State = States.Die;

                        Projectile.damage = 0;
                        AfterImageOpacity = 0f;
                        Projectile.velocity = Projectile.velocity.RotatedBy(Pi + (Main.rand.NextFloat(PiOver4 / 2f, PiOver4 + PiOver4 / 2f) * sign));
                        Projectile.timeLeft = 120;

                        Projectile.netUpdate = true;
                        return;
                    }
                }

                Projectile.velocity.Y += 0.2f;
                break;
            case States.Bounce:
                Projectile.tileCollide = false;
                Projectile.velocity *= 0.975f;
                Projectile.velocity.Y += 0.2f * (1 - AfterImageOpacity);
                int seekDelay = 24;
                if(Time > seekDelay)
                {
                    if(AfterImageOpacity < 1f)
                        AfterImageOpacity = Clamp((Time - seekDelay) / 12f, 0f, 1f);
                    if(Time > (SeekingHitCounter == 0 ? seekDelay * 3f : seekDelay + 12))
                    {
                        State = States.Seek;
                        Time = 0;
                        return;
                    }
                }
                break;
            case States.Seek:
                Vector2 goalPos;
                NPC target = Projectile.Center.ClosestNPCAt(1100f, bossPriority: true);
                if (target == null)
                    goalPos = Main.player[Projectile.owner].Calamity().mouseWorld;
                else
                    goalPos = target.Center;

                Projectile.velocity = Projectile.velocity.RotateTowards((goalPos - Projectile.Center).ToRotation(), 0.15f);

                float seekVelocity = 10f;
                float seekTime = 10;

                if(Time < seekTime)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * Clamp(Lerp(0.01f, seekVelocity, CircOutEasing(Time / seekTime, 1)), 0.01f, seekVelocity);
                else
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * seekVelocity;

                AfterImageOpacity = 1f;
                break;
            case States.Die:                
                Projectile.tileCollide = true;
                Projectile.velocity.Y += 0.25f;
                break;
        }

        Time++;

        if (State != States.Held)
            Projectile.rotation += Projectile.velocity.X * 0.01f;

        if(State == States.Held || State == States.Bounce)
            Projectile.timeLeft = 150;
        if (State == States.Seek && Projectile.timeLeft <= 30)
        {
            Projectile.timeLeft = 120;
            AfterImageOpacity = 0f;
            State = States.Die;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        float sign = Math.Sign(Projectile.velocity.X);
        if (State == States.Thrown)
        {
            Projectile.velocity = Projectile.velocity.RotatedBy(Pi);
            State = States.Bounce;
            Time = 0;
        }

        if (State == States.Seek)
        {
            if (++SeekingHitCounter >= 2)
            {
                Projectile.damage = 0;
                AfterImageOpacity = 0f;
                Projectile.velocity = Projectile.velocity.RotatedBy(Pi);
                Projectile.timeLeft = 120;
                State = States.Die; 
            }
            else
            {
                //AfterImageOpacity = 0f;
                Projectile.velocity = Projectile.velocity.RotatedBy(Pi) * 0.75f;
                State = States.Bounce;
            }

            Time = 0;
        }

        if (State == States.Bounce || State == States.Die)
        {
            Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(PiOver4 / 2f, PiOver4 + PiOver4 / 2f) * sign);
            Projectile.netUpdate = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (int i = 0; i <= 10; i++)
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + Projectile.velocity, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 20f));
        
        return true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.localAI[1] == 0 ? 0 : 1)).Value;
        Color color = Color.Black;
        switch (Projectile.localAI[0])
        {
            case 0:
                color = new(255, 133, 187);
                break;
            case 1:
                color = new(253, 189, 53);
                break;
            case 2:
                color = new(220, 216, 155);
                break;
        }

        //DrawAfterimagesCentered
        if(AfterImageOpacity > 0)
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float rotation2 = Projectile.oldRot[i];
                SpriteEffects effects2 = ((Projectile.oldSpriteDirection[i] == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                Vector2 position = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Color afterImageColor = Projectile.GetAlpha(color) * ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                afterImageColor *= AfterImageOpacity;
                Main.spriteBatch.Draw(WhiteOutTexture, position, null, afterImageColor, rotation2, WhiteOutTexture.Size() * 0.5f, Projectile.scale, effects2, 0f);
            }

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, null, color * Projectile.Opacity, Projectile.rotation, WhiteOutTexture.Size() * 0.5f, Projectile.scale * 1.25f * CircOutEasing(AfterImageOpacity, 1), SpriteEffects.None);

        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = tex.Frame(3, 4, (int)Projectile.ai[1], (int)Projectile.localAI[1]);

        Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }
}

public class GoldenJavelin : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/OratorJavelin";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 16;
        Projectile.timeLeft = 900;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.damage = 1000;
        Projectile.extraUpdates = 2;
        Projectile.hide = true;
        Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
    }

    private int TargetIndex = -1;
    private float TargetInitialAngle = 0f;
    private float StabAngle = 0f;
    private Vector2 StabOffset = Vector2.Zero;

    public enum States
    {
        Thrown,
        Impaled,
        Dislodged,
        Slicing,
        Seeking
    }
    public States State = States.Thrown;

    public ref float Time => ref Projectile.ai[2];

    public ref float AfterImageOpacity => ref Projectile.localAI[1];

    public override void AI()
    {
        switch (State)
        {
            case States.Thrown:
                Projectile.rotation = Projectile.velocity.ToRotation();
                break;
            case States.Impaled:
                NPC target = null;
                Time = 0;
                if (TargetIndex != -1)
                {
                    target = Main.npc[TargetIndex];
                    if (target != null && target.active)
                    {
                        Projectile.Center = target.Center + StabOffset.RotatedBy(target.rotation - TargetInitialAngle);
                        Projectile.rotation = StabAngle + (target.rotation - TargetInitialAngle);
                        Projectile.velocity = Vector2.Zero;
                    }
                    else
                        TargetIndex = -1;
                }
                break;
            case States.Dislodged:
                if (Time == 0)
                {
                    Vector2 hitDirection = Projectile.rotation.ToRotationVector2();
                    Projectile.velocity = hitDirection * 12f;

                    target = Main.npc[TargetIndex];

                    target.StrikeNPC(target.CalculateHitInfo(Projectile.damage * 2, Math.Sign(hitDirection.X), true, Projectile.knockBack * 2f, Projectile.DamageType));

                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 velocity = hitDirection.RotatedBy(Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(8f, 12f);
                        velocity.Y -= 4;

                        Particle spark = new SparkParticle(target.Center, velocity, true, 18, Main.rand.NextFloat(0.5f, 1f), new(117, 255, 159));
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }
                if (Time >= 16)
                {
                    Projectile.velocity *= 0.925f;
                }
                if (Projectile.velocity.Length() < 0.1f)
                {
                    StabAngle = Projectile.rotation;
                    Projectile.velocity = Vector2.Zero;
                    State = States.Slicing;
                    TargetInitialAngle = 0; //reusing this variable as a counter for Slicing
                    Time = 0;
                }
                Time++;
                break;
            case States.Slicing:
                Projectile.friendly = true;
                target = Main.npc[TargetIndex];
                if (Time <= 30)
                {
                    AfterImageOpacity = Time / 30f;
                    Projectile.rotation = StabAngle.AngleLerp((target.Center - Projectile.Center).ToRotation(), AfterImageOpacity);
                }
                else if(Time <= 300)
                {
                    Vector2 goalPosition = target.Center + StabAngle.ToRotationVector2() * 256f;
                    Vector2 toGoal = (goalPosition - Projectile.Center);

                    Projectile.velocity = toGoal.SafeNormalize(Vector2.Zero) * (toGoal.Length() / 10f);
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                    StabAngle += Lerp(0.0575f, 0f, (Time - 30) / 300f);
                }
                else if(Time <= 330)
                {
                    float reelBackSpeedExponent = 2.6f;
                    float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time - 300, true);
                    float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                    Vector2 reelBackVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                }
                else if(Time <= 385)
                {                   
                    if (Time == 331)
                    {
                        if (++TargetInitialAngle >= 3) //reusing this variable as a counter for Slicing
                        {
                            State = States.Seeking;
                            Time = 0;
                            return;
                        }
                        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 16f;
                        Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                    }
                }
                else
                {
                    StabAngle += Pi;
                    Time = 30;
                }
                Time++;
                break;
            case States.Seeking:
                target = Main.npc[TargetIndex];
                Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                break;
        }
        if (State == States.Dislodged || State == States.Slicing)
            Projectile.timeLeft = 900;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if ((State == States.Thrown || State == States.Seeking) && target.active && target.life > 0)
        {
            Projectile.Center += Projectile.velocity * 2.5f;
            TargetIndex = target.whoAmI;
            TargetInitialAngle = target.rotation;
            StabAngle = Projectile.rotation;
            StabOffset = Projectile.Center - target.Center;
            Projectile.friendly = false;
            State = States.Impaled;
        }
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        Vector2 rotation = Projectile.rotation.ToRotationVector2() * 38f;
        hitbox.Location = new Point((int)(hitbox.Location.X + rotation.X), (int)(hitbox.Location.Y + rotation.Y));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>(Texture + "WhiteOut").Value;
        Color color = Color.Black;
        switch (Projectile.localAI[0])
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

        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], color * AfterImageOpacity, 2, texture: WhiteOutTexture);
        
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle frame = tex.Frame(1, 3, 0, (int)Projectile.localAI[0]);

        Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }
}
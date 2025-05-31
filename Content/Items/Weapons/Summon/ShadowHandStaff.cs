using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Players;
using Windfall.Content.Buffs.Weapons.Minions;
using Windfall.Content.Items.Lore;

namespace Windfall.Content.Items.Weapons.Summon;

public class ShadowHandStaff : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";
    public override string Texture => "Windfall/Assets/Items/Weapons/Summon/ShadowHandStaff";
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;
        Item.damage = 470;
        Item.mana = 10;
        Item.useTime = Item.useAnimation = 34;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;
        Item.knockBack = 0.5f;
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
        Item.UseSound = SoundID.Item103;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<OratorHandMinion>();
        Item.shootSpeed = 10f;
        Item.DamageType = DamageClass.Summon;
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
            Main.spriteBatch.DrawLineBetween(new Vector2(box.X, i) + Main.screenPosition, new Vector2(box.X + box.Width, i) + Main.screenPosition, Color.Lerp(new Color(0, 150, 50) * 0.5f, new Color(0, 64, 44) * 0.5f, value), 1f);
        }

        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.BottomLeft() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.TopLeft() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.BottomRight() + Main.screenPosition, box.TopRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

        Main.spriteBatch.DrawLineBetween(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, new(48, 38, 8), 8f);
        Main.spriteBatch.DrawLineBetween(box.BottomLeft() + Main.screenPosition, box.BottomRight() + Main.screenPosition, Color.DarkGoldenrod, 4f);

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

    public override void HoldItem(Player player)
    {
        player.Calamity().rightClickListener = true;
        player.Calamity().mouseWorldListener = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse != 2)
        {
            if (player.slotsMinions + 4 > player.maxMinions)
                return false;

            position = Main.MouseWorld;
            int mainHand = Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI,-1);

            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));

            float xOff = (player.Center.X - position.X) * 2;
            position.X += xOff;
            int subHand = Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI, mainHand);

            Main.projectile[mainHand].ai[0] = subHand;

            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));
        }

        return false;
    }
}

public class OratorHandMinion : ModProjectile
{
    public override string Texture => "Windfall/Assets/NPCs/Enemies/Orator_Hand";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 9;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 75;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.minionSlots = 2f;
        Projectile.timeLeft = 18000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.manualDirectionChange = true;
        Projectile.scale = 1.25f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 24;
        Projectile.hide = true;
    }

    public Player Owner => Main.player[Projectile.owner];
    public NPC Target
    {
        get
        {
            NPC target = null;

            if (Owner.HasMinionAttackTargetNPC)
                target = CheckNPCTargetValidity(Main.npc[Owner.MinionAttackTargetNPC]);

            if (target != null)
                return target;

            else
            {
                for (int npcIndex = 0; npcIndex < Main.npc.Length; npcIndex++)
                {
                    target = CheckNPCTargetValidity(Main.npc[npcIndex]);
                    if (target != null)
                        return target;
                }
            }

            return null;
        }
    }
    public static float AggroRange => 2000f;
    public NPC CheckNPCTargetValidity(NPC potentialTarget)
    {
        if (potentialTarget.CanBeChasedBy(this, false))
        {
            float targetDist = Vector2.Distance(potentialTarget.Center, Projectile.Center);

            if ((targetDist < AggroRange) && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, potentialTarget.position, potentialTarget.width, potentialTarget.height))
            {
                return potentialTarget;
            }
        }

        return null;
    }

    public int OtherHand => (int)Projectile.ai[0];
    
    public int HandSide => Projectile.whoAmI < OtherHand ? -1 : 1;

    public enum AIState
    {
        NoTarget,
        Punch,
        Orbit,
        Conjure
    }
    AIState CurrentAI = AIState.NoTarget;

    public enum Poses
    {
        Default,
        Fist,
        Palm,
        Gun
    }
    Poses Pose = Poses.Default;

    Rectangle cuffFrame = new();
    int cuffCounter = 0;

    public int SharedTime
    {
        get => (int)(HandSide == 1 ? Projectile.ai[1] : Main.projectile[OtherHand].ai[1]); 
        
        set
        {
            if (HandSide == 1)
                Projectile.ai[1] = value;
            else
                Main.projectile[OtherHand].ai[1] = value;
        }
    }

    public bool SharedAttackBool
    {
        get => (HandSide == 1 ? Projectile.ai[2] : Main.projectile[OtherHand].ai[2]) != 0;

        set
        {
            if (HandSide == 1)
                Projectile.ai[2] = value ? 1 : 0;
            else
                Main.projectile[OtherHand].ai[2] = value ? 1 : 0;
        }
    }

    bool LocalAttackBool = false;
    int LocalTime = 0;

    public override void AI()
    {
        #region Frames
        int frameWidth = TextureAssets.Projectile[Type].Width() / 4;

        if (Pose != Poses.Default)
            Projectile.frame = 0;
        else
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 9)
                    Projectile.frame = 0;
            }
        }
        cuffCounter++;
        if (cuffCounter >= 16)
        {
            cuffCounter = 0;
            cuffFrame.Y += cuffFrame.Height;
            if (cuffFrame.Y >= cuffFrame.Height * 6)
                cuffFrame.Y = 0;
        }
        #endregion           

        Player player = Main.player[Projectile.owner];

        #region Buff
        BuffPlayer buffPlayer = player.Buff();

        player.AddBuff(ModContent.BuffType<ShadowHandBuff>(), 3600);
        if (player.dead)
            buffPlayer.OratorMinions = false;
        #endregion

        if (buffPlayer.OratorMinions)
            Projectile.timeLeft = 2;

        NPC target = Target;
        if (target == null)
            CurrentAI = AIState.NoTarget;
        else if (CurrentAI == AIState.NoTarget)
        {
            SharedTime = 1;
            SharedAttackBool = false;
            LocalAttackBool = false;
            LocalTime = 0;
            CurrentAI = AIState.Punch;
        }

        switch(CurrentAI)
        {
            case AIState.NoTarget:
                Pose = Poses.Default;

                Vector2 goalPos = player.Center + player.velocity + new Vector2(124 * HandSide, +75);
                goalPos.Y += (float)Math.Sin(SharedTime / 20f) * 16f;

                #region Movement
                Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                Projectile.rotation = (-3 * Pi / 2) - (Pi / 8 * HandSide);
                Projectile.direction = HandSide;
                #endregion
                break;
            case AIState.Punch:
                Pose = Poses.Fist;
                Vector2 toTarget;
                Vector2 targetHeading = target.Center + target.velocity;

                if (!LocalAttackBool)
                {
                    toTarget = (targetHeading - Owner.Center).SafeNormalize(Vector2.Zero);
                    goalPos = targetHeading - (toTarget.RotatedBy(HandSide == 1 ? PiOver4 : -PiOver4) * (180 + Max(target.width, target.height) / 2f));

                    goalPos.Y += (float)Math.Sin(SharedTime / 15f) * 56f * HandSide;

                    #region Movement
                    Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                    Projectile.rotation = toTarget.ToRotation() + (PiOver2 * Math.Sign(-toTarget.X));
                    Projectile.direction = Math.Sign(-toTarget.X);
                    #endregion

                    int attackDelay = 45;

                    if(SharedTime % attackDelay == 0)
                    {
                        if (SharedAttackBool && HandSide == 1)
                            LocalAttackBool = true;
                        else if (!SharedAttackBool && HandSide == -1)
                            LocalAttackBool = true;
                    }
                    if(SharedTime % attackDelay == 1)
                        SharedAttackBool = !SharedAttackBool;
                }
                else
                {
                    toTarget = (targetHeading - Projectile.Center).SafeNormalize(Vector2.Zero);

                    if (LocalTime < 0)
                    {
                        Projectile.velocity *= 0.9f;

                        if (LocalTime == -1)
                        {
                            LocalAttackBool = false;
                            LocalTime = 0;
                        }
                        else
                            LocalTime++;
                        return;
                    }

                    if (LocalTime < 10)
                    {
                        float reelBackSpeedExponent = 2.6f;
                        float reelBackCompletion = Utils.GetLerpValue(0f, 10, LocalTime, true);
                        float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                        Vector2 reelBackVelocity = toTarget * -reelBackSpeed;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                    }
                    else if (LocalTime == 10)
                        Projectile.velocity = toTarget * 64f;
                    else
                    {
                        Projectile.velocity.RotateTowards(toTarget.ToRotation(), 0.05f);
                        Projectile.velocity *= 0.9f;
                    }

                    Projectile.rotation = Projectile.velocity.ToRotation();
                    if (LocalTime < 10)
                    {
                        Projectile.rotation += Pi;
                        Projectile.direction = Math.Sign(toTarget.X);
                    }
                    Projectile.scale = 1.5f;

                    if (LocalTime >= 30)
                    {
                        LocalAttackBool = false;
                        LocalTime = 0;
                    }
                    else
                        LocalTime++;
                }
                break;
            case AIState.Orbit:
                break;
            case AIState.Conjure:
                break;
        }

        Vector2 rotVec = Projectile.rotation.ToRotationVector2();
        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - (rotVec * (Projectile.width / (Main.rand.NextFloat(1.5f, 1.75f)))) + rotVec.RotatedBy(-PiOver2 * Projectile.direction) * Main.rand.NextFloat(0f, 48f), (rotVec.RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(1f, 5f)), Main.rand.NextFloat(20f, 30f));
        
        if(HandSide == 1)
            SharedTime++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if(CurrentAI == AIState.Punch && LocalAttackBool)
        {
            int timeSaved = 30 - LocalTime;
            SharedTime += timeSaved;

            LocalTime = -10;
            Projectile.velocity = -Projectile.velocity * 0.2f;

            Vector2 toProjectile = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero);
            float size = (target.width + target.height) / 2f;

            for (int i = 0; i < 16; i++)
            {
                float rot = Main.rand.NextFloat(-1f, 1f);
                Vector2 dir = toProjectile.RotatedBy(rot);
                float scale = 1f - Math.Abs(rot);
                Vector2 hitLocation = target.Center + dir * size / 2f;
                SparkParticle particle = new(hitLocation, dir * Main.rand.NextFloat(2f, 12f), false, 24, scale * Main.rand.NextFloat(0.8f, 1.6f), Main.rand.NextBool() ? Color.Orange : Color.LimeGreen);
                GeneralParticleHandler.SpawnParticle(particle);
            }
            //DirectionalPulseRing ring = new(target.Center, Vector2.Zero, Main.rand.NextBool() ? Color.Orange : Color.LimeGreen, 0f, size / 64f, 12);
            //GeneralParticleHandler.SpawnParticle(ring);

        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (Projectile.Center.Y > Main.projectile[OtherHand].Center.Y)
            behindNPCs.Add(index);
        else
            behindProjectiles.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Rectangle frame = texture.Frame(4, Main.projFrames[Type], (int)Pose, Projectile.frame);

        Vector2 origin = frame.Size() * 0.5f;
        origin.X *= 0.75f;

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.direction == -1)
            spriteEffects = SpriteEffects.FlipVertically;
        Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/NPCs/Enemies/Orator_Hand_Cuffs").Value;

        cuffFrame.Width = texture.Width;
        cuffFrame.Height = texture.Height / 9;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = cuffFrame.Size() * 0.5f;
        origin.X *= 0.8f;
        origin.Y -= 1;
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.direction == -1)
        {
            spriteEffects = SpriteEffects.FlipVertically;
            origin.Y += 2;
        }
        Main.EntitySpriteDraw(texture, drawPosition, cuffFrame, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
    }
}


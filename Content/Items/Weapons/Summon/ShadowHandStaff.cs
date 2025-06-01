using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Summon;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Utilities;
using System.Collections.ObjectModel;
using Terraria.Graphics.Shaders;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Players;
using Windfall.Common.Systems;
using Windfall.Content.Buffs.StatBuffs;
using Windfall.Content.Buffs.Weapons.Minions;
using Windfall.Content.Items.Weapons.Magic;
using Windfall.Content.Items.Weapons.Melee;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Skies;

namespace Windfall.Content.Items.Weapons.Summon;

public class ShadowHandStaff : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Summon";
    public override string Texture => "Windfall/Assets/Items/Weapons/Summon/ShadowHandStaff";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 4;
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;
        Item.damage = 470;
        Item.mana = 10;
        Item.useTime = Item.useAnimation = 24;
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
        Item.channel = true;
        Item.autoReuse = true;
    }

    public int GrazePoints = 0;
    private static readonly int GrazeMax = 100;
    float MaxGraze = 100f;


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
    }

    public override bool AltFunctionUse(Player player) => GrazePoints > 0;

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse != 2)
        {
            if (player.slotsMinions + 4 > player.maxMinions)
                return false;

            CalamityUtils.KillShootProjectiles(true, type, player);

            position = Main.MouseWorld;

            int mainHand = Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI, -1);

            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));

            float xOff = (player.Center.X - position.X) * 2;
            position.X += xOff;
            int subHand = Projectile.NewProjectile(source, position, velocity, type, Item.damage, knockback, player.whoAmI, mainHand);

            Main.projectile[mainHand].ai[0] = subHand;

            for (int i = 0; i <= 20; i++)
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));

            if ((position - player.Center).X < 0)
            {
                (Main.projectile[subHand].position, Main.projectile[mainHand].position) = (Main.projectile[mainHand].position, Main.projectile[subHand].position);
            }
        }

        return false;
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Player myPlayer = Main.LocalPlayer;

        if (GrazePoints == 0 && !myPlayer.Calamity().mouseRight)
            return;


        if (myPlayer.HeldItem() != Item || !myPlayer.active || myPlayer.dead)
            return;

        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        float barScale = 1f;

        var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
        var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

        Vector2 barOrigin = barBG.Size() * 0.5f;
        Vector2 drawPos = (myPlayer.Center - Main.screenPosition) + Vector2.UnitY * barScale * myPlayer.height;
        Rectangle frameCrop = new(0, 0, (int)(GrazePoints / 100f * barFG.Width), barFG.Height);

        Color bgColor = Color.DarkGray * 0.5f;
        bgColor.A = 255;

        spriteBatch.Draw(barBG, drawPos, null, bgColor, 0f, barOrigin, barScale, 0f, 0f);
        spriteBatch.Draw(barFG, drawPos, frameCrop, Color.Lerp(Color.Yellow, Color.LimeGreen, GrazePoints / 100f), 0f, barOrigin, barScale, 0f, 0f);

        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
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
        if (potentialTarget.CanBeChasedBy(this))
        {
            float targetDist = Vector2.Distance(potentialTarget.Center, Projectile.Center);

            if (targetDist < AggroRange)
                return potentialTarget;
        }

        return null;
    }

    public int OtherHand => (int)Projectile.ai[0];
    
    public int HandSide => Projectile.whoAmI < OtherHand ? -1 : 1;

    public enum AIState
    {
        NoTarget,
        Punch,
        Protect,
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

    PulseRing grazeArea;
    int grazeTime = 0;
    int consumedGraze = 0;
    public Vector2 attackVec;

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
        if(CurrentAI <= AIState.Protect)
        {
            if (CurrentAI == AIState.Protect)
            {
                if (player.HeldItem().type != ModContent.ItemType<ShadowHandStaff>() || !player.Calamity().mouseRight)
                {
                    CurrentAI = AIState.NoTarget;
                    grazeTime = 0;
                }
            }
            else if (player.HeldItem().type == ModContent.ItemType<ShadowHandStaff>() && player.Calamity().mouseRight)
            {
                consumedGraze = ((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints;
                if (consumedGraze == 0)
                {
                    CurrentAI = AIState.Protect;
                    if (HandSide == 1)
                    {
                        grazeArea = new(Owner.Center, Vector2.Zero, Color.LimeGreen, 0f, 1f, 20);
                        GeneralParticleHandler.SpawnParticle(grazeArea);
                    }
                }
                else if (consumedGraze == 100)
                {
                    CurrentAI = AIState.Conjure;
                }
                else
                {
                    LocalTime = 0;
                    attackVec = Vector2.UnitX;
                    CurrentAI = AIState.Orbit;
                }

                if (HandSide == 1)
                    ((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints = 0;
                SharedTime = 0;
            }
        }

        if (target == null && CurrentAI != AIState.Protect)
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

                Vector2 goalPos = player.Center + player.velocity + new Vector2(128 * HandSide, +75);
                goalPos.Y += (float)Math.Sin(SharedTime / 20f) * 16f;

                #region Movement
                Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                Projectile.rotation = (-3 * Pi / 2) - (Pi / 8 * HandSide);
                Projectile.direction = HandSide;
                #endregion
                break;
            case AIState.Punch:
                Pose = Poses.Fist;

                Vector2 targetHeading = target.Center + target.velocity;
                Vector2 toTarget = (targetHeading - Projectile.Center).SafeNormalize(Vector2.Zero);
                float toTargetRot = toTarget.ToRotation();

                if (!LocalAttackBool)
                {
                    Vector2 fromPlayer = (targetHeading - Owner.Center).SafeNormalize(Vector2.Zero);
                    goalPos = targetHeading - (fromPlayer.RotatedBy(HandSide == 1 ? PiOver4 : -PiOver4) * (180 + Max(target.width, target.height) / 2f));

                    goalPos.Y += (float)Math.Sin(SharedTime / 15f) * 56f * HandSide;

                    #region Movement
                    Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                   
                    Projectile.rotation = toTargetRot;
                    Projectile.direction = Math.Sign(toTarget.X);
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
                        Projectile.velocity.RotateTowards(toTargetRot, 0.05f);
                        Projectile.velocity *= 0.9f;
                    }

                    if (LocalTime < 10)
                    {
                        Projectile.rotation = toTargetRot;
                        Projectile.direction = Math.Sign(toTarget.X);
                    }
                    else
                    {
                        Projectile.rotation = Projectile.velocity.ToRotation();
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
            case AIState.Protect:
                Pose = Poses.Palm;

                goalPos = player.Center + player.velocity + new Vector2(64 * HandSide, 0);
                goalPos.Y += (float)Math.Sin(SharedTime / 20f) * 16f * HandSide;

                #region Movement
                Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                Projectile.rotation = -PiOver2;
                Projectile.direction = -HandSide;
                #endregion

                int grazeRadius = 76;

                if (HandSide == 1)
                {
                    if (SharedTime >= 10)
                    {
                        GeneralParticleHandler.RemoveParticle(grazeArea);
                        grazeArea = new(Owner.Center, Vector2.Zero, Color.LimeGreen, 1f, 1f, 24);
                        GeneralParticleHandler.SpawnParticle(grazeArea);

                        if (((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints < 100 && grazeTime == 0 && Owner.immuneTime == 0 && !Owner.immune)
                        {
                            int projectilesInGraze = Main.projectile.Count(p => p.active && p.hostile && !Owner.Hitbox.Intersects(p.Hitbox) && (p.Center - Owner.Center).Length() < grazeRadius);
                            if (projectilesInGraze > 0)
                            {
                                ((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints += projectilesInGraze;
                                if (((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints > 100)
                                    ((ShadowHandStaff)Owner.ActiveItem().ModItem).GrazePoints = 100;
                                grazeTime = 4;
                            }
                        }

                        Owner.AddBuff(ModContent.BuffType<ApotropaicEmbrace>(), 2);
                    }

                    if (grazeTime > 0)
                        grazeTime--;
                }
                break;
            case AIState.Orbit:
                Projectile.netUpdate = true;
                Pose = Poses.Fist;

                if (HandSide == -1)
                {
                    if (SharedTime < 90)
                    {
                        attackVec = attackVec.RotatedBy(Lerp(0.15f, 0.025f, SharedTime / 90f));
                        attackVec.Normalize();
                        //direction *= WhatHand;
                        goalPos = Target.Center + (attackVec * 500);

                        #region Movement
                        Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                        Projectile.rotation = Projectile.DirectionTo(Target.Center).ToRotation();
                        if (Projectile.Center.X > Target.Center.X)
                            Projectile.direction = -1;
                        else
                            Projectile.direction = 1;
                        #endregion
                    }
                    else
                    {
                        if (SharedTime < 110)
                        {
                            if (SharedTime == 90)
                            {
                                attackVec = (Target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX * Projectile.direction);
                                LocalAttackBool = false;
                            }
                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, 20, SharedTime - 90, true);
                            float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = attackVec * -reelBackSpeed;
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                        }
                        else
                        {
                            if (SharedTime == 110)
                                Projectile.velocity = attackVec * 75;
                            Projectile.velocity *= 0.93f;
                            Projectile subHand = Main.projectile[OtherHand];
                            if (Projectile.Hitbox.Intersects(subHand.Hitbox) && !LocalAttackBool)
                            {
                                Vector2 midPoint = Projectile.Center + ((subHand.Center - Projectile.Center) / 2);

                                Projectile.Center = midPoint - (Projectile.rotation.ToRotationVector2() * (Projectile.width / 3f));
                                subHand.Center = midPoint - (subHand.rotation.ToRotationVector2() * (subHand.width / 3f));


                                ScreenShakeSystem.StartShake(9f);
                                SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, midPoint);
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int projCount = consumedGraze / 6;
                                    for (int i = 0; i < projCount; i++)
                                    {
                                        Vector2 velocity = (Vector2.UnitY * -1).RotatedBy(Main.rand.NextFloat(-PiOver2, PiOver2)) * Main.rand.NextFloat(4f, 8f);
                                        velocity.Y *= 2;
                                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), midPoint, velocity, ModContent.ProjectileType<MinionHandRing>(), Projectile.damage / 2, 0f, Owner.whoAmI, ai2: Target.whoAmI);
                                    }
                                }
                                PulseRing pulse = new(midPoint, Vector2.Zero, new(253, 189, 53), 0f, 3f, 16);
                                GeneralParticleHandler.SpawnParticle(pulse);
                                DetailedExplosion explosion = new(midPoint, Vector2.Zero, new(255, 133, 187), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                                GeneralParticleHandler.SpawnParticle(explosion);

                                Projectile.velocity = Vector2.Zero;
                                subHand.velocity = Vector2.Zero;

                                LocalAttackBool = true;
                                return;
                            }
                            else if (LocalAttackBool)
                            {
                                Projectile.velocity = Vector2.Zero;

                                if (LocalTime >= 60)
                                    CurrentAI = AIState.Punch;
                                LocalTime++;
                            }
                        }
                    }
                }
                else
                {
                    if (SharedTime >= 0)
                    {
                        Projectile mainHand = Main.projectile[OtherHand];
                        attackVec = ((OratorHandMinion)mainHand.ModProjectile).attackVec;
                        if (SharedTime < 90)
                        {
                            //direction *= WhatHand;
                            goalPos = Target.Center + (attackVec * -500);

                            #region Movement
                            Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                            Projectile.rotation = mainHand.rotation + Pi;
                            Projectile.direction = mainHand.direction * -1;
                            #endregion
                        }
                        else
                        {
                            if (SharedTime < 110)
                            {
                                float reelBackSpeedExponent = 2.6f;
                                float reelBackCompletion = Utils.GetLerpValue(0f, 20, SharedTime - 90, true);
                                float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                                Vector2 reelBackVelocity = attackVec * reelBackSpeed;
                                Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                            }
                            else if (!((OratorHandMinion)mainHand.ModProjectile).LocalAttackBool)
                            {
                                if (SharedTime == 110)
                                    Projectile.velocity = attackVec * -75;
                                Projectile.velocity *= 0.93f;
                                Projectile.velocity = Projectile.velocity.RotateTowards((mainHand.Center - Projectile.Center).ToRotation(), PiOver4);
                            }
                            else
                            {
                                Projectile.velocity = Vector2.Zero;

                                if (LocalTime >= 60)
                                    CurrentAI = AIState.Punch;
                                LocalTime++;
                            }
                        }
                    }
                }
                break;
            case AIState.Conjure:
                Pose = Poses.Palm;

                Vector2 goalDirection = (Target.Center - Owner.Center).SafeNormalize(Vector2.Zero);
                goalPos = Owner.Center + goalDirection * 175f;
                Projectile.direction = -HandSide;
                if (HandSide == 1)
                {
                    Vector2 rotation = goalDirection.RotatedBy(-PiOver2);
                    goalPos -= rotation * Lerp(32, 150, SharedTime / 200f);
                    Projectile.rotation = rotation.ToRotation();

                    EmpyreanMetaball.SpawnDefaultParticle(Owner.Center + goalDirection * 150f, Main.rand.NextVector2Circular(5f, 5f), 1.5f * (SharedTime / 2));
                }
                else
                {
                    Vector2 rotation = goalDirection.RotatedBy(PiOver2);
                    goalPos -= rotation * Lerp(80, 160, SharedTime / 200f);
                    Projectile.rotation = rotation.ToRotation();
                }
                Projectile.velocity = (goalPos - Projectile.Center).SafeNormalize(Vector2.Zero) * ((goalPos - Projectile.Center).Length() / 10f);
                Projectile.rotation += PiOver2 * HandSide;

                if (SharedTime > 200)
                {
                    CurrentAI = AIState.Punch;
                    if (HandSide == 1)
                    {
                        DirectionalPulseRing pulse = new(Owner.Center, goalDirection * 8f, new(117, 255, 159), new Vector2(0.5f, 1f), goalDirection.ToRotation(), 0f, 3f, 32);
                        GeneralParticleHandler.SpawnParticle(pulse);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Owner.Center + goalDirection * 150f, goalDirection * 40, ModContent.ProjectileType<SelenicIdolMinion>(), (int)(Projectile.damage * 1.5f), 0f, Owner.whoAmI);
                    }
                }
                break;
        }

        if(grazeArea != null)
            grazeArea.Position = Owner.Center;

        Vector2 rotVec = Projectile.rotation.ToRotationVector2();
        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center - (rotVec * (Projectile.width / (Main.rand.NextFloat(1.5f, 1.75f)))) + rotVec.RotatedBy(-PiOver2 * Projectile.direction) * Main.rand.NextFloat(0f, 48f), (rotVec.RotatedBy(Pi + Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(1f, 5f)), Main.rand.NextFloat(20f, 30f));
        
        if(HandSide == 1)
            SharedTime++;
    }

    public override bool? CanHitNPC(NPC target) => Pose == Poses.Fist && LocalAttackBool;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if(CurrentAI == AIState.Punch && LocalAttackBool)
        {
            int timeSaved = 30 - LocalTime;
            SharedTime += timeSaved;

            LocalTime = -1;
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
        Texture2D texture = OratorHand.Cuffs.Value;

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

public class MinionHandRing : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/HandRings";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
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
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float AfterImageOpacity => ref Projectile.ai[1];

    private bool spinDir = false;

    private Vector2 truePosition = Vector2.Zero;

    public override void OnSpawn(IEntitySource source)
    {
        spinDir = Main.rand.NextBool();
        Projectile.localAI[0] = Main.rand.Next(3);
        Projectile.localAI[1] = Main.rand.Next(4);
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        //Projectile.velocity *= 0.9925f;
        if (spinDir)
            Projectile.rotation += 0.01f * Projectile.velocity.Length();
        else
            Projectile.rotation -= 0.01f * Projectile.velocity.Length();

        NPC target = Main.npc[(int)Projectile.ai[2]];

        if (Time < 60)
        {
            Projectile.velocity.Y += 0.25f;
            Projectile.velocity *= (1 - AfterImageOpacity);

            if (Time > 30 && Time < 60)
                AfterImageOpacity = (Time - 30) / 30f;
        }
        else if (Time < 210)
        {
            if (Time == 60)
                Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            if(target != null && target.active)
                Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), 0.33f);
            float maxSpeed = 18f;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * Clamp(Lerp(0.01f, maxSpeed, CircOutEasing((Time - 60) / 120)), 0.01f, maxSpeed);

            AfterImageOpacity = 1f;
        }
        else
        {
            Projectile.velocity *= 0.97f;
        }

        Lighting.AddLight(Projectile.Center, EmpyreanMetaball.BorderColor.ToVector3());
        Time++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D WhiteOutTexture = (Projectile.localAI[1] == 0 ? HandRing.WhiteOut0 : HandRing.WhiteOut1).Value; 
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
        if (Projectile.timeLeft <= 90)
            color = Color.Lerp(color, EmpyreanMetaball.BorderColor, (90 - Projectile.timeLeft) / 60f);
        DrawCenteredAfterimages(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], color * AfterImageOpacity, 2, texture: WhiteOutTexture);

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, null, color * Projectile.Opacity, Projectile.rotation, WhiteOutTexture.Size() * 0.5f, Projectile.scale * 1.25f * CircOutEasing(AfterImageOpacity), SpriteEffects.None);

        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = tex.Frame(3, 4, (int)Projectile.ai[1], (int)Projectile.localAI[1]);

        Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        if (Projectile.timeLeft > 90)
            return;

        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.localAI[1] == 0 ? 0 : 1)).Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float ratio = 0f;
        if (Projectile.timeLeft <= 90)
            ratio = (90 - Projectile.timeLeft) / 60f;
        ratio = Clamp(ratio, 0f, 1f);
        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, WhiteOutTexture.Frame(), Color.White * AfterImageOpacity, Projectile.rotation, WhiteOutTexture.Frame().Size() * 0.5f, Projectile.scale * ratio, SpriteEffects.None);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 10; i++)
        {
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 20f));
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(spinDir);

        writer.Write(truePosition.X);
        writer.Write(truePosition.Y);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        spinDir = reader.ReadBoolean();

        truePosition = reader.ReadVector2();
    }
}

public class SelenicIdolMinion : ModProjectile, ILocalizedModType
{
    public new static string LocalizationCategory => "Projectiles.Summon";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/GoldenMoon";

    SlotId loopingSoundSlot;

    private static float Acceleration = 0.5f;
    private static int MaxSpeed = 12;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 320;
        Projectile.height = 320;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 36000;
        Projectile.scale = 1f;
        Projectile.alpha = 0;
        Projectile.Calamity().DealsDefenseDamage = true;
        Projectile.netImportant = true;
    }

    private int Time
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    private enum States
    {
        Chasing,
        Dying,
        Exploding,
    }
    private States AIState = States.Chasing;

    private ref float GoopScale => ref Projectile.ai[2];

    int deathCounter = 0;
    float rotationCounter = 0;

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, Projectile.Center);
        ScreenShakeSystem.StartShake(5f);
        for (int i = 0; i <= 50; i++)
        {
            Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f) * 10;
            EmpyreanMetaball.SpawnDefaultParticle(spawnPos, (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * 4, 40 * Main.rand.NextFloat(3f, 5f));
        }
    }

    public override void AI()
    {
        switch (AIState)
        {
            case States.Chasing:
                if (Time <= 60)
                {
                    float lerp = Time / 60f;
                    Projectile.scale = CircOutEasing(lerp);
                    GoopScale = 1 - SineInEasing(lerp);
                    EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(48f, 48f) * Projectile.scale), Main.rand.NextVector2Circular(18, 18) + Projectile.velocity, 200 * Main.rand.NextFloat(0.75f, 0.9f) * (1 - lerp));
                }

                if (Projectile.owner == Main.myPlayer)
                    Projectile.velocity += (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;

                if (Projectile.velocity.Length() > MaxSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.velocity.Length() * 0.95f);

                #region Looping Sound
                if (!SoundEngine.TryGetActiveSound(loopingSoundSlot, out var activeSound))
                {
                    // if it isn't, play the sound and remember the SlotId
                    var tracker = new ProjectileAudioTracker(Projectile);
                    loopingSoundSlot = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop, Projectile.position, soundInstance => {
                        // This example is inlined, see ActiveSoundShowcaseProjectile.cs for other approaches
                        soundInstance.Position = Projectile.position;
                        return tracker.IsActiveAndInGame();
                    });
                }
                #endregion

                if (Time > 900)
                {
                    AIState = States.Dying;
                    Time = 0;
                }
                break;
            case States.Dying:
                float lerpValue = deathCounter / 180f;

                Projectile.scale = Lerp(1f, 0.5f, lerpValue);
                GoopScale = lerpValue;

                if (lerpValue >= 1f)
                    Projectile.ai[0] = 2;
                if (Projectile.velocity.Length() > 0f)
                {
                    if (Projectile.owner == Main.myPlayer)
                        Projectile.velocity += (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * Acceleration;

                    if (Projectile.velocity.Length() > (MaxSpeed * Projectile.scale))
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (MaxSpeed * Projectile.scale);
                }
                if (deathCounter > 120)
                {
                    Projectile.velocity = Vector2.Zero;
                    AIState = States.Exploding;
                }

                #region Death Shake
                float ShakeBy = Lerp(0f, 18f, lerpValue);
                Vector2 shakeOffset = new(Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2), Main.rand.NextFloat(-ShakeBy, ShakeBy) / (Projectile.scale * 2));
                Projectile.position += shakeOffset;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                    Projectile.oldPos[i] += shakeOffset;
                #endregion

                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(12, 12) * (lerpValue + 0.5f), 180 * (Main.rand.NextFloat(0.75f, 0.9f)));
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (Main.rand.NextVector2Circular(25f, 25f) * Projectile.scale), Main.rand.NextVector2Circular(18, 18) * (lerpValue + 0.5f), 90 * (Main.rand.NextFloat(0.75f, 0.9f)));

                deathCounter++;
                break;
            case States.Exploding:
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
                ScreenShakeSystem.StartShake(7.5f);
                for (int i = 0; i <= 50; i++)
                    EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 24; i++)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, (TwoPi / 24 * i).ToRotationVector2(), ModContent.ProjectileType<RushBolt>(), (int)(Projectile.damage / 2f), 0f, -1, 0, i % 2 == 0 ? -10 : 0, i % 2 == 0 ? 1 : 0);
                }

                PulseRing pulse = new(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
                GeneralParticleHandler.SpawnParticle(pulse);
                DetailedExplosion explosion = new(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
                GeneralParticleHandler.SpawnParticle(explosion);

                Projectile.active = false;
                Projectile.netUpdate = true;
                break;
        }
        Time++;
        Projectile.rotation = Projectile.velocity.ToRotation();
        rotationCounter += 0.01f;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CircularHitboxCollision(Projectile.Center, projHitbox.Width / 2, targetHitbox))
            return true;
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"].SetTexture(LoadSystem.SwordSlash);

        CalamityMod.Graphics.Primitives.PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:PhaseslayerRipEffect"]), 40);

        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Texture2D tex = OratorSky.MoonBloom.Value;

        Color[] colors = [
            Color.Gold,
            Color.Goldenrod,
            Color.DarkGoldenrod,
            Color.Goldenrod,
        ];

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, LerpColors(Main.GlobalTimeWrappedHourly * 0.25f, colors), Main.GlobalTimeWrappedHourly * 0.25f, tex.Size() * 0.5f, ((Projectile.scale * 0.825f) + (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.025f)) * (1 - GoopScale), 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        tex = TextureAssets.Projectile[Type].Value;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale, 0);

        return false;
    }

    internal Color ColorFunction(float completionRatio)
    {
        float opacity = Projectile.Opacity;
        opacity *= (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D);

        if (deathCounter > 0)
            opacity = Clamp(Lerp(opacity, -0.5f, deathCounter / 90f), 0f, 1f);

        return Color.Lerp(Color.Gold, new(170, 100, 30), (completionRatio) * 3f) * opacity * (Projectile.velocity.Length() / MaxSpeed);
    }

    internal float WidthFunction(float completionRatio) => 200f * (1f - completionRatio) * 0.8f * Projectile.scale;

    public override void PostDraw(Color lightColor)
    {
        if (GoopScale != 0)
        {
            Texture2D tex = LoadSystem.Circle.Value;

            Vector2[] offsets = [new(70, -35), new(-78, 48), new(-0, 80), new(78, 0), new(34, 70), new(-44, -70)];

            for (int i = 0; i < offsets.Length; i++)
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + offsets[i].RotatedBy(rotationCounter) * Projectile.scale, null, Color.White, rotationCounter, tex.Size() * 0.5f, Projectile.scale * 4f * GoopScale, 0);
        }
    }
}


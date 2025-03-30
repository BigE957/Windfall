using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
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
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaThrow";

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


        Rectangle box = new(x - xOffset, y - yOffset, (int)(longestLength) - yOffset * 7, lineCount * 28 + yOffset + (int)Math.Ceiling(yOffset / 2f));

        Utils.DrawInvBG(Main.spriteBatch, box, new Color(0, 150, 50) * 0.5f);
        for(int i = box.Y; i < box.Y + box.Height; i++)
        {
            float value = Utils.GetLerpValue(box.Y, box.Y + box.Height, i);
            Main.spriteBatch.DrawLineBetween(new Vector2(box.X, i) + Main.screenPosition, new Vector2(box.X + box.Width, i) + Main.screenPosition, Color.Lerp(new Color(0, 150, 50) * 0.5f, new Color(0, 64, 44), value), 1f);
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
        for(int i = 0; i < 4; i++)
        {
            float rotation = 0f;
            Vector2 position = Vector2.Zero;

            switch(i)
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
        tooltips.Add(new(WindfallMod.Instance, "LoreTab", GetWindfallTextValue(LocalizationCategory + "." + Name + ".Lore")));
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        //Updates the swing state before anything else, to ensure the state can accurately be referred to in all cases
        if(Main.projectile.Any(p => p.active && p.type == Item.shoot && p.As<ApotelesmaProj>().State == (float)AIState.Throw))
            return false;

        if (player.altFunctionUse == 2)
        {
            if(ApotelesmaCharge <= 0)
                return false;

            State = AIState.Throw;

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
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, (float)State, velocity.ToRotation());
        return false;
    }

    internal Tuple<Vector2, Vector2, int>[] GemData = new Tuple<Vector2, Vector2, int>[5];

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (ApotelesmaCharge == 0 && State == AIState.UpSlice)
            return;

        Player myPlayer = Main.LocalPlayer;

        if (myPlayer.HeldItem() != Item || !myPlayer.active || myPlayer.dead)
        {
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
                currentPos = Vector2.Lerp(GemData[i].Item1, GemData[i].Item2, SineOutEasing(counter / 30f));
            else
                currentPos = GemData[i].Item2;            

            float rotation = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.5f + (offsetValue * 24)) * PiOver4 * 0.34f;

            scale = 1.05f + 0.05f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.5f + (offsetValue * 24));

            Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/Apotelesma/JadeOrb").Value;
            Rectangle gemFrame = tex.Frame(4, 1);

            //Main.spriteBatch.Draw(screwOutlineTex, position, null, Color.Lerp(Color.GreenYellow, Color.White, (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.5f + 0.5f) * outlineOpacity, rotation, screwOutlineTex.Size() / 2f, scale, 0, 0);
            Main.spriteBatch.Draw(tex, currentPos - Main.screenPosition, gemFrame, Color.White, rotation, gemFrame.Size() / 2f, scale, 0, 0);
        }

        spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);

        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    internal static void ShatterGem(Vector2 position)
    {
        SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with {VariantsWeights = [1f, 0f, 0f] }, position);

        for (int i = 1; i < 4; i++)
            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), position, new(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-10f, -8f)), ModContent.ProjectileType<JadeShard>(), 0, 0f, ai0: i);

        for (int i = 0; i < 8; i++)
        {
            Color color = Color.Lerp(new Color(117, 255, 159), Color.Black, (i / 8f));
            Particle p = new HeavySmokeParticle(position, Vector2.UnitY * -2 * (i+1), color, 32, 1f, (1 - (i / 8f)) * 0.5f, Main.rand.NextFloat(-0.05f, 0.05f), true);
            GeneralParticleHandler.SpawnParticle(p);
        }
    }
}

public class ApotelesmaProj : ModProjectile
{
    internal ref float State => ref Projectile.ai[0];
    private ref float Time => ref Projectile.ai[2];

    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaSwing";

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
        ConsumedCharge = 0;
        Projectile.localNPCHitCooldown = ((Apotelesma.AIState)State == Apotelesma.AIState.Spin || (Apotelesma.AIState)State == Apotelesma.AIState.Throw) ? 12 : 24;
    }

    int ConsumedCharge = 0;
    bool HeldDown = false;

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

        if (owner.altFunctionUse == 2)
        {
            if (owner.Calamity().mouseRight) //Holding Right Click, consume Gems and prep for right click attack
            {
                if(owner.HeldItem().type != ModContent.ItemType<Apotelesma>())
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.timeLeft = 240;

                HeldDown = true;

                owner.reuseDelay = 2;

                float lerpVal = 1f;
                if (Time <= 30)
                    lerpVal = Clamp(SineOutEasing(Time / 30f), 0f, 1f);
                
                float angle = -PiOver2 - PiOver4;
                if (Time <= 30)
                    angle = 0f.AngleLerp(-PiOver2 - PiOver4, lerpVal);
                angle *= owner.direction;

                Projectile.Center = owner.MountedCenter + (Vector2.UnitX * owner.direction).RotatedBy(angle) * Lerp(0, 24, lerpVal);
                Projectile.rotation = (PiOver4 * owner.direction + angle);
                Projectile.scale = Lerp(0.5f, 1f, lerpVal);
                Projectile.Opacity = Lerp(0.25f, 1f, lerpVal);

                owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, (Vector2.UnitX * owner.direction).RotatedBy(angle - Lerp(PiOver4, PiOver2, lerpVal)).ToRotation());

                Apotelesma item = (Apotelesma)owner.HeldItem().ModItem;

                if (Time != 0 && Time % 30 == 0 && item.ApotelesmaCharge > 0)
                {
                    State = (float)Apotelesma.AIState.Throw;
                    Apotelesma.ShatterGem(item.GemData[item.ApotelesmaCharge - 1].Item2);

                    for (int i = 0; i < item.ApotelesmaCharge; i++)
                    {
                        item.GemData[i] = new(item.GemData[i].Item2, item.GemData[i].Item1, 0);
                    }
                    item.ApotelesmaCharge--;
                    ConsumedCharge++;

                    if (ConsumedCharge == 5)
                    {
                        Projectile.ai[1] = (owner.Calamity().mouseWorld - owner.Center).ToRotation();

                        SoundEngine.PlaySound(TheOrator.DashWarn, owner.Center);
                        Particle pulse = new PulseRing(owner.Center, Vector2.Zero, new(117, 255, 159), 0f, 1f, 24);
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                }

                Time++;
                return;
            }
            if(HeldDown)
            {
                Projectile.timeLeft = 240;

                HeldDown = false;
                Time = 0;

                if (ConsumedCharge == 5)
                {
                    State = (float)Apotelesma.AIState.Rush;

                    SoundEngine.PlaySound(SoundID.Item71, owner.Center);
                    SoundEngine.PlaySound(TheOrator.Dash);

                    Vector2 velocity = (owner.Calamity().mouseWorld - owner.Center).SafeNormalize(Vector2.UnitX * owner.direction);

                    Particle pulse = new DirectionalPulseRing(owner.Center, velocity.SafeNormalize(Vector2.Zero) * -1.5f, new(117, 255, 159), new Vector2(0.5f, 2f), velocity.ToRotation(), 0f, 1f, 24);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
                else if (ConsumedCharge > 0)
                {
                    State = (float)Apotelesma.AIState.Throw;

                    Time = 31;
                    Projectile.timeLeft += 80 * (ConsumedCharge - 1);
                    Projectile.velocity = (owner.Calamity().mouseWorld - owner.Center).SafeNormalize(Vector2.UnitX * owner.direction) * -40;
                }
                else
                    Projectile.Kill();
                return;
            }
        }

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
                        if (Time == 31 || Projectile.ai[1] == -1)
                        {
                            NPC tryTarget = Projectile.Center.ClosestNPC(1100f, bossPriority: true);
                            if (tryTarget != null)
                                Projectile.ai[1] = tryTarget.whoAmI;
                            else
                                Projectile.ai[1] = -1;
                        }

                        Vector2 targetPosition;
                        if (Projectile.ai[1] != -1)
                            targetPosition = Main.npc[(int)Projectile.ai[1]].Center;
                        else 
                            targetPosition = owner.Calamity().mouseWorld;

                        if (Time <= 30)
                        {
                            NPC tryTarget = Projectile.Center.ClosestNPC(1100f, bossPriority: true);
                            if (tryTarget != null)
                                Projectile.ai[1] = tryTarget.whoAmI;
                            else
                                Projectile.ai[1] = -1;

                            if (Projectile.ai[1] != -1)
                                targetPosition = Main.npc[(int)Projectile.ai[1]].Center;
                            else if (Main.myPlayer == Projectile.owner)
                                targetPosition = owner.Calamity().mouseWorld;

                            float reelBackSpeedExponent = 2.6f;
                            float reelBackCompletion = Utils.GetLerpValue(0f, 30, Time, true);
                            float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                            Vector2 reelBackVelocity = (targetPosition - Projectile.Center).SafeNormalize(Vector2.UnitY) * -reelBackSpeed;
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, reelBackVelocity, 0.25f);
                        }
                        else
                        {
                            if (Time == 31)
                            {
                                SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
                                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX * owner.direction) * -40;
                            }
                            else
                            {
                                Projectile.velocity = Projectile.velocity.RotateTowards((targetPosition - Projectile.Center).ToRotation(), CalamityWorld.death ? 0.0015f : 0.00125f * (Time - 30));
                                Projectile.velocity *= 0.95f;
                                if (Projectile.velocity.LengthSquared() < 25)
                                    Time = 0;
                            }
                        }
                    }
                    else
                    {
                        Projectile.velocity = Projectile.velocity.RotateTowards((owner.Center - Projectile.Center).ToRotation(), 0.09f).SafeNormalize(Vector2.Zero) * Clamp(Projectile.velocity.Length() * 1.05f, 0f, 30f);
                        if ((owner.Center - Projectile.Center).Length() < 32)
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

                        owner.GiveIFrames(-1, 2);
                    }
                    else if (Time == 40)
                        owner.GiveIFrames(-1, 2);

                    float rotation = 0.3f;

                    if (Time >= 40)
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
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), owner.Center, rushDirection.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<RushBolt>(), (int)(Projectile.damage * 1.5f), 0f, -1, 0, -20, Time % 16 == 0 ? 1 : 0);
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), owner.Center, rushDirection.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<RushBolt>(), (int)(Projectile.damage * 1.5f), 0f, -1, 0, -20, Time % 16 == 0 ? 0 : 1);
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

        Vector2 hitDirection = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX * (Main.player[Projectile.owner].Center.X > target.Center.X ? 1 : -1));
        for(int i = 0; i < 5; i++)
        {
            Vector2 velocity = hitDirection.RotatedBy(Main.rand.NextFloat(-PiOver4, PiOver4)) * Main.rand.NextFloat(8f, 12f);
            velocity.Y -= 4;

            Particle spark = new SparkParticle(target.Center, velocity, true, 18, Main.rand.NextFloat(0.5f, 1f), new(117, 255, 159));
            GeneralParticleHandler.SpawnParticle(spark);
        }

        //if (!target.IsAnEnemy())
        //    return;

        if ((Apotelesma.AIState)State == Apotelesma.AIState.Throw || (Apotelesma.AIState)State == Apotelesma.AIState.Rush)
            return;

        Player owner = Main.player[Projectile.owner];

        if (owner.HeldItem.type != ModContent.ItemType<Apotelesma>())
            return;
        Apotelesma item = owner.HeldItem.ModItem as Apotelesma;
        int counter = item.HitCounter++;
        if (counter >= 6)
        {
            item.HitCounter = 0;
            int charge = item.ApotelesmaCharge;
            if (charge < 5)
            {
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, target.Center);
                item.GemData[charge] = new(target.Center, Vector2.Zero, 0);
                for(int i = 0; i < charge; i++)
                {
                    Tuple<Vector2, Vector2, int> data = item.GemData[i];
                    item.GemData[i] = new(data.Item2, data.Item2, 0);
                }
                item.ApotelesmaCharge++;
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
        if (HeldDown)
            return false;

        if (!scytheSpin && !scytheSlice && (Apotelesma.AIState)State != Apotelesma.AIState.Throw && (Apotelesma.AIState)State != Apotelesma.AIState.Rush)
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

        spinRotation = Lerp(spinDirection, spinDirection + Pi * 4f, SineOutEasing(spinCounter / (float)spinDuration));

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
            sliceDirection = (player.Calamity().mouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction).ToRotation();
            sliceDirection -= PiOver2;
        }
        sliceRotation = Lerp(sliceDirection, sliceDirection + (up ? -(PiOver2 + Pi) : PiOver2 + Pi), SineOutEasing(sliceCounter / ((float)sliceDuration - 4)));
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
                    scaleMult = Lerp(0.75f, 1f, SineOutEasing(sliceCounter / 8f));
                else if(sliceCounter >= sliceDuration - 10)
                    scaleMult = Lerp(1f, 0.5f, SineOutEasing((sliceCounter - (sliceDuration - 10)) / 10f));
            }
            else if (scytheSpin)
            {
                if (spinCounter <= 8)
                    scaleMult = Lerp(0.5f, 1f, SineOutEasing(spinCounter / 8f));
                else if (spinCounter >= spinDuration - 8)
                    scaleMult = Lerp(1f, 0.5f, SineOutEasing((spinCounter - (spinDuration - 8)) / 8f));
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

            Texture2D slash = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaSlash").Value;
            Texture2D slashWhiteout = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaSlashWhiteOut").Value;

            float slashFade = 1f;
            if (scytheSlice)
            {
                if (sliceCounter < sliceDuration / 2)
                    slashFade = Lerp(0f, 1f, SineOutEasing(sliceCounter / 18f));
                else if (sliceCounter >= sliceDuration - 10)
                    slashFade = Lerp(1f, 0f, SineInEasing((sliceCounter - (sliceDuration - 10)) / 10f));
            }
            else if (scytheSpin)
            {
                if (spinCounter <= 18)
                    slashFade = Lerp(0f, 1f, SineOutEasing(spinCounter / 18f));
                else if (spinCounter >= spinDuration - 16)
                    slashFade = Lerp(1f, 0f, SineInEasing((spinCounter - (spinDuration - 16)) / 16f));
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
            Texture2D tex = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Melee/Apotelesma/ApotelesmaThrow").Value;

            SpriteEffects effect = SpriteEffects.None;
            if (HeldDown && Main.player[Projectile.owner].direction == 1)
                effect = SpriteEffects.FlipHorizontally;

            Vector2 shake = Vector2.Zero;
            if(HeldDown && ConsumedCharge >= 5)
                shake = Main.rand.NextVector2Circular(8, 8);

            Main.EntitySpriteDraw(tex, Projectile.Center + shake - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() * 0.5f, 1.25f * Projectile.scale, effect);
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
        //Projectile.damage = 100;
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
        NPC target = owner.Center.ClosestNPC(900f, bossPriority: true);

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

public class JadeShard : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Weapons/Melee/Apotelesma/JadeOrb";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;
        Projectile.scale = 1f;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
    }

    private bool TileColided = false;

    public override void OnSpawn(IEntitySource source)
    {
        if (Projectile.ai[0] == 0)
            Projectile.ai[0] = Main.rand.Next(1, 4);
    }

    public override void AI()
    {
        Projectile.rotation += Projectile.velocity.X / 10f;

        if (!TileColided && Projectile.velocity.Y < 12f)
            Projectile.velocity.Y += 0.5f;
        else
            Projectile.velocity.X *= 0.975f;

        if (Projectile.timeLeft <= 60)
            Projectile.Opacity = Projectile.timeLeft / 60f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity.Y = 0;
        Projectile.Center = Projectile.Center.ToTileCoordinates().ToWorldCoordinates();
        TileColided = true;
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle gemFrame = tex.Frame(4, 1, (int)Projectile.ai[0]);

        Main.spriteBatch.Draw(tex, Projectile.Bottom - Main.screenPosition, gemFrame, Color.White * Projectile.Opacity, Projectile.rotation, gemFrame.Size() / 2f, Projectile.scale, 0, 0);
        
        return false;
    }
}

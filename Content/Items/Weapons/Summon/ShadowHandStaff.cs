using CalamityMod;
using CalamityMod.Items;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Players;
using Windfall.Content.Buffs.Weapons.Minions;
using Windfall.Content.Projectiles.Boss.Orator;

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
        Item.shoot = ModContent.ProjectileType<ShadowHand_Minion>();
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
        tooltips.Add(new(WindfallMod.Instance, "LoreTab", GetWindfallTextValue(LocalizationCategory + "." + Name + ".Lore")));
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
            position = Main.MouseWorld;
            int seeker = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
            if (Main.projectile.IndexInRange(seeker))
                Main.projectile[seeker].originalDamage = Item.damage;
            for (int i = 0; i <= 20; i++)
            {
                EmpyreanMetaball.SpawnDefaultParticle(position, Main.rand.NextVector2Circular(5f, 5f), 20 * Main.rand.NextFloat(1.5f, 2.3f));
            }
        }
        return false;
    }
}

public class ShadowHand_Minion : ModProjectile
{
    public override string Texture => "Windfall/Assets/NPCs/Enemies/ShadowHand";
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 6;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 78;
        Projectile.height = 50;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.minionSlots = 2f;
        Projectile.timeLeft = 18000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft *= 5;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.manualDirectionChange = true;
        Projectile.scale = 1.25f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 60;
    }
    internal enum AIState
    {
        Spawning,
        NoTarget,
        Hunting,
        Recoil,
        Dashing,
        Globbing,
        Sacrifice,
    }
    internal AIState CurrentAI
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }
    private int aiCounter
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    private bool attackBool = false;
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
    public static float AggroRange = 1600f;
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

    public override void OnSpawn(IEntitySource source)
    {
        CurrentAI = AIState.Spawning;
        Projectile.velocity = Main.rand.NextFloat(0, TwoPi).ToRotationVector2() * Main.rand.Next(10, 15);
        Projectile.rotation = Projectile.velocity.ToRotation() + Pi;

    }
    public override void AI()
    {
        AggroRange = 2000f;
        #region Frames
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= Main.projFrames[Projectile.type])
        {
            Projectile.frameCounter = 0;
            Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
        }
        #endregion           

        Player player = Main.player[Projectile.owner];
        BuffPlayer buffPlayer = player.Buff();

        #region Buff
        player.AddBuff(ModContent.BuffType<ShadowHandBuff>(), 3600);
        if (player.dead)
            buffPlayer.DeepSeeker = false;
        if (buffPlayer.DeepSeeker)
            Projectile.timeLeft = 2;
        Projectile.MinionAntiClump();
        #endregion

        NPC target = Target;
        if (target == null)
            CurrentAI = AIState.NoTarget;
        else if (CurrentAI == AIState.NoTarget)
            CurrentAI = AIState.Hunting;
        switch (CurrentAI)
        {
            case AIState.Spawning:
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + (Vector2.UnitY * Main.rand.NextFloat(-16, 16)) + new Vector2(-54, 0).RotatedBy(Projectile.rotation), Main.rand.NextBool(3) ? 191 : dustStyle);
                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                if (Projectile.velocity.Length() < 2)
                {
                    CurrentAI = AIState.NoTarget;
                    aiCounter = 0;
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                }
                break;
            case AIState.NoTarget:
                Projectile.velocity += (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.5f;
                if (Projectile.velocity.Length() > 10)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10;
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2)
                    Projectile.rotation = 0 + (PiOver4 * (Projectile.velocity.Length() / 10));
                else
                    Projectile.rotation = Pi - (PiOver4 * (Projectile.velocity.Length() / 10));

                aiCounter = 0;
                break;
            case AIState.Hunting:

                #region Movement
                Vector2 homeInVector = target.Center - Projectile.Center;
                float targetDist = homeInVector.Length();
                homeInVector.Normalize();
                if (targetDist > 250f)
                {
                    float velocity = 30f;
                    Projectile.velocity = (Projectile.velocity * 40f + homeInVector * velocity) / 41f;
                }
                else
                {
                    if (targetDist < 200f)
                    {
                        float velocity = -30f;
                        Projectile.velocity = (Projectile.velocity * 40f + homeInVector * velocity) / 41f;
                    }
                    else
                        Projectile.velocity *= 0.97f;
                }
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                if (Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2)
                    Projectile.rotation = 0 + (PiOver4 * (Projectile.velocity.Length() / 10));
                else
                    Projectile.rotation = Pi - (PiOver4 * (Projectile.velocity.Length() / 10));
                #endregion

                #region Attack
                Vector2 toTarget = (target.Center - Projectile.Center);
                if (targetDist < 350f)
                {
                    if (aiCounter % 45 == 0 && aiCounter <= 140)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_OgreSpit, Projectile.Center);
                        Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * -10;
                        Projectile.rotation = Projectile.velocity.ToRotation() + Pi;
                        Projectile Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                        Bolt.hostile = false;
                        Bolt.friendly = true;
                        Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(Pi / 8), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                        Bolt.hostile = false;
                        Bolt.friendly = true;
                        Bolt = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, toTarget.SafeNormalize(Vector2.UnitX).RotatedBy(-Pi / 8), ModContent.ProjectileType<DarkBolt>(), Projectile.damage, 0f, -1, 0, 15);
                        Bolt.hostile = false;
                        Bolt.friendly = true;
                        CurrentAI = AIState.Recoil;
                    }
                    else if (aiCounter >= 180)
                    {
                        Projectile.rotation = toTarget.ToRotation();
                        if (Main.rand.NextBool() || toTarget.Length() > 600f)
                        {
                            attackBool = Projectile.position.X > target.position.X;
                            Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * -5;
                            CurrentAI = AIState.Dashing;
                        }
                        else
                        {
                            Projectile.velocity = Vector2.Zero;
                            CurrentAI = AIState.Globbing;
                        }
                        aiCounter = 0;
                    }
                    else
                        attackBool = false;
                }
                #endregion

                break;
            case AIState.Dashing:
                Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), Pi / 10);
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                if (Projectile.velocity.Length() < 2)
                {
                    SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow, Projectile.Center);
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * -30;
                    attackBool = true;
                    CurrentAI = AIState.Recoil;
                }
                break;
            case AIState.Globbing:
                Vector2 baseAngle;
                float rotation;
                if (attackBool)
                {
                    baseAngle = 0f.ToRotationVector2();
                    rotation = -Pi / 8;
                }
                else
                {
                    baseAngle = Pi.ToRotationVector2();
                    rotation = Pi / 8;
                }
                if (aiCounter % 5 == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.Center);
                    Vector2 myAngle = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * Math.Ceiling((double)aiCounter / 5));
                    for (int i = 0; i < 10; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + (myAngle * 40), myAngle.RotatedByRandom(Pi / 6) * Main.rand.NextFloat(0f, 15f), 20 * Main.rand.NextFloat(1f, 2f));
                    Projectile Glob = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), Projectile.Center, myAngle * 15, ModContent.ProjectileType<DarkGlob>(), Projectile.damage, 0f, -1, 1, 0.5f);
                    Glob.hostile = false;
                    Glob.friendly = true;
                }
                Projectile.rotation = baseAngle.SafeNormalize(Vector2.UnitX).RotatedBy(rotation * ((float)aiCounter / 5)).ToRotation();
                if (aiCounter % 30 == 0)
                {
                    aiCounter = -30;
                    CurrentAI = AIState.Hunting;
                }
                break;
            case AIState.Recoil:
                if (attackBool)
                {
                    Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), 0.05f);
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    dustStyle = Main.rand.NextBool() ? 66 : 263;
                    dust = Dust.NewDustPerfect(Projectile.Center + (Vector2.UnitY * Main.rand.NextFloat(-16, 16)) + new Vector2(-54, 0).RotatedBy(Projectile.rotation), Main.rand.NextBool(3) ? 191 : dustStyle);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                else
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.UnitX) / -2;
                if (Projectile.velocity.Length() < 2)
                {
                    CurrentAI = AIState.Hunting;
                    attackBool = false;
                    Projectile.velocity = Vector2.Zero;
                }
                break;
        }
        aiCounter++;

        EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + new Vector2(-32, 0).RotatedBy(Projectile.rotation), Vector2.UnitX.RotatedBy(Projectile.rotation) * -8, Projectile.scale * 34);
        if (Main.rand.NextBool(3))
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center + Main.rand.NextVector2Circular(2, 2) + new Vector2(-32, 0).RotatedBy(Projectile.rotation), Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.5f, 0.5f)) * -Main.rand.NextFloat(6f, 8f), Projectile.scale * Main.rand.NextFloat(30f, 40f));
        Lighting.AddLight(Projectile.Center, new Vector3(0.32f, 0.92f, 0.71f));
    }
    public override bool PreDraw(ref Color lightColor) => false;
    public void DrawSelf(Vector2 drawPosition, Color color, float rotation)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

        Rectangle frame = texture.Frame(4, Main.projFrames[Projectile.type], 0, Projectile.frame);

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (!(Projectile.rotation + Pi > Pi / 2 && Projectile.rotation + Pi < 3 * Pi / 2) && CurrentAI != AIState.Globbing)
            spriteEffects = SpriteEffects.FlipVertically;
        if (attackBool && CurrentAI == AIState.Globbing)
            spriteEffects = SpriteEffects.FlipVertically;

        Main.EntitySpriteDraw(texture, drawPosition, frame, color, rotation, frame.Size() * 0.5f, Projectile.scale, spriteEffects, 0);
    }
}



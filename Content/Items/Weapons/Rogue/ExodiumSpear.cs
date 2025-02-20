using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using Luminance.Core.Graphics;
using Windfall.Common.Graphics.Metaballs;
using static Windfall.Content.NPCs.GlobalNPCs.DebuffGlobalNPC;

namespace Windfall.Content.Items.Weapons.Rogue;
public class ExodiumSpear : RogueWeapon, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Rogue";
    public override string Texture => "Windfall/Assets/Items/Weapons/Rogue/ExodiumSpear";

    public override void SetDefaults()
    {
        Item.damage = 1;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 1f;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.maxStack = 1;
        Item.shoot = ModContent.ProjectileType<ExodiumSpearProj>();
        Item.shootSpeed = 28f;
        Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
    }
    public override float StealthVelocityMultiplier => 1.75f;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable())
        {
            int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }
        return true;
    }
}

public class ExodiumSpearProj : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "Windfall/Assets/Items/Weapons/Rogue/ExodiumSpear";

    public override void SetDefaults()
    {
        Projectile.width = 130;
        Projectile.height = 140;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Projectile.timeLeft = SecondsToFrames(90);
    }

    public override void AI()
    {
        if (Projectile.ai[0] == 0f)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + PiOver4;
        }
        //Sticky Behaviour
        Projectile.StickyProjAI(30);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => Projectile.ModifyHitNPCSticky(12);
    public override bool? CanDamage() => Projectile.ai[0] == 1f ? false : base.CanDamage();

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 v = (Projectile.rotation - PiOver4).ToRotationVector2();
        Vector2 lineStart = Projectile.Center - (v * Projectile.width * 0.5f);
        Vector2 lineEnd = Projectile.Center + (v * Projectile.width * 0.5f);
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 24, ref collisionPoint);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, spriteEffects);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 16; i++)
        {
            Vector2 spawnPos = Projectile.Center - ((Projectile.rotation - PiOver4).ToRotationVector2() * Main.rand.NextFloat(-64f, 64f));
            if (!Main.tile[spawnPos.ToTileCoordinates()].IsTileSolid())
                EmpyreanMetaball.SpawnDefaultParticle(spawnPos, Main.rand.NextVector2Circular(2f, 2f), Main.rand.NextFloat(10f, 20f));
        }
    }
    private readonly List<int> DebuffTypes =
    [
        BuffID.Daybreak,
        ModContent.BuffType<Nightwither>(),
        ModContent.BuffType<BrimstoneFlames>(),
        ModContent.BuffType<AstralInfectionDebuff>(),
        ModContent.BuffType<HolyFlames>(),
        ModContent.BuffType<Plague>(),
        BuffID.OnFire3,
        ModContent.BuffType<SagePoison>(),
        ModContent.BuffType<ElementalMix>(),

    ];
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.Calamity().stealthStrike)
        {
            int damage = 370;
            foreach (Projectile spear in Main.projectile.Where(p => p.active && p.ai[0] == 1f && (p.type == ModContent.ProjectileType<ExodiumSpearProj>())))
            {
                NPC impaledNPC = Main.npc[(int)spear.ai[1]];
                if (CalamityUtils.IsAnEnemy(impaledNPC, false))
                {
                    for (int i = 0; i < impaledNPC.buffType.Length; i++)
                    {
                        if (DebuffTypes.Contains(impaledNPC.buffType[i]))
                        {
                            int index = DebuffTypes.IndexOf(impaledNPC.buffType[i]);
                            switch (index)
                            {
                                case 0: //Daybreak
                                    damage += (int)(100 * (impaledNPC.buffTime[i] / 60D) * VanillaHeatDamageMult(target));
                                    break;
                                case 1: //Nightwither
                                    damage += (int)(100 * (impaledNPC.buffTime[i] / 60D) * ColdDamageMult(target));
                                    break;
                                case 2: //Brimstone Flames
                                    damage += (int)(30 * (impaledNPC.buffTime[i] / 60D) * HeatDamageMult(target));
                                    break;
                                case 3: //Astral Infection
                                    damage += (int)(37.5 * (impaledNPC.buffTime[i] / 60D) * SicknessDamageMult(target));
                                    break;
                                case 4: //Holy Flames
                                    damage += (int)(100 * (impaledNPC.buffTime[i] / 60D) * HeatDamageMult(target));
                                    break;
                                case 5: //Plague
                                    damage += (int)(50 * (impaledNPC.buffTime[i] / 60D) * SicknessDamageMult(target));
                                    break;
                                case 6: //Hellfire
                                    damage += (int)(15 * (impaledNPC.buffTime[i] / 60D) * VanillaHeatDamageMult(target));
                                    break;
                                case 7: //Sage Poison
                                    damage += (int)(impaledNPC.Calamity().sagePoisonDamage * (impaledNPC.buffTime[i] / 60D) * SicknessDamageMult(target));
                                    break;
                                case 8: //Elemental Mix
                                    damage += (int)(200 * (impaledNPC.buffTime[i] / 60D));
                                    break;
                            }
                            target.DelBuff(target.buffType.First(i => i == DebuffTypes[index]));
                        }
                    }
                    damage += (int)(spear.localAI[0] / 60 * 25);
                }
                spear.Kill();
                spear.active = false;
            }

            target.StrikeNPC(target.CalculateHitInfo(damage, hit.HitDirection, true, hit.Knockback * 2, Projectile.DamageType));

            SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, Projectile.Center);
            ScreenShakeSystem.StartShake(5f);
            for (int i = 0; i <= 50; i++)
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
            CalamityMod.Particles.Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
            GeneralParticleHandler.SpawnParticle(pulse);
            CalamityMod.Particles.Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
            GeneralParticleHandler.SpawnParticle(explosion);
            Projectile.active = false;
        }
        else
        {
            if (target.type == NPCID.CultistBoss)
                target.buffImmune[BuffID.Frostburn2] = false;
            target.AddBuff(BuffID.Frostburn2, 1800);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Frostburn2, 1800);
}

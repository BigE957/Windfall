using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Terraria;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
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
        Projectile.timeLeft = 5400;
    }

    public override void AI()
    {
        if (base.Projectile.ai[0] == 0f)
        {
            base.Projectile.rotation = base.Projectile.velocity.ToRotation() + PiOver4;
        }

        //Sticky Behaviour
        if (Projectile.ai[0] == 1f)
        {
            int seconds = 30;
            bool killProj = false;
            bool spawnDust = false;

            //the projectile follows the NPC, even if it goes into blocks
            Projectile.tileCollide = false;

            //timer for triggering hit effects
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] % 30f == 0f)
            {
                spawnDust = true;
            }

            //So AI knows what NPC it is sticking to
            int npcIndex = (int)Projectile.ai[1];
            NPC npc = Main.npc[npcIndex];

            //Kill projectile after so many seconds or if the NPC it is stuck to no longer exists
            if (Projectile.localAI[0] >= (float)(60 * seconds))
            {
                killProj = true;
            }
            else if (!npcIndex.WithinBounds(Main.maxNPCs))
            {
                killProj = true;
            }

            else if (npc.active && !npc.dontTakeDamage)
            {
                //follow the NPC
                Projectile.Center = npc.Center - Projectile.velocity * 2f;
                Projectile.gfxOffY = npc.gfxOffY;

                //if attached to npc, trigger npc hit effects every half a second
                if (spawnDust)
                {
                    npc.HitEffect(0, 1.0);
                }
            }
            else
            {
                killProj = true;
            }

            //Kill the projectile or reset stats if needed
            if (killProj)
                Projectile.Kill();
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        Player player = Main.player[Projectile.owner];
        Rectangle myRect = Projectile.Hitbox;

        if (Projectile.owner == Main.myPlayer)
        {
            for (int npcIndex = 0; npcIndex < Main.maxNPCs; npcIndex++)
            {
                NPC npc = Main.npc[npcIndex];
                //covers most edge cases like voodoo dolls
                if (npc.active && !npc.dontTakeDamage &&
                    ((Projectile.friendly && (!npc.friendly || (npc.type == NPCID.Guide && Projectile.owner < Main.maxPlayers && player.killGuide) || (npc.type == NPCID.Clothier && Projectile.owner < Main.maxPlayers && player.killClothier))) ||
                    (Projectile.hostile && npc.friendly && !npc.dontTakeDamageFromHostiles)) && (Projectile.owner < 0 || npc.immune[Projectile.owner] == 0 || Projectile.maxPenetrate == 1))
                {
                    if (npc.noTileCollide || !Projectile.ownerHitCheck)
                    {
                        bool stickingToNPC;
                        //Solar Crawltipede tail has special collision
                        if (npc.type == NPCID.SolarCrawltipedeTail)
                        {
                            Rectangle rect = npc.Hitbox;
                            int crawltipedeHitboxMod = 8;
                            rect.X -= crawltipedeHitboxMod;
                            rect.Y -= crawltipedeHitboxMod;
                            rect.Width += crawltipedeHitboxMod * 2;
                            rect.Height += crawltipedeHitboxMod * 2;
                            stickingToNPC = Projectile.Colliding(myRect, rect);
                        }
                        else
                        {
                            stickingToNPC = Projectile.Colliding(myRect, npc.Hitbox);
                        }
                        if (stickingToNPC)
                        {
                            //reflect projectile if the npc can reflect it (like Selenians)
                            if (npc.reflectsProjectiles && Projectile.CanBeReflected())
                            {
                                npc.ReflectProjectile(Projectile);
                                return;
                            }

                            //let the projectile know it is sticking and the npc it is sticking too
                            Projectile.ai[0] = 1f;
                            Projectile.ai[1] = (float)npcIndex;

                            //follow the NPC
                            Projectile.velocity = (npc.Center - Projectile.Center);

                            Projectile.netUpdate = true;

                            //Count how many projectiles are attached, delete as necessary
                            Point[] array2 = new Point[12];
                            int projCount = 0;
                            for (int projIndex = 0; projIndex < Main.maxProjectiles; projIndex++)
                            {
                                Projectile proj = Main.projectile[projIndex];
                                if (projIndex != Projectile.whoAmI && proj.active && proj.owner == Main.myPlayer && proj.type == Projectile.type && proj.ai[0] == 1f && proj.ai[1] == (float)npcIndex)
                                {
                                    array2[projCount++] = new Point(projIndex, proj.timeLeft);
                                    if (projCount >= array2.Length)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (projCount >= array2.Length)
                            {
                                int stuckProjAmt = 0;
                                for (int m = 1; m < array2.Length; m++)
                                {
                                    if (array2[m].Y < array2[stuckProjAmt].Y)
                                    {
                                        stuckProjAmt = m;
                                    }
                                }
                                Main.projectile[array2[stuckProjAmt].X].Kill();
                            }
                        }
                    }
                }
            }
        }
    }

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
                                    damage += (int)( (impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().HeatDebuffMultiplier.ApplyTo(100));
                                    break;
                                case 1: //Nightwither
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().ColdDebuffMultiplier.ApplyTo(100));
                                    break;
                                case 2: //Brimstone Flames
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().HeatDebuffMultiplier.ApplyTo(30));
                                    break;
                                case 3: //Astral Infection
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().SicknessDebuffMultiplier.ApplyTo(37.5f));
                                    break;
                                case 4: //Holy Flames
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().HeatDebuffMultiplier.ApplyTo(100));
                                    break;
                                case 5: //Plague
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().SicknessDebuffMultiplier.ApplyTo(50));
                                    break;
                                case 6: //Hellfire
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().HeatDebuffMultiplier.ApplyTo(15));
                                    break;
                                case 7: //Sage Poison
                                    damage += (int)((impaledNPC.buffTime[i] / 60D) * impaledNPC.Calamity().SicknessDebuffMultiplier.ApplyTo(impaledNPC.Calamity().sagePoisonDamage));
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
            CameraSystem.StartScreenShake(Projectile.Center, Vector2.Zero, 5f, 10, 60);
            for (int i = 0; i <= 50; i++)
                EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f) * Main.rand.NextFloat(1f, 2f), 40 * Main.rand.NextFloat(3f, 5f));
            Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Color.Teal, 0f, 2.5f, 16);
            GeneralParticleHandler.SpawnParticle(pulse);
            Particle explosion = new DetailedExplosion(Projectile.Center, Vector2.Zero, new(117, 255, 159), new Vector2(1f, 1f), 0f, 0f, 1f, 16);
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

using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.NormalNPCs;
using Terraria.ModLoader.IO;
using Windfall.Common.Systems;
using Windfall.Content.Buffs.StatBuffs;
using Windfall.Content.NPCs.PlayerNPCs;
using Windfall.Content.Projectiles.GodlyAbilities;

namespace Windfall.Common.Players
{
    public class GodlyPlayer : ModPlayer
    {
        public bool Evil1Essence = false;
        public bool Evil2Essence = false;
        public bool SlimeGodEssence = false;
        public int Ambrosia = 0;
        public override void LoadData(TagCompound tag)
        {
            Evil1Essence = tag.GetBool("Evil1Essence");
            Evil2Essence = tag.GetBool("Evil2Essence");
            SlimeGodEssence = tag.GetBool("SlimeGodEssence");
            Ambrosia = tag.GetInt("Ambrosia");
        }
        public override void SaveData(TagCompound tag)
        {
            if(Evil1Essence)
                tag["Evil1Essence"] = Evil1Essence;
            if(Evil2Essence)
                tag["Evil2Essence"] = Evil2Essence;
            if (SlimeGodEssence)
                tag["SlimeGodEssence"] = SlimeGodEssence;
            if(Ambrosia != 0)
                tag["Ambrosia"] = Ambrosia;
        }

        public static readonly SoundStyle IchorGoopyHit = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveIchorShoot");
        public static readonly SoundStyle SlimeGodShot = new("CalamityMod/Sounds/Custom/SlimeGodShot", 2);

        private int abilityCounter = 0;
        public int activeAbility = 0;
        private int OldAmbrosia = 0;
        private enum AbilityIDS
        {
            Dash = 1,
            Harvest,
            Attack1,
        }
        private Vector2 ancientVelocity = Vector2.Zero;
        private Vector2 olderVelocity = Vector2.Zero;
        private NPC target = null;
        public struct Pair(Dust d, NPC owner)
        {
            public Dust Dust = d;
            public NPC Owner = owner;
        }
        private static List<Pair> dustArray = [];
        private static List<NPC> harvestNPCArray = [];
        private static List<int> harvestCounterArray = [];
        private static List<NPC> harvestNPCBlacklist = [];

        public override void UpdateDead()
        {            
            activeAbility = 0;
            abilityCounter = 0;
            dustArray = [];
            Ambrosia = 0;
            ambrosiaCounter = 0;
            muckCounter = 0;
        }
        private int ambrosiaCounter = 0;
        private int muckCounter = 0;
        public override void PreUpdate()
        {
            #region Ambrosia           
            if (AnyGodlyEssence(Player))
            {
                if (activeAbility == 0)
                {
                    ambrosiaCounter++;
                    if (ambrosiaCounter >= 120)
                    {
                        Ambrosia++;
                        ambrosiaCounter = 0;
                    }
                }
                if (Ambrosia > 100)
                    Ambrosia = 100;
                if (Ambrosia != OldAmbrosia)
                {
                    if (Evil1Essence && !WorldGen.crimson && OldAmbrosia > Ambrosia && activeAbility != 0)
                        for(int i = 0; i < (OldAmbrosia - Ambrosia) / 2f; i++)
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 5, ProjectileID.TinyEater, (int)Player.GetDamage(DamageClass.Generic).ApplyTo(25f), 0f, Player.whoAmI);
                    DisplayLocalizedText($"Ambrosia: {Ambrosia}");
                    OldAmbrosia = Ambrosia;                   
                }
            }
            #endregion
            #region Active Ability Player Effects
            if (activeAbility != 0)
            {
                switch (activeAbility)
                {
                    case (int)AbilityIDS.Dash:
                        if(WorldGen.crimson)
                            Player.maxFallSpeed = 32;
                        else
                        {
                            Player.gravity = 0.75f;
                            Player.maxFallSpeed = 20;
                        }
                        break;
                    case (int)AbilityIDS.Attack1:
                        Player.gravity = 0.5f;
                        Player.maxFallSpeed = 5;
                        break;
                }
            }
            #endregion
            #region Passive Ability Effects
            if(Evil1Essence && WorldGen.crimson)
            {
                int neededCreepers = (int)Math.Floor(Ambrosia / 20f);
                int creeperCount = Main.projectile.Where(p => p != null && p.active && p.type == ModContent.ProjectileType<GodlyCreeper>() && p.owner == Player.whoAmI).Count();
                if(creeperCount < neededCreepers)
                {
                    while(creeperCount < neededCreepers)
                    {
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), Player.Center, Main.rand.NextVector2CircularEdge(10f, 10f), ModContent.ProjectileType<GodlyCreeper>(), (int)Player.GetDamage(DamageClass.Generic).ApplyTo(10), 0, Player.whoAmI);
                        creeperCount++;
                        if (proj.ModProjectile is GodlyCreeper creep)
                            creep.ambrosiaRequirement = creeperCount * 20;
                    }
                }
            }
            if(SlimeGodEssence)
            {
                if(Main.npc.Where(n => n != null && n.active && n.IsAnEnemy() && n.Distance(Player.Center) < 800).Any() && Ambrosia < 100)
                {
                    muckCounter++;
                    if(muckCounter % 120 == 0)
                    {
                        Vector2 spawnLocation = Vector2.Zero;
                        for (int i = 0; i < 1000; i++)
                        {
                            spawnLocation = Player.Center + (Main.rand.NextVector2Circular(600, 600));
                            Point tileLoation = spawnLocation.ToTileCoordinates();
                            if (!Main.tile[tileLoation].IsTileSolid())
                                break;
                            else
                                spawnLocation = Player.Center - new Vector2(Main.rand.NextFloat(-500, 500), Main.rand.NextFloat(100, 500));
                        }
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), spawnLocation, Vector2.Zero, Main.rand.NextBool() ? ModContent.ProjectileType<GodlyCrimulanMuck>() : ModContent.ProjectileType<GodlyEbonianMuck>(), 0, 0f, Player.whoAmI);
                    }
                }
            }
            #endregion           
        }
        public override void PostUpdate()
        {
            #region Visual Effect Updates
            if (dustArray.Count > 0)
            {
                dustArray.RemoveAll(dp => dp.Dust == null || !dp.Dust.active || dp.Dust.type != DustID.Ichor || dp.Owner == null || !dp.Owner.active);
                foreach (Pair dp in dustArray)
                {
                    Dust d = dp.Dust;
                    NPC npc = dp.Owner;
                    if ((dp.Dust.position - Player.position).Length() <= 32)
                        dp.Dust.active = false;
                    else
                        d.velocity = (Player.Center - d.position).SafeNormalize(Vector2.Zero) * 12;
                }
            }
            #endregion
            #region Ability Activation
            if (activeAbility == 0)
            {
                abilityCounter = 0;

                if (WindfallKeybinds.GodlyDashHotkey.JustPressed && ((Evil1Essence && !WorldGen.crimson) || (Evil2Essence && WorldGen.crimson)))
                {
                    if (WorldGen.crimson)
                    {
                        if (Ambrosia >= 20 && Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage && (Player.Center - n.Center).LengthSquared() <= 810000).Any())
                        {
                            activeAbility = (int)AbilityIDS.Dash;
                            Ambrosia -= 20;
                        }
                    }
                    else if (Ambrosia >= 15)
                    {
                        activeAbility = (int)AbilityIDS.Dash;
                        Ambrosia -= 15;
                    }
                }
                else if (WindfallKeybinds.GodlyHarvestHotkey.JustPressed && ((Evil1Essence && WorldGen.crimson) || (Evil2Essence && !WorldGen.crimson)))
                {
                    if (!WorldGen.crimson)
                    {
                        if(Ambrosia >= 10)
                            Ambrosia -= 10;
                        else
                            return;
                    }
                    activeAbility = (int)AbilityIDS.Harvest;                  
                }
                else if (WindfallKeybinds.GodlyAttack1Hotkey.JustPressed && SlimeGodEssence && Ambrosia >= 20)
                {
                    activeAbility = (int)AbilityIDS.Attack1;
                    Ambrosia -= 20;
                }                             
            }
            #endregion
            #region Active Ability Effects
            else
            {
                switch (activeAbility)
                {
                    case (int)AbilityIDS.Dash:
                        #region Perforator Essence
                        if (WorldGen.crimson)
                        {
                            if (abilityCounter == 0)
                            {
                                #region Target Selection
                                target = null;
                                foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage))
                                {
                                    if (target == null)
                                        target = npc;
                                    else if ((Main.MouseWorld - npc.Center).LengthSquared() <= (Main.MouseWorld - target.Center).LengthSquared())
                                        target = npc;
                                }
                                if (target == null || (Player.Center - target.Center).LengthSquared() > 810000)
                                {
                                    foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage))
                                    {
                                        if (target == null)
                                            target = npc;
                                        else if ((Player.Center - npc.Center).LengthSquared() <= (Player.Center - target.Center).LengthSquared())
                                            target = npc;
                                    }
                                    if (target == null || (Player.Center - target.Center).LengthSquared() > 810000)
                                    {
                                        activeAbility = 0;
                                        Ambrosia += 20;
                                        break;
                                    }
                                }
                                #endregion
                                SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton);
                                ancientVelocity = (target.Center - Player.Center).SafeNormalize(Vector2.Zero);
                                Player.GiveUniversalIFrames(25);
                            }                         
                            olderVelocity = Player.velocity;

                            if (abilityCounter > 8)
                            {
                                if (Math.Abs((target.Center - Player.Center).ToRotation() - Player.velocity.ToRotation()) < 1f)
                                    Player.velocity = (target.Center - Player.Center).SafeNormalize(Vector2.Zero) * 32;
                                else
                                    Player.velocity = ancientVelocity * 32;
                            }
                            else
                                ancientVelocity = (target.Center - Player.Center).SafeNormalize(Vector2.Zero);
                            for (int i = 0; i < 3; i++)
                            {
                                Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(40f, 40f), DustID.Blood, Vector2.Zero, Scale: 2f);
                                d.noGravity = true;
                            }
                            Dust p = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(40f, 40f), DustID.Ichor, Vector2.Zero, Scale: 1.5f);
                            p.noGravity = true;
                            if (Player.Hitbox.Intersects(target.Hitbox) || abilityCounter > 24)
                            {
                                activeAbility = 0;
                                Player.velocity = olderVelocity.SafeNormalize(Vector2.Zero) * 16;
                                if (Player.Hitbox.Intersects(target.Hitbox))
                                {
                                    var modifiers = new NPC.HitModifiers();
                                    NPC.HitInfo hit = modifiers.ToHitInfo((int)Player.GetDamage(DamageClass.Generic).ApplyTo(100f), false, 0f, true);
                                    target.StrikeNPC(hit);
                                    target.AddBuff(ModContent.BuffType<BurningBlood>(), 120);
                                    target.AddBuff(BuffID.Ichor, 240);
                                    Player.velocity.Y = -10;
                                    Player.wingTime += Player.wingTimeMax / 10;
                                    SoundEngine.PlaySound(IchorGoopyHit);
                                    for (int i = 0; i < 50; i++)
                                    {
                                        Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                                        Dust d = Dust.NewDustPerfect(Player.Center, DustID.Ichor, speed * 5, Scale: 2f);
                                        d.noGravity = true;
                                        Dust b = Dust.NewDustPerfect(Player.Center, DustID.Blood, speed * 5, Scale: 2.5f);
                                        b.noGravity = true;
                                    }
                                }
                                target = null;
                            }
                        }
                        #endregion
                        #region Eater of Worlds Essence
                        else
                        {
                            if (abilityCounter == 0)
                                Player.velocity = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 15;
                            Player.wingTime = 0;

                            Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(40f, 40f), DustID.Corruption, Vector2.Zero, Scale: 1.5f);
                            d.noGravity = true;

                            foreach(NPC npc in Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<GodlyTumor>()))
                            {
                                if(Player.Hitbox.Intersects(npc.Hitbox))
                                {
                                    EoWSlam(npc.Bottom.Y, npc);                                  
                                    npc.StrikeInstantKill();
                                    Player.Godly().Ambrosia += 5;
                                    Player.AddBuff(ModContent.BuffType<WretchedHarvest>(), 240);
                                }
                            }

                            if (Player.oldVelocity.Y == Player.velocity.Y)
                            {
                                EoWSlam(Player.Bottom.Y);
                            }
                            else
                            {
                                ancientVelocity = olderVelocity;
                                olderVelocity = Player.velocity;                               
                            }
                        }
                        #endregion
                        break;
                    case (int)AbilityIDS.Harvest:
                        #region Brain of Cthulhu Essence
                        if (WorldGen.crimson)
                        {
                            if(Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage && (n.Center - Player.Center).Length() <= 500 && !harvestNPCArray.Contains(n) && !harvestNPCBlacklist.Contains(n)).Any())
                                while(harvestNPCArray.Count < 5 && Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage && (n.Center - Player.Center).Length() <= 500 && !harvestNPCArray.Contains(n) && !harvestNPCBlacklist.Contains(n) && harvestNPCArray.Count <= 5).Any())
                                {
                                    List<NPC> arr = Main.npc.Where(n => n.active && !n.friendly && !n.dontTakeDamage && (n.Center - Player.Center).Length() <= 500 && !harvestNPCArray.Contains(n) && !harvestNPCBlacklist.Contains(n) && harvestNPCArray.Count <= 5).ToList();
                                    NPC npc = null;
                                    foreach(NPC n in arr)
                                    {
                                        if (npc == null)
                                            npc = n;
                                        else
                                            if((n.Center - Main.MouseWorld).LengthSquared() < (npc.Center - Main.MouseWorld).LengthSquared())
                                                npc = n;
                                    }
                                    harvestNPCArray.Add(npc);
                                    harvestCounterArray.Add(0);
                                }
                            for (int i = 0;  i < harvestNPCArray.Count; i++)
                            {
                                if (!harvestNPCArray[i].active || (harvestNPCArray[i].Center - Player.Center).Length() > 500)
                                {
                                    harvestNPCArray[i] = null;
                                    harvestCounterArray[i] = -10;
                                }
                                else
                                {
                                    NPC npc = harvestNPCArray[i];
                                    if (harvestCounterArray[i] >= 120)
                                    {
                                        var modifiers = new NPC.HitModifiers();
                                        NPC.HitInfo hit = modifiers.ToHitInfo((int)Player.GetDamage(DamageClass.Generic).ApplyTo(100f), false, 0f, true);
                                        npc.StrikeNPC(hit);
                                        npc.AddBuff(BuffID.Ichor, 240);
                                        npc.AddBuff(BuffID.Confused, 240);
                                        SoundEngine.PlaySound(IchorGoopyHit, npc.Center);
                                        for (int j = 0; j < 50; j++)
                                        {
                                            Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                                            Dust d = Dust.NewDustPerfect(npc.Center, DustID.Ichor, speed * 5, Scale: 2f);
                                            d.noGravity = true;
                                            Dust b = Dust.NewDustPerfect(npc.Center, DustID.Blood, speed * 5, Scale: 2.5f);
                                            b.noGravity = true;
                                        }
                                        if(npc.boss)
                                            Player.Heal(5);
                                        else if(!npc.SpawnedFromStatue && npc.type != ModContent.NPCType<SuperDummyNPC>())
                                            Player.Heal(1);
                                        harvestNPCBlacklist.Add(npc);
                                        harvestCounterArray[i] = -10;
                                        i--;
                                    }
                                    else
                                    {                                       
                                        if (abilityCounter % 6 == 0)
                                        {
                                            var modifiers = new NPC.HitModifiers();
                                            NPC.HitInfo hit = modifiers.ToHitInfo(1, false, 0f);
                                            Ambrosia++;
                                            npc.StrikeNPC(hit);
                                        }
                                        Vector2 speed = (Player.Center - npc.Center).SafeNormalize(Vector2.Zero);
                                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.Ichor, speed * 12, Scale: 2f);
                                        d.noGravity = true;
                                        dustArray.Add(new Pair(d, npc));
                                        harvestCounterArray[i]++;
                                    }
                                }
                            }
                            harvestNPCArray.RemoveAll(n => harvestNPCBlacklist.Contains(n) || n == null);
                            harvestCounterArray.RemoveAll(i => i < 0);
                            if (WindfallKeybinds.GodlyHarvestHotkey.JustReleased)
                            {
                                harvestNPCBlacklist = [];
                                harvestNPCArray = [];
                                harvestCounterArray = [];
                                activeAbility = 0;
                                break;
                            }
                        }
                        #endregion
                        #region Hive Mind Essence
                        else
                        {
                            NPC npc = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, ModContent.NPCType<GodlyTumor>());
                            if (npc.ModNPC is GodlyTumor tumor)
                                tumor.Owner = Player;
                            activeAbility = 0;
                        }
                        #endregion
                        break;
                    case (int)AbilityIDS.Attack1:
                        #region Slime God Essence
                        if (abilityCounter % 5 == 0)
                        {
                            SoundEngine.PlaySound(SlimeGodShot);
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(Pi/10) * 15, Main.rand.NextBool() ? ModContent.ProjectileType<GodlyEbonianGlob>() : ModContent.ProjectileType<GodlyCrimulanGlob>(), (int)Player.GetDamage(DamageClass.Generic).ApplyTo(10f), 0.5f);
                            Player.velocity -= (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero);
                            if(Player.velocity.LengthSquared() >= 400)
                                Player.velocity = Player.velocity.SafeNormalize(Vector2.Zero) * 20;
                        }
                        if (abilityCounter > 50)
                            activeAbility = 0;
                        #endregion
                        break;
                }
                abilityCounter++;
            }
            #endregion
        }
        public override void PostUpdateMiscEffects()
        {
            if (Evil1Essence)
            {
                if(WorldGen.crimson)
                    Player.endurance += 0.1f * (Ambrosia / 100);
                else
                    Player.GetDamage<GenericDamageClass>() += 0.1f * (Ambrosia / 100);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Evil2Essence)
            {
                if (!WorldGen.crimson)
                {
                    if (Player.HasBuff<WretchedHarvest>())
                    {
                        if (target.HasBuff<BrainRot>())
                            target.AddBuff(BuffID.CursedInferno, 120);
                        else
                            target.AddBuff(ModContent.BuffType<BrainRot>(), 120);
                    }
                    if (Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<GodlyBlob>() && p.owner == Player.whoAmI).Count() <= 5 && Main.rand.NextBool(25))
                        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, Main.rand.NextVector2CircularEdge(10f, 10f), ModContent.ProjectileType<GodlyBlob>(), 10, 0, Player.whoAmI);
                }
                else
                {
                    int longestSide = target.Hitbox.Width > target.Hitbox.Height ? target.Hitbox.Width : target.Hitbox.Height;
                    if(Vector2.Distance(target.Center, Player.Center) - longestSide < 64 || hit.DamageType is TrueMeleeDamageClass)
                        Ambrosia++;
                }
            }
        }
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if(Evil1Essence && !WorldGen.crimson)
            {
                for (int i = 0; i < Ambrosia / 2f; i++)
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, Main.rand.NextVector2CircularEdge(5, 5), ProjectileID.TinyEater, 25, 0f, Player.whoAmI);
                Ambrosia += 10;
            }
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (Evil1Essence && !WorldGen.crimson)
            {
                for (int i = 0; i < Ambrosia / 2f; i++)
                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, Main.rand.NextVector2CircularEdge(5, 5), ProjectileID.TinyEater, 25, 0f, Player.whoAmI);
                Ambrosia += 10;
            }
        }
        public override bool CanStartExtraJump(ExtraJump jump)
        {
            if (activeAbility == (int)AbilityIDS.Dash || activeAbility == (int)AbilityIDS.Attack1)
                return false;
            else
                return true;
        }
        private void EoWSlam(float y, NPC tumor = null)
        {
            for (int i = 0; i < 4; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X + (32 * i), y), new Vector2(4 + (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, (int)Player.GetDamage(DamageClass.Generic).ApplyTo(25f), 0f);
                Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X - (32 * i), y), new Vector2(-4 - (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, (int)Player.GetDamage(DamageClass.Generic).ApplyTo(25f), 0f);
            }

            for (int i = 0; i < 50; i++)
            {
                Dust d = Dust.NewDustPerfect(new Vector2(Player.Center.X, y) + new Vector2(Main.rand.NextFloat(-125f, 125f), 0f), DustID.Corruption, new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-15f, 0f)), Scale: 1.5f);
                d.noGravity = true;
            }
            
            if (Math.Abs(ancientVelocity.Y) >= 5)
                Player.velocity.Y = -ancientVelocity.Y;
            else
                Player.velocity.Y = -5;
            Player.velocity /= 2;
            if (tumor != null) //Determines if this is a Normal Slam or a Hive Tumor Slam
            {
                Player.velocity = Player.velocity.SafeNormalize(Vector2.Zero) * 15;

                for (int i = 1; i < 3; i++)
                {
                    Particle boomRing = new DirectionalPulseRing(tumor.Center, Vector2.Zero, Color.MediumPurple, Vector2.One, 0f, 0.14f * i, 1.43f * i, 20);
                    GeneralParticleHandler.SpawnParticle(boomRing);
                }
                
                for (int i = 0; i < 30; i++)
                {
                    bool randomDust = Main.rand.NextBool();
                    Dust boomDust = Dust.NewDustPerfect(tumor.Center, randomDust ? DustID.ShadowbeamStaff : DustID.DemonTorch, Main.rand.NextVector2Circular(30f, 30f), Scale: randomDust ? Main.rand.NextFloat(1f, 2f) : Main.rand.NextFloat(2.5f, 3f));
                    boomDust.noGravity = true;
                    boomDust.noLight = true;
                    boomDust.noLightEmittence = true;
                }
                
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/HiveMindRoarFast") { PitchVariance = 0.2f }, tumor.Center);

                foreach (NPC npc in Main.npc.Where(n => n != null && n.active && !n.friendly && !n.dontTakeDamage && Vector2.Distance(tumor.Center, n.Center) < 300))
                {
                    var modifiers = new NPC.HitModifiers();
                    NPC.HitInfo hit = modifiers.ToHitInfo((int)Player.GetDamage(DamageClass.Generic).ApplyTo(100f), false, npc.boss ? 0f : 20f);
                    npc.StrikeNPC(hit);
                    if (npc.knockBackResist != 0 && npc.boss == false)
                    {
                        npc.velocity = (npc.Center - tumor.Center).SafeNormalize(Vector2.Zero) * npc.velocity.Length();
                        npc.velocity.Y -= 5;
                    }
                    npc.AddBuff(ModContent.BuffType<BrainRot>(), 120);
                    npc.AddBuff(BuffID.CursedInferno, 240);
                }
            }
            foreach (NPC npc in Main.npc.Where(n => n != null && n.active && Vector2.Distance(Player.Center, n.Center) < 300 && n.velocity.Y == 0))
            {
                if (npc.knockBackResist != 0 && npc.boss == false)
                {
                    if (tumor != null)
                        npc.velocity.Y -= 20 * npc.knockBackResist;                       
                    else
                        npc.velocity.Y -= 10 * npc.knockBackResist;
                    npc.velocity.X = 0;
                }
                npc.AddBuff(ModContent.BuffType<ArmorCrunch>(), 480);
            }
            Player.wingTime = Player.wingTimeMax;
            activeAbility = 0;
        }
        public static bool AnyGodlyEssence(Player player) => player.Godly().Evil1Essence || player.Godly().Evil2Essence || player.Godly().SlimeGodEssence;
        public static int GodlyEssenceCount(Player player)
        {
            int count = 0;
            if (player.Godly().Evil1Essence)
                count++;
            if (player.Godly().Evil2Essence)
                count++;
            if (player.Godly().SlimeGodEssence)
                count++;
            return count;
        }
        public static bool IsUsingAbility(Player player) => player.Godly().activeAbility != 0;
    }
}

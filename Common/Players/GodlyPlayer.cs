using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Magic;
using Terraria.ModLoader.IO;
using Windfall.Common.Systems;

namespace Windfall.Common.Players
{
    public class GodlyPlayer : ModPlayer
    {
        public bool Evil1Essence = false;
        public bool Evil2Essence = false;
        public bool SlimeGodEssence = false;
        public override void LoadData(TagCompound tag)
        {
            Evil1Essence = tag.GetBool("Evil1Essence");
            Evil2Essence = tag.GetBool("Evil2Essence");
            SlimeGodEssence = tag.GetBool("SlimeGodEssence");
        }
        public override void SaveData(TagCompound tag)
        {
            if(Evil1Essence)
                tag["Evil1Essence"] = Evil1Essence;
            if(Evil2Essence)
                tag["Evil2Essence"] = Evil2Essence;
            if (SlimeGodEssence)
                tag["SlimeGodEssence"] = SlimeGodEssence;
        }

        public static readonly SoundStyle PerforatorDashHit = new("CalamityMod/Sounds/Custom/Perforator/PerfHiveIchorShoot");
        public static readonly SoundStyle SlimeGodShot = new("CalamityMod/Sounds/Custom/SlimeGodShot", 2);

        private int abilityCounter = 0;
        public int activeAbility = 0;
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
            dustArray = new List<Pair>();            
        }
        public override void PreUpdate()
        {
            if(activeAbility != 0)
            {
                switch (activeAbility)
                {
                    case (int)AbilityIDS.Dash:
                        if(WorldGen.crimson)
                            Player.maxFallSpeed = 32;
                        break;
                    case (int)AbilityIDS.Attack1:
                        Player.gravity = 0.5f;
                        Player.maxFallSpeed = 5;
                        break;
                }
            }
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
            if (activeAbility == 0)
            {
                if (WindfallKeybinds.GodlyDashHotkey.JustPressed && ((Evil1Essence && !WorldGen.crimson) || (Evil2Essence && WorldGen.crimson)))
                    activeAbility = (int)AbilityIDS.Dash;
                else if (WindfallKeybinds.GodlyHarvestHotkey.JustPressed && ((Evil1Essence && WorldGen.crimson) || (Evil2Essence && !WorldGen.crimson)))
                    activeAbility = (int)AbilityIDS.Harvest;
                else if (WindfallKeybinds.GodlyAttack1Hotkey.JustPressed && SlimeGodEssence)
                    activeAbility = (int)AbilityIDS.Attack1;
                
                abilityCounter = 0;
            }
            else
            {
                switch (activeAbility)
                {
                    case (int)AbilityIDS.Dash:
                        #region Perforator Essence
                        if (WorldGen.crimson)
                        {
                            Player.maxFallSpeed = 32;
                            if (abilityCounter == 0)
                            {
                                target = null;
                                foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly))
                                {
                                    if (target == null)
                                        target = npc;
                                    else if ((Main.MouseWorld - npc.Center).LengthSquared() <= (Main.MouseWorld - target.Center).LengthSquared())
                                        target = npc;
                                }
                                if (target == null || (Player.Center - target.Center).LengthSquared() > 810000)
                                {
                                    foreach (NPC npc in Main.npc.Where(n => n.active && !n.friendly))
                                    {
                                        if (target == null)
                                            target = npc;
                                        else if ((Player.Center - npc.Center).LengthSquared() <= (Player.Center - target.Center).LengthSquared())
                                            target = npc;
                                    }
                                    if (target == null || (Player.Center - target.Center).LengthSquared() > 810000)
                                    {
                                        activeAbility = 0;
                                        break;
                                    }
                                }
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
                                    NPC.HitInfo hit = modifiers.ToHitInfo(100, false, 0f);
                                    target.StrikeNPC(hit);
                                    target.AddBuff(ModContent.BuffType<BurningBlood>(), 120);
                                    target.AddBuff(BuffID.Ichor, 240);
                                    Player.velocity.Y = -10;
                                    Player.wingTime += Player.wingTimeMax / 10;
                                    SoundEngine.PlaySound(PerforatorDashHit);
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
                            {
                                Player.velocity += (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10;
                                if (Player.velocity.LengthSquared() <= 100)
                                    Player.velocity = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10;
                            }
                            Player.wingTime = 0;

                            Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(40f, 40f), DustID.Corruption, Vector2.Zero, Scale: 1.5f);
                            d.noGravity = true;

                            if (Player.oldVelocity.Y == Player.velocity.Y)
                            {
                                
                                for (int i = 0; i < 4; i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X + (32 * i), Player.Bottom.Y), new Vector2(4 + (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, 25, 0.25f);
                                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X - (32 * i), Player.Bottom.Y), new Vector2(-4 - (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, 25, 0.25f);
                                }
                                
                                for(int i = 0; i < 50; i++)
                                {
                                    d = Dust.NewDustPerfect(Player.Bottom + new Vector2(Main.rand.NextFloat(-125f, 125f), 0f), DustID.Corruption, new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-15f, 0f)), Scale: 1.5f);
                                    d.noGravity = true;
                                }
                                Player.velocity /= 2;
                                if (Math.Abs(ancientVelocity.Y) >= 5)
                                    Player.velocity.Y = -ancientVelocity.Y;
                                else
                                    Player.velocity.Y = -5;
                                Player.wingTime = Player.wingTimeMax;
                                activeAbility = 0;
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
                                        NPC.HitInfo hit = modifiers.ToHitInfo(100, false, 0f);
                                        npc.StrikeNPC(hit);
                                        npc.AddBuff(BuffID.Ichor, 240);
                                        npc.AddBuff(BuffID.Confused, 240);
                                        SoundEngine.PlaySound(PerforatorDashHit, npc.Center);
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
                            DisplayLocalizedText("Hive Mind Harvest Active!");
                            if (abilityCounter > 60)
                                activeAbility = 0;
                        }
                        #endregion
                        break;
                    case (int)AbilityIDS.Attack1:
                        #region Slime God Essence
                        if (abilityCounter % 5 == 0)
                        {
                            SoundEngine.PlaySound(SlimeGodShot);
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(Pi/10) * 15, ModContent.ProjectileType<AbyssBall>(), 25, 0.5f);
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
        }
        public override bool CanStartExtraJump(ExtraJump jump)
        {
            if (activeAbility == (int)AbilityIDS.Dash || activeAbility == (int)AbilityIDS.Attack1)
                return false;
            else
                return true;
        }
        public static bool HasGodlyEssence(Player player) => player.Godly().Evil1Essence || player.Godly().Evil2Essence || player.Godly().SlimeGodEssence;
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

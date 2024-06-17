using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
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
        public static readonly SoundStyle SlimeGodShot = new("CalamityMod/Sounds/Custom/SlimeGodShot1", 2);

        private int abilityCounter = 0;
        private int activeAbility = 0;
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
        private static List<Pair> dustArray = new List<Pair>();
        public override void PostUpdate()
        {
            #region Visual Effect Updates
            if (dustArray.Count > 0)
            {
                dustArray.RemoveAll(dp => dp.Dust == null || !dp.Dust.active || dp.Owner == null || !dp.Owner.active);
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
                if (WindfallKeybinds.GodlyDashHotkey.JustPressed)// && (Evil1Essence && !WorldGen.crimson) || (Evil2Essence && WorldGen.crimson))
                    activeAbility = (int)AbilityIDS.Dash;
                else if (WindfallKeybinds.GodlyHarvestHotkey.JustPressed)// && (Evil1Essence && WorldGen.crimson) || (Evil2Essence && !WorldGen.crimson))
                    activeAbility = (int)AbilityIDS.Harvest;
                else if (WindfallKeybinds.GodlyAttack1Hotkey.JustPressed)// && SlimeGodEssence)
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
                            if (abilityCounter == 0)
                            {
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

                            if(abilityCounter > 8)
                                Player.velocity = ancientVelocity * 32;
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
                            foreach(NPC npc in Main.npc.Where(n => n.active && !n.friendly && (n.Center - Player.Center).Length() <= 500))
                            {
                                if (abilityCounter % 6 == 0)
                                {
                                    var modifiers = new NPC.HitModifiers();
                                    NPC.HitInfo hit = modifiers.ToHitInfo(5, false, 0f);
                                    npc.StrikeNPC(hit);
                                }
                                Vector2 speed = (Player.Center - npc.Center).SafeNormalize(Vector2.Zero);
                                Dust d = Dust.NewDustPerfect(npc.Center, DustID.Ichor, speed * 12, Scale: 2f);
                                d.noGravity = true;
                                dustArray.Add(new Pair(d, npc));
                            }                           
                            
                            if (WindfallKeybinds.GodlyHarvestHotkey.JustReleased)
                            {
                                activeAbility = 0;
                                break;
                            }
                            else if(abilityCounter >= 120)
                            {
                                foreach (NPC npc in Main.npc.Where(n => n != null && n.active && !n.friendly && (n.Center - Player.Center).LengthSquared() <= 360000))
                                {
                                    var modifiers = new NPC.HitModifiers();
                                    NPC.HitInfo hit = modifiers.ToHitInfo(100, false, 0f);
                                    npc.StrikeNPC(hit);
                                    npc.AddBuff(BuffID.Ichor, 240);
                                    npc.AddBuff(BuffID.Confused, 240);
                                    SoundEngine.PlaySound(PerforatorDashHit, npc.Center);
                                    for (int i = 0; i < 50; i++)
                                    {
                                        Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.Ichor, speed * 5, Scale: 2f);
                                        d.noGravity = true;
                                        Dust b = Dust.NewDustPerfect(npc.Center, DustID.Blood, speed * 5, Scale: 2.5f);
                                        b.noGravity = true;
                                    }
                                }
                                Player.Heal(25);
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
    }
}

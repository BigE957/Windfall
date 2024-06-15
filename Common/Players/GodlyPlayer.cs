using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override void PostUpdate()
        {
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
                        if(WorldGen.crimson) //Perforators Essence
                        {
                            DisplayLocalizedText("Perforator Dash Active!");
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
                                ancientVelocity = (target.Center - Player.Center).SafeNormalize(Vector2.Zero);
                                Player.GiveUniversalIFrames(25);
                            }                         
                            olderVelocity = Player.velocity;

                            Player.velocity = ancientVelocity * 32;

                            DisplayLocalizedText($"{Player.velocity.Length()}");
                                
                            if (Player.Hitbox.Intersects(target.Hitbox) || abilityCounter > 15)
                            {
                                activeAbility = 0;
                                Player.velocity = olderVelocity.SafeNormalize(Vector2.Zero) * 16;
                                if (Player.Hitbox.Intersects(target.Hitbox))
                                {
                                    var modifiers = new NPC.HitModifiers();
                                    NPC.HitInfo hit = modifiers.ToHitInfo(100, false, 0f);
                                    target.StrikeNPC(hit);
                                    target.AddBuff(ModContent.BuffType<BurningBlood>(), 30);
                                    target.AddBuff(BuffID.Ichor, 60);
                                    Player.velocity.Y = -10;
                                }
                                target = null;                             
                            }
                        }
                        else //Eater of Worlds Essence
                        {
                            if (abilityCounter == 0)
                            {
                                Player.velocity += (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10;
                                if (Player.velocity.LengthSquared() <= 100)
                                    Player.velocity = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 10;
                            }
                            Player.wingTime = 0;
                            DisplayLocalizedText($"{ancientVelocity.Y}");
                            if (Player.oldVelocity.Y == Player.velocity.Y)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X + (32 * i), Player.Bottom.Y), new Vector2(4 + (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, 25, 0.25f);
                                    Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), new Vector2(Player.Center.X - (32 * i), Player.Bottom.Y), new Vector2(-4 - (i * 5), -20 + (i * 3)), ProjectileID.VilethornBase, 25, 0.25f);
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
                        break;
                    case (int)AbilityIDS.Harvest:
                        if (WorldGen.crimson) //Brain of Cthulhu Essence
                        {
                            DisplayLocalizedText("BoC Harvest Active!");
                            if (abilityCounter > 60)
                                activeAbility = 0;
                        }
                        else //Hive Mind Essence
                        {
                            DisplayLocalizedText("Hive Mind Harvest Active!");
                            if (abilityCounter > 60)
                                activeAbility = 0;
                        }
                        break;
                    case (int)AbilityIDS.Attack1: //Slime God Essence
                        if(abilityCounter % 5 == 0)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), Player.Center, (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(Pi/10) * 15, ModContent.ProjectileType<AbyssBall>(), 25, 0.5f);
                            Player.velocity -= (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero);
                            if(Player.velocity.LengthSquared() >= 400)
                                Player.velocity = Player.velocity.SafeNormalize(Vector2.Zero) * 20;
                        }
                        if (abilityCounter > 50)
                            activeAbility = 0;
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
    }
}

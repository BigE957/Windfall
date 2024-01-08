using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Events;
using Terraria.DataStructures;
using CalamityMod;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.Audio;
using CalamityMod.NPCs;
using Humanizer;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Dusts;
using Terraria.ModLoader.IO;
using WindfallAttempt1.Utilities;

namespace WindfallAttempt1.NPCs.WorldEvents
{
    public class WanderingCalClone : ModNPC
    {
        /// <summary>
        /// The main focus of this NPC is to show how to make something similar to the vanilla bone merchant;
        /// which means that the NPC will act like any other town NPC but won't have a happiness button, won't appear on the minimap,
        /// and will spawn like an enemy NPC. If you want a traditional town NPC instead, see <see cref="ExamplePerson"/>.
        /// </summary>
        private static Profiles.StackedNPCProfile NPCProfile;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

            Main.npcFrameCount[NPC.type] = 27;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
            NPCID.Sets.PrettySafe[Type] = 300;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
            NPCID.Sets.ShimmerTownTransform[NPC.type] = false; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

            //This sets entry is the most important part of this NPC. Since it is true, it tells the game that we want this NPC to act like a town NPC without ACTUALLY being one.
            //What that means is: the NPC will have the AI of a town NPC, will attack like a town NPC, and have a shop (or any other additional functionality if you wish) like a town NPC.
            //However, the NPC will not have their head displayed on the map, will de-spawn when no players are nearby or the world is closed, and will spawn like any other NPC.
            NPCID.Sets.ActsLikeTownNPC[Type] = true;

            // This prevents the happiness button
            NPCID.Sets.NoTownNPCHappiness[Type] = true;

            //To reiterate, since this NPC isn't technically a town NPC, we need to tell the game that we still want this NPC to have a custom/randomized name when they spawn.
            //In order to do this, we simply make this hook return true, which will make the game call the TownNPCName method when spawning the NPC to determine the NPC's name.
            NPCID.Sets.SpawnsWithCustomName[Type] = true;

            // Connects this NPC with a custom emote.
            // This makes it when the NPC is in the world, other NPCs will "talk about him".
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

            //The vanilla Bone Merchant cannot interact with doors (open or close them, specifically), but if you want your NPC to be able to interact with them despite this,
            //uncomment this line below.
            NPCID.Sets.AllowDoorInteraction[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.5f;

            AnimationType = NPCID.Guide;
        }

        //Make sure to allow your NPC to chat, since being "like a town NPC" doesn't automatically allow for chatting.
        public override bool CanChat()
        {
            return true;
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> {
            "Wandering Potion Seller",
        };
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //If any player is underground and has an example item in their inventory, the example bone merchant will have a slight chance to spawn.
            if (spawnInfo.Player.townNPCs > 2f && !DownedBossSystem.downedCalamitasClone && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<WanderingCalClone>()))
            {
                if (WorldSaveSystem.CloneRevealed)
                {
                    return 0.35f;
                }
                else
                {
                    return 0.1f;
                }
            }

            //Else, the example bone merchant will not spawn if the above conditions are not met.
            return 0f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (WorldSaveSystem.CloneRevealed)
            {
                base.OnSpawn(source);
                string key = "Calamitas is back...";
                Color messageColor = new Color(50, 125, 255);
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }
            else
            {
                base.OnSpawn(source);
                string key = "A Potion Seller has arrived!";
                Color messageColor = new Color(50, 125, 255);
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }
        }
        public override string GetChat()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();

            // These are things that the NPC has a chance of telling you when you talk to it.
            if (WorldSaveSystem.CloneRevealed)
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.StandardDialogue1").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.StandardDialogue2").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.StandardDialogue3").Value);
            }
            else
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.PotionSellerDialogue1").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.PotionSellerDialogue2").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.Dialogue.CalPotionSeller.PotionSellerDialogue3").Value);
            }
            
            return chat; // chat is implicitly cast to a string.
        }

        public override void SetChatButtons(ref string button, ref string button2)
        { // What the chat buttons are when you open up the chat UI
            button = Language.GetTextValue("LegacyInterface.28"); //This is the key to the word "Shop"
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                NPC.aiStyle = 0;
                WorldSaveSystem.CloneRevealed = true;
                Main.CloseNPCChatOrSign();
            }
        }
        public static readonly SoundStyle DashSound = new("CalamityMod/Sounds/Custom/SCalSounds/SCalDash");
        public static readonly SoundStyle Transformation = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneBigShoot");
        public static readonly SoundStyle CalCloneTeleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");

        internal int aiCounter = 0;
        int i = 20;
        float CalCloneHoverY;

        public override void AI()
        {
            if (Main.dayTime)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(CalCloneTeleport, NPC.Center);
                NPC.active = false;
            }
            if (NPC.aiStyle == 0)
            {
                aiCounter++;
                if (aiCounter == 1)
                {
                    string key = "Well...";
                    Color messageColor = Color.Orange;
                    CalamityUtils.DisplayLocalizedText(key, messageColor);
                }
                if (!WorldSaveSystem.CloneRevealed)
                {
                    if (aiCounter == 60)
                    {
                        string key = "TOO BAD!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 90)
                    {
                        string key = "The only thing you’re getting from me is PAIN… and… DEATH!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 150)
                    {
                        string key = "Cause I’m taking over this town! All your friends are gonna soon be MY FRIENDS!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 210)
                    {
                        i = 20;
                        SoundEngine.PlaySound(DashSound, NPC.Center);
                        string key = "MWAHAHAHAHA!!!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 240)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            int brimDust = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                            Main.dust[brimDust].velocity *= 3f;
                            if (Main.rand.NextBool())
                            {
                                Main.dust[brimDust].scale = 0.5f;
                                Main.dust[brimDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                            }
                        }
                        for (int j = 0; j < 70; j++)
                        {
                            int brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                            Main.dust[brimDust2].noGravity = true;
                            Main.dust[brimDust2].velocity *= 5f;
                            brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                            Main.dust[brimDust2].velocity *= 2f;
                        }
                        SoundEngine.PlaySound(Transformation, NPC.Center + new Vector2(0, +10));
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 20, (int)NPC.Center.Y + 50, ModContent.NPCType<CalamitasClone>());
                        NPC.active = false;
                    }
                    if (aiCounter >= 210)
                    {
                        if (i > 0)
                        {
                            NPC.position.Y -= i-- * 1.5f;
                            CalCloneHoverY = NPC.position.Y;
                        }
                        else
                        {
                            NPC.position.Y = CalCloneHoverY;
                        }
                    }
                }
                else
                {
                    if (aiCounter == 60)
                    {
                        string key = "Fine, I’ll just beat you again!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 90)
                    {
                        i = 20;
                        SoundEngine.PlaySound(DashSound, NPC.Center);
                        string key = "MWAHAHAHAHA!!!";
                        Color messageColor = Color.Orange;
                        CalamityUtils.DisplayLocalizedText(key, messageColor);
                    }
                    else if (aiCounter == 120)
                    {
                        for (int i = 0; i < 40; i++)
                        {
                            int brimDust = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                            Main.dust[brimDust].velocity *= 3f;
                            if (Main.rand.NextBool())
                            {
                                Main.dust[brimDust].scale = 0.5f;
                                Main.dust[brimDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                            }
                        }
                        for (int j = 0; j < 70; j++)
                        {
                            int brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                            Main.dust[brimDust2].noGravity = true;
                            Main.dust[brimDust2].velocity *= 5f;
                            brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                            Main.dust[brimDust2].velocity *= 2f;
                        }
                        SoundEngine.PlaySound(Transformation, NPC.Center + new Vector2(0, +10));
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 20, (int)NPC.Center.Y + 50, ModContent.NPCType<CalamitasClone>());
                        NPC.active = false;
                    }
                    if (aiCounter >= 90)
                    {
                        if (i > 0)
                        {
                            NPC.position.Y -= i-- * 1.5f;
                            CalCloneHoverY = NPC.position.Y;
                        }
                        else
                        {
                            NPC.position.Y = CalCloneHoverY;
                        }
                    }
                }
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            projType = (calamity.Find<ModProjectile>("SeethingDischargeBrimstoneHellblast").Type);
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 2f;
        }
    }
}
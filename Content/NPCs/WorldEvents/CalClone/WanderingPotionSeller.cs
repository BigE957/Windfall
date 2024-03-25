using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CalamityMod;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;

namespace Windfall.Content.NPCs.WorldEvents.CalClone
{
    public class WanderingPotionSeller : ModNPC
    {
        /// <summary>
        /// The main focus of this NPC is to show how to make something similar to the vanilla bone merchant;
        /// which means that the NPC will act like any other town NPC but won't have a happiness button, won't appear on the minimap,
        /// and will spawn like an enemy NPC. If you want a traditional town NPC instead, see <see cref="ExamplePerson"/>.
        /// </summary>
        private static Profiles.StackedNPCProfile NPCProfile;
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/WanderingPotionSeller";
        private static SoundStyle Jumpscare => new("Windfall/Assets/Sounds/NPCs/Jumpscare");

        // the time of day the traveler will spawn (double.MaxValue for no spawn). Saved and loaded with the world in TravelingMerchantSystem
        public static double spawnTime = double.MaxValue;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has
            NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
            NPCID.Sets.AttackFrameCount[NPC.type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
            NPCID.Sets.PrettySafe[Type] = 300;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
            NPCID.Sets.ShimmerTownTransform[NPC.type] = false; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.SpawnsWithCustomName[Type] = true;
            NPCID.Sets.AllowDoorInteraction[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 36;
            NPC.height = 52;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.5f;

            AnimationType = NPCID.Guide;
        }
        public override bool CanChat()
        {
            if(NPC.aiStyle == 0)
                return false;
            return true;
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //If any player is underground and has an example item in their inventory, the example bone merchant will have a slight chance to spawn.
            if (spawnInfo.Player.townNPCs > 2f && !DownedBossSystem.downedCalamitasClone && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !Main.dayTime && WorldSaveSystem.CloneRevealed && !NPC.AnyNPCs(ModContent.NPCType<WanderingPotionSeller>()) && !NPC.AnyNPCs(ModContent.NPCType<WanderingCalClone>()))
                return 0.1f;
            return 0f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            string key = "A Potion Seller has arrived!";
            Color messageColor = new(50, 125, 255);
            CalamityUtils.DisplayLocalizedText(key, messageColor);
        }
        public override bool PreAI()
        {
            if (Main.dayTime)
            {
                // Here we despawn the NPC and send a message stating that the NPC has despawned
                // LegacyMisc.35 is {0) has departed!
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(CalCloneTeleport, NPC.Center);
                string key = "A Potion Seller has departed!";
                Color messageColor = new(50, 125, 255);
                CalamityUtils.DisplayLocalizedText(key, messageColor);
                NPC.active = false;
                NPC.netSkip = -1;
                NPC.life = 0;
                return false;
            }
            return true;
        }
        public override string GetChat()
        {
            WeightedRandom<string> chat = new();

            // These are things that the NPC has a chance of telling you when you talk to it.
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.CalPotionSeller.PotionSeller1").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.CalPotionSeller.PotionSeller2").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.CalPotionSeller.PotionSeller3").Value);

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
                Main.CloseNPCChatOrSign();
            }
        }
        public static readonly SoundStyle CalCloneTeleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");

        private int aiCounter = 0;
        private float zoom = 0;
        public override void AI()
        {
            NPC.homeless = true;
            if (NPC.aiStyle == 0)
            {
                aiCounter++;
                if (aiCounter == 1)
                {
                    string key = "Well...";
                    Color messageColor = Color.Orange;
                    CalamityUtils.DisplayLocalizedText(key, messageColor);
                    zoom = 0;
                }
                else if (aiCounter < 120)
                {
                    
                    Vector2 LerpLocation = Vector2.Zero;
                    if (aiCounter < 100)
                        zoom = MathHelper.Lerp(zoom, 1, 0.075f);
                    else
                        zoom = 1;
                    ZoomSystem.SetZoomEffect(zoom);
                    Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = new(NPC.Center.X - 2, NPC.Center.Y - 8);
                    Main.LocalPlayer.Windfall_Camera().ScreenFocusInterpolant = zoom;
                }
                else if (aiCounter == 120)
                {
                    ZoomSystem.SetZoomEffect(50);
                    Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = NPC.Center;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(new Vector2(NPC.Center.X, NPC.Center.Y - 6), DustID.Blood, speed * 5, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(CalCloneTeleport, NPC.Center);
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X * NPC.spriteDirection, (int)NPC.Center.Y + 12, ModContent.NPCType<WanderingCalClone>(), 0, 1f);
                    SoundEngine.PlaySound(Jumpscare, NPC.Center);
                    NPC.active = false;
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
            projType = calamity.Find<ModProjectile>("SeethingDischargeBrimstoneHellblast").Type;
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 2f;
        }
        public static double GetRandomSpawnTime(double minTime, double maxTime)
        {
            // A simple formula to get a random time between two chosen times
            return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
        }
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return false;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;
        }
    }
}
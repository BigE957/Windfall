﻿using CalamityMod.Items.Accessories;
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
using Windfall.Common.Utilities;
using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Common.Systems;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class IlmeranPaladin : ModNPC
    {
        private static Profiles.StackedNPCProfile NPCProfile;
        public override string Texture => "Windfall/Assets/NPCs/WanderingNPCs/IlmeranPaladin";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25; // The amount of frames the NPC has

            NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs.
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
            NPCID.Sets.PrettySafe[Type] = 300;
            NPCID.Sets.AttackType[Type] = 0; // Throws a weapon
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

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
                Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, -1)
            //new Profiles.DefaultNPCProfile(Texture + "_Shimmer", -1)
            );
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
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.ai[1] = 0;
            AnimationType = NPCID.Guide;
            NPC.rarity = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.velocity = new Vector2(0, NPC.ai[0]);
            if (NPC.ai[1] != 0)
            {
                NPC.spriteDirection = NPC.direction = (int)NPC.ai[1] * -1;
            }
        }

        //Make sure to allow your NPC to chat, since being "like a town NPC" doesn't automatically allow for chatting.
        public override bool CanChat()
        {
            return true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			// Sets the preferred biomes of this town NPC listed in the bestiary.
			// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,

			// Sets your NPC's flavor text in the bestiary.
			new FlavorTextBestiaryInfoElement("One of few to have survived the Incineration of Ilmeris, this paladin wanders the remnants of his home; keeping an eternal vigil over his sacred home."),
        });
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // "Knocks out" the Ilmeran Paladin when the NPC is killed.
            if (Main.netMode != NetmodeID.Server && NPC.life <= 0)
            {
                NPC.life = NPC.lifeMax;
                NPC.velocity = new Vector2(0, 0);
                NPC.Transform(ModContent.NPCType<IlmeranPaladinKnocked>());
            }
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> {
            "Haakor",
            "Riley",
            "John",
        };
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert)
            {
                return 0.34f;
            }
            return 0f;
        }

        public override string GetChat()
        {
            WeightedRandom<string> chat = new();

            // These are things that the NPC has a chance of telling you when you talk to it.
            if (NPC.ai[1] == 1)
            {
                NPC.ai[1] = 0;
                return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.SavedDialogue").Value;
            }
            else if (Sandstorm.Happening)
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.SandstormDialogue").Value);
            else
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.NoSandstormDialogue").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.StandardDialogue1").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.StandardDialogue2").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.StandardDialogue3").Value);
            return chat; // chat is implicitly cast to a string.
        }

        public override void SetChatButtons(ref string button, ref string button2)
        { // What the chat buttons are when you open up the chat UI
            button = Language.GetTextValue("LegacyInterface.28"); //This is the key to the word "Shop"
            button2 = "Quest";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = "Shop";
            }
            else
            {
                Utilities.QuestDialogueHelper(Main.npc[NPC.whoAmI]);
            }
        }

        public override void AddShops()
        {
            new NPCShop(Type)
                .AddWithCustomValue<AmidiasSpark>(50000)
                .AddWithCustomValue<Cnidrisnack>(500)
                .AddWithCustomValue<AncientIlmeranRod>(10000, WindfallConditions.ScoogHunt1ActiveOrCompleted)
                .AddWithCustomValue<IlmeranHorn>(20000, WindfallConditions.ScoogHunt1Completed)
                .Register();
        }

        public override bool CheckActive()
        {
            if (NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 10;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<ScourgeoftheDesertProj>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }
    }
}
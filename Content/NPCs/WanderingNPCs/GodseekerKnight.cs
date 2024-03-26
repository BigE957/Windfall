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
using Windfall.Common.Utilities;
using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Common.Systems;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.NPCs.HiveMind;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class GodseekerKnight : ModNPC
    {
        private static Profiles.StackedNPCProfile NPCProfile;
        public override string Texture => "Windfall/Assets/NPCs/WanderingNPCs/GodseekerKnight";
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
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.SpawnsWithCustomName[Type] = true;

            // Connects this NPC with a custom emote.
            // This makes it when the NPC is in the world, other NPCs will "talk about him".
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();
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
            NPC.lifeMax = 400000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.rarity = 1;
            NPC.ai[1] = 0;
            AnimationType = NPCID.Guide;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.velocity = new Vector2(0, NPC.ai[0]);
            if (NPC.ai[1] != 0)
                NPC.spriteDirection = NPC.direction = (int)NPC.ai[1] * -1;
        }
        public override bool CanChat() => true;
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			// Sets the preferred biomes of this town NPC listed in the bestiary.
			// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Jungle,

			// Sets your NPC's flavor text in the bestiary.
			new FlavorTextBestiaryInfoElement((string)Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Bestiary")),
        });
        }

        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override List<string> SetNPCNameList() => new(){"Erahim"};

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if ((spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson) && NPC.downedBoss2)
            {
                return 0.34f;
            }
            return 0f;
        }

        public override string GetChat()
        {
            Player player = Main.player[Main.myPlayer];
            WeightedRandom<string> chat = new();
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Standard1").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Standard2").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Standard3").Value);
            if (!NPC.downedQueenSlime)
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.SlimeGod1").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.SlimeGod2").Value);
            }
            if(player.ZoneCorrupt || player.ZoneCrimson)
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Evilbiome1").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Evilbiome2").Value);
            }
            if (player.Calamity().statigelSet || (WearingStatigelHelmet(player) && player.armor[11].type == ModContent.ItemType<StatigelArmor>() && player.armor[12].type == ModContent.ItemType<StatigelGreaves>()))
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Statigel1").Value);
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.Statigel2").Value);
            }
            return chat;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.Buttons.{nameof(GodseekerKnight)}.Techniques").Value;
            button2 = Language.GetTextValue("LegacyInterface.64");
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.{nameof(GodseekerKnight)}.Chat.WorkingOnIt").Value;
            else
                Utilities.QuestDialogueHelper(Main.npc[NPC.whoAmI]);
        }

        public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<HiveMind>());
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 10;
            knockback = 2f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 20;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<CosmicKunaiProj>();
            attackDelay = 1;
        }
        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 2f;
        }
        private static bool WearingStatigelHelmet(Player player)
        {
            Item hat = player.armor[10];
            if (hat.type == ModContent.ItemType <StatigelHeadMagic>() || hat.type == ModContent.ItemType<StatigelHeadMelee>() || hat.type == ModContent.ItemType<StatigelHeadRanged>() || hat.type == ModContent.ItemType<StatigelHeadRogue>() || hat.type == ModContent.ItemType<StatigelHeadSummon>())
                return true;
            return false;
        }
    }
}
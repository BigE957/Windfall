using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Projectiles.Rogue;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Utils;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;

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
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.SpawnsWithCustomName[Type] = true;
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
                NPC.spriteDirection = NPC.direction = (int)NPC.ai[1] * -1;
        }
        public override bool CanChat() => true;
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			// Sets the preferred biomes of this town NPC listed in the bestiary.
			// With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,

			// Sets your NPC's flavor text in the bestiary.
			new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(IlmeranPaladin)}")),
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
        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override List<string> SetNPCNameList() => new() { "Nasser" };
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert)
                return 0.34f;
            return 0f;
        }
        public override string GetChat()
        {
            Player player = Main.player[Main.myPlayer];
            WeightedRandom<string> chat = new();
            if (WorldSaveSystem.IlmeranPaladinChats == 0)
                return GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.FirstChat");
            if (NPC.ai[1] == 1)
            {
                NPC.ai[1] = 0;
                return GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Saved");
            }
            if (Sandstorm.Happening)
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Sandstorm"));
            else
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.NoSandstorm"));
            if (player.Calamity().victideSet || (WearingVictideHelmet(player) && player.armor[11].type == ModContent.ItemType<VictideBreastplate>() && player.armor[12].type == ModContent.ItemType<VictideGreaves>()))
                if (player.armor[0].type == ModContent.ItemType<VictideHeadMagic>())
                {
                    chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.LookingFamiliar1"));
                    chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.LookingFamiliar2"));
                }
                else
                {
                    chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.LookingIlmeran1"));
                    chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.LookingIlmeran2"));
                }
            if (NPC.FindFirstNPC(ModContent.NPCType<SEAHOE>()) != -1)
            {
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Amidias1"));
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Amidias2"));
            }
            if (Main.dayTime)
            {
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Daytime1"));
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Daytime2"));
            }
            else
            {
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Nighttime1"));
                chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Nighttime2"));
            }
            chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Standard1"));
            chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Standard2"));
            chat.Add(GetWindfallTextValue($"Dialogue.IlmeranPaladin.Chat.Standard3"));
            WorldSaveSystem.IlmeranPaladinChats++;
            return chat;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28"); //This is the key to the word "Shop"
            button2 = Language.GetTextValue("LegacyInterface.64"); //This is the key to the word "Quest"
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
                shop = "Shop";
            else
                WindfallUtils.ProgressiveQuestDialogueHelper(Main.npc[NPC.whoAmI]);
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
        public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>());
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
        private static bool WearingVictideHelmet(Player player)
        {
            Item hat = player.armor[10];
            if (hat.type == ModContent.ItemType<VictideHeadMagic>() || hat.type == ModContent.ItemType<VictideHeadMelee>() || hat.type == ModContent.ItemType<VictideHeadRanged>() || hat.type == ModContent.ItemType<VictideHeadRogue>() || hat.type == ModContent.ItemType<VictideHeadSummon>())
                return true;
            return false;
        }
    }
}
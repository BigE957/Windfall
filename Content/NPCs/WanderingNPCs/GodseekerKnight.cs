using CalamityMod.Items.Armor.Statigel;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.Projectiles.Rogue;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;
using Windfall.Content.Items.Quests;

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
            NPCID.Sets.AllowDoorInteraction[Type] = true;

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

            GodlyEssenceIDs = new()
            {
                ModContent.ItemType<DeificInsignia>(),
            };
        }
        private static List<int> GodlyEssenceIDs = new List<int>();
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
            if (WorldSaveSystem.GodseekerKnightChats == 0)
                Dialogue = DialogueState.Initial1;
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
			new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(GodseekerKnight)}")),
        });
        }

        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override List<string> SetNPCNameList() => new() { "Erahim" };

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if ((spawnInfo.Player.ZoneCorrupt || spawnInfo.Player.ZoneCrimson) && NPC.downedBoss2)
                return 0.34f;
            return 0f;
        }

        private enum DialogueState
        {
            Neutral,

            #region First Time Chat
            Initial1,
            WhatUDoin,
            WhoAreU,
            Gods,
            War,
            Master,
            Knights,
            End1,
            #endregion

            #region Essences Chat
            Initial2,
            Effects,
            Cleansed,
            End2,
            #endregion
        }
        private DialogueState Dialogue;
        private string Topic = "";

        private readonly List<dialogueDirections> MyDialogue = new()
        {
            #region Initial Conversation
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial1,
                Button1 = new(){name = "What are you doing here?", heading = (int)DialogueState.WhatUDoin},
                Button2 = new(){name = "Who are you?", heading = (int)DialogueState.WhoAreU},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WhatUDoin,
                Button1 = new(){name = "Gods?", heading = (int)DialogueState.Gods},
                Button2 = new(){name = "What happened here?", heading = (int)DialogueState.War},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WhoAreU,
                Button1 = new(){name = "Godseeker?", heading = (int)DialogueState.Master},
                Button2 = new(){name = "Crusade?", heading = (int)DialogueState.War},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Gods,
                Button1 = new(){name = "Interesting...", heading = (int)DialogueState.End1},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.War,
                Button1 = new(){name = "Interesting...", heading = (int)DialogueState.End1},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Master,
                Button1 = new(){name = "Interesting...", heading = (int)DialogueState.End1},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Knights,
                Button1 = new(){name = "Interesting...", heading = (int)DialogueState.End1},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End1,
                Button1 = new(){name = "Oh, sorry...", heading = (int)DialogueState.Neutral, end = true},
                Button2 = new(){name = "Bye!", heading = (int)DialogueState.Neutral, end = true},
            },

            #endregion

            #region Godly Essence Conversation
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial2,
                Button1 = new(){name = "What kind of effects...?", heading = (int)DialogueState.Effects},
                Button2 = new(){name = "Can they be cleansed?", heading = (int)DialogueState.Cleansed},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Effects,
                Button1 = new(){name = "Freaky...", heading = (int)DialogueState.End2},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Cleansed,
                Button1 = new(){name = "That's good...", heading = (int)DialogueState.End2},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End2,
                Button1 = new(){name = "Will do!", heading = (int)DialogueState.Neutral, end = true},
                Button2 = new(){name = "That's your job...", heading = (int)DialogueState.Neutral, end = true},
            },
            #endregion
        };
        public override string GetChat()
        {
            Player player = Main.player[Main.myPlayer];
            if (Dialogue != DialogueState.Neutral)
            {
                if (WorldSaveSystem.GodseekerKnightChats == 0)
                    Topic = "JustMet";
                return GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Conversation.{Topic}.{Dialogue}");
            }
            WeightedRandom<string> chat = new();
            chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Standard1"));
            chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Standard2"));
            chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Standard3"));
            if (!NPC.downedQueenSlime)
            {
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.SlimeGod1"));
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.SlimeGod2"));
            }
            if (player.ZoneCorrupt || player.ZoneCrimson)
            {
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Evilbiome1"));
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Evilbiome2"));
            }
            if (player.Calamity().statigelSet || (WearingStatigelHelmet(player) && player.armor[11].type == ModContent.ItemType<StatigelArmor>() && player.armor[12].type == ModContent.ItemType<StatigelGreaves>()))
            {
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Statigel1"));
                chat.Add(GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Chat.Statigel2"));
            }
            WorldSaveSystem.GodseekerKnightChats++;
            return chat;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (Dialogue != DialogueState.Neutral)
                SetConversationButtons(MyDialogue, (int)Dialogue, ref button, ref button2);
            else
            {
                button = GetWindfallTextValue($"Dialogue.Buttons.{nameof(GodseekerKnight)}.Essence");
                button2 = Language.GetTextValue("LegacyInterface.64");
            }
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (Dialogue == DialogueState.Neutral)
            {
                Player player = Main.player[Main.myPlayer];
                if (firstButton)
                {
                    int GodlyEssenceIndex = HasGodlyEssence(player);
                    if (GodlyEssenceIndex != -1)
                    {
                        Item item = player.inventory[GodlyEssenceIndex];
                        switch (item.type)
                        {
                            //This will determine what reward the player gets for turning in a Godly Essence
                            default:
                                Item.NewItem(NPC.GetSource_GiftOrReward(), NPC.Center, Vector2.Zero, Main.hardMode ? ItemID.GoldenCrateHard : ItemID.GoldenCrate);
                                break;
                        }
                        item.stack -= 1;
                        WorldSaveSystem.EssenceCounter++;
                        Main.npcChatText = GetWindfallTextValue($"Dialogue.GodseekerKnight.Essence.Redeemed.{WorldSaveSystem.EssenceCounter}");
                    }
                    else
                    {
                        if (!WorldSaveSystem.EssenceExplained)
                        {
                            Topic = "Essence";
                            Dialogue = DialogueState.Initial2;
                            Main.npcChatText = GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Conversation.{Topic}.{Dialogue}");
                            WorldSaveSystem.EssenceExplained = true;
                        }
                        else

                            Main.npcChatText = GetWindfallTextValue($"Dialogue.GodseekerKnight.Essence.Hints.{WhoIsNext()}");
                    }
                }
                else
                    RandomizedQuestDialougeHelper(Main.npc[NPC.whoAmI]);
            }
            else
            {
                Dialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)Dialogue, firstButton);
                Main.npcChatText = GetWindfallTextValue($"Dialogue.{nameof(GodseekerKnight)}.Conversation.{Topic}.{Dialogue}");
            }
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
            if (hat.type == ModContent.ItemType<StatigelHeadMagic>() || hat.type == ModContent.ItemType<StatigelHeadMelee>() || hat.type == ModContent.ItemType<StatigelHeadRanged>() || hat.type == ModContent.ItemType<StatigelHeadRogue>() || hat.type == ModContent.ItemType<StatigelHeadSummon>())
                return true;
            return false;
        }      
        private static int HasGodlyEssence(Player player)
        {
            int i = 0;
            if (player != null)
                foreach (Item item in player.inventory)
                {
                    if (GodlyEssenceIDs.Contains(item.type))
                        return i;
                    i++;
                }
            return -1;
        }
        private static string WhoIsNext()
        {
            if (!NPC.downedBoss2)
                if (WorldGen.crimson)
                    return "BoC";
                else
                    return "EoW";
            else if (!DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator)
                if (WorldGen.crimson)
                    return "Perforators";
                else
                    return "HiveMind";
            else if (!DownedBossSystem.downedSlimeGod)
                return "SlimeGod";
            else if (!Main.hardMode)
                return "WallOfFlesh";
            else if (!NPC.downedQueenSlime)
                return "QueenSlime";
            else if (!NPC.downedEmpressOfLight)
                return "EoL";
            return "None";

        }
    }
}
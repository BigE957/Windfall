using CalamityMod;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using static Windfall.Common.Utilities.Utilities;

namespace Windfall.Content.NPCs.TravellingNPCs
{
    public class TravellingCultist : ModNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
        public override string HeadTexture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist_Head";

        public const double despawnTime = 48600.0;
        public static double spawnTime = double.MaxValue;
        private static Profiles.StackedNPCProfile NPCProfile;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = 1 
        };

        public override bool PreAI()
        {
            if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
            {
                // Here we despawn the NPC and send a message stating that the NPC has despawned
                CalamityUtils.DisplayLocalizedText("The Strange Cultist has departed!", new(50, 125, 255));
                NPC.active = false;
                NPC.netSkip = -1;
                NPC.life = 0;
                return false;
            }
            return true;
        }
        public static void UpdateTravelingMerchant()
        {
            bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>()) != -1);

            if (Main.dayTime && Main.time == 0)
                if (!travelerIsThere && (Main.rand.NextBool(4) || WorldSaveSystem.PlanteraJustDowned))
                {
                    spawnTime = GetRandomSpawnTime(5400, 8100);
                    WorldSaveSystem.PlanteraJustDowned = false;
                }
                else
                    spawnTime = double.MaxValue;
                
            if (!travelerIsThere && CanSpawnNow())
            {
                int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<TravellingCultist>(), 1); // Spawning at the world spawn
                NPC traveler = Main.npc[newTraveler];
                traveler.homeless = true;
                traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
                traveler.netUpdate = true;

                // Prevents the traveler from spawning again the same day
                spawnTime = double.MaxValue;

                string key = ("A " + traveler.FullName + " has arrived!");
                Color messageColor = new(50, 125, 255);
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }
        }
        private static bool CanSpawnNow()
        {
            // can't spawn if any events are running
            if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
                return false;

            // can't spawn if the sundial is active
            if (Main.IsFastForwardingTime())
                return false;
            //progression locks
            if (!Main.hardMode || !NPC.downedBoss3 || NPC.downedAncientCultist)
                return false;
            // can spawn if daytime, and between the spawn and despawn times
            return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
        }
        public override void OnSpawn(IEntitySource source)
        {
            QuestComplete = false;
            QuestArtifact = new(0, 0);
        }
        private static bool IsNpcOnscreen(Vector2 center)
        {
            int w = NPC.sWidth + NPC.safeRangeX * 2;
            int h = NPC.sHeight + NPC.safeRangeY * 2;
            Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
            foreach (Player player in Main.player)
                if (player.active && player.getRect().Intersects(npcScreenRect))
                    return true;
            return false;
        }
        public static double GetRandomSpawnTime(double minTime, double maxTime) => (maxTime - minTime) * Main.rand.NextDouble() + minTime;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 60;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 12;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPCID.Sets.NoTownNPCHappiness[Type] = true; 
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TravellingCultist>();

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f, 
                Direction = 1 
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture))
            );
        }
        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Stylist;
            TownNPCStayingHomeless = true;
            NPC.immortal = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

			new FlavorTextBestiaryInfoElement("A strange fellow who's recently begun showing up out of nowhere. He claims to want to fight against the Lunar Cult, but can he really be trusted...?"),
        });
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;
        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override List<string> SetNPCNameList()
        {
            return new List<string>() {
                "Strange Cultist",
                "Poorly Disguised Cultist",
                "Suspicious Looking Cultist",
            };
        }
        public static QuestItem QuestArtifact = new(0,0);
        public static bool QuestComplete = false;
        public static int RitualQuestProgress = 0;
        private enum DialogueState
        {
            Initial,
            Weird,
            ThatsYou,
            TheCult,
            HowYouKnow,
            HowToHelp,
            ImIn,
            Quests1,
            //post-plantera convo
            Initial2,
            Orator,
            Meeting,
            HelpThem,
            HowCanI,
            Doneish,
            Tablet,
            Thanks,
            Quests2,
            //All recruited dialogue
            Initial3,
            Sealing,
            UhOh,
            Dragons,
            Fovos,
            Why,
            Quests3,
            //Ritual Dialogue
            RitualTime,
        }
        private static DialogueState CurrentDialogue
        {
            get => (DialogueState)WorldSaveSystem.cultistChatState;
            set => WorldSaveSystem.cultistChatState = (int)value;
        }
        public override string GetChat()
        {
            if (CurrentDialogue != DialogueState.Quests1 || CurrentDialogue != DialogueState.Quests2)
                return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Conversation.{CurrentDialogue}").Value;
            WeightedRandom<string> chat = new();

            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Standard1").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Standard2").Value);
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Standard3").Value);
            if (NPC.AnyNPCs(NPCID.Mechanic) || NPC.AnyNPCs(NPCID.Clothier))
            {
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.AnyDungeonNPC").Value);
                if (NPC.AnyNPCs(NPCID.Mechanic))
                    chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Mechanic").Value);
                if (NPC.AnyNPCs(NPCID.Clothier))
                    chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Clothier").Value);
            }
            if (NPC.downedBoss3)
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.Skeletron").Value);
            for (int i = 0; i < CultMeetingSystem.Recruits.Count; i++)
                chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Chat.{(RecruitableLunarCultist.RecruitNames)i}").Value);
            return chat;
        }
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            #region Initial Conversation
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "That's me!", heading = (int)DialogueState.ThatsYou},
                Button2 = new(){name = "You look... odd...", heading = (int)DialogueState.Weird},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Weird,
                Button1 = new(){name = "That's me!", heading = (int)DialogueState.ThatsYou},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ThatsYou,
                Button1 = new(){name = "What's the problem?", heading = (int)DialogueState.TheCult},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.TheCult,
                Button1 = new(){name = "How can I help?", heading = (int)DialogueState.HowToHelp},
                Button2 = new(){name = "How do you know?", heading = (int)DialogueState.HowYouKnow},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.HowYouKnow,
                Button1 = new(){name = "How can I help?", heading = (int)DialogueState.HowToHelp},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.HowToHelp,
                Button1 = new(){name = "How can I help?", heading = (int)DialogueState.ImIn},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ImIn,
                Button1 = new(){name = "Will do!", heading = (int)DialogueState.Quests1, end = true},
                Button2 = null,
            },
            #endregion

            #region Post-Plantera Conversation
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial2,
                Button1 = new(){name = "Meetings?", heading = (int)DialogueState.Meeting},
                Button2 = new(){name = "The Orator?", heading = (int)DialogueState.Orator},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Meeting,
                Button1 = new(){name = "Can we help them?", heading = (int)DialogueState.HelpThem},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Orator,
                Button1 = new(){name = "Can we help them?", heading = (int)DialogueState.HelpThem},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.HelpThem,
                Button1 = new(){name = "How would I do that?", heading = (int)DialogueState.HowCanI},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.HowCanI,
                Button1 = new(){name = "I see...", heading = (int)DialogueState.Doneish},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Doneish,
                Button1 = new(){name = "Oh, this tablet...?", heading = (int)DialogueState.Tablet},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Tablet,
                Button1 = new(){name = "Thanks!", heading = (int)DialogueState.Thanks},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Thanks,
                Button1 = new(){name = "Bye!", heading = (int)DialogueState.Quests2, end = true},
                Button2 = null,
            },
            #endregion

            #region All Recruited Conversation
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial3,
                Button1 = new(){name = "Meetings?", heading = (int)DialogueState.Meeting},
                Button2 = new(){name = "The Orator?", heading = (int)DialogueState.Orator},
            }
            #endregion
        };
        public override void SetChatButtons(ref string button, ref string button2)
        {
            if(CurrentDialogue == DialogueState.Quests1 || CurrentDialogue == DialogueState.Quests2)
                button = Language.GetTextValue("LegacyInterface.64");
            else
                SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton && CurrentDialogue.ToString().Contains("Quests"))
            {
                if (CurrentDialogue == DialogueState.Quests3)
                {
                    if (QuestArtifact.Stack == 0)
                        QuestArtifact = QuestSystem.RitualQuestItems[RitualQuestProgress];
                    QuestArtifact = CollectorQuestDialogueHelper(Main.npc[NPC.whoAmI], ref QuestComplete, QuestArtifact, QuestSystem.RitualQuestItems);
                    if (QuestArtifact.Stack == 0)
                        RitualQuestProgress++;
                }
                else
                    QuestArtifact = CollectorQuestDialogueHelper(Main.npc[NPC.whoAmI], ref QuestComplete, QuestArtifact, QuestSystem.DungeonQuestItems);
                return;
            }
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Conversation.{CurrentDialogue}").Value;
        }
        public override void AI()
        {
            NPC.homeless = true;
            if (CurrentDialogue == DialogueState.Quests1 && NPC.downedPlantBoss)
                CurrentDialogue = DialogueState.Initial2;
            if (RitualQuestProgress > 2 && CurrentDialogue == DialogueState.Quests3)
                CurrentDialogue = DialogueState.RitualTime;
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 15;
            randExtraCooldown = 8;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<PhantasmalFuryProj>();
            attackDelay = 1;
        }
    }
}
